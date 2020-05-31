using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.RemoteInterfaces;
using System.Diagnostics;
using System.Runtime.Remoting.Lifetime;
using System.Collections;

namespace Galaxy.Tools
{
    public class RemoteControllerSponsor : MarshalByRefObject, ISponsor
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public TimeSpan Renewal(ILease lease)
        {
            // Extend 10 minutes
            return TimeSpan.FromMinutes(10) ;
        }
    }

    public class RemoteController : MarshalByRefObject, IRemoteController
    {
        #region Variables
        private string m_strCurWorkingDir;
        private int m_iPort;
        private string m_strMachineName;

        private OutputCmdInfoHandler m_outputCmdInfoHandlers;
        private CmdFinishHandler m_cmdFinishHandlers;
        private int m_iOutputLineCount;
        private string m_strLastOutputLine;
        #endregion

        #region Properties

        public string WorkingDir
        {
            get { return m_strCurWorkingDir; }
        }

        public int Port
        {
            get { return m_iPort; }
            set { m_iPort = value; }
        }

        public string MachineName
        {
            get { return m_strMachineName; }
            set { m_strMachineName = value; }
        }
        #endregion

        public RemoteController()
        {
            m_strCurWorkingDir = "c:\\";
        }

        public override object InitializeLifetimeService()
        {
            ILease lease = base.InitializeLifetimeService() as ILease;
            if (LeaseState.Initial == lease.CurrentState)
            {
                lease.InitialLeaseTime = TimeSpan.FromMinutes(10);
                lease.SponsorshipTimeout = TimeSpan.FromMinutes(10);
                lease.RenewOnCallTime = TimeSpan.FromMinutes(5);
            }

            return lease;
        }

        #region IRemoteController interface

        public int AddCmdOutputEventSinker(OutputCmdInfoHandler outputCmdInfo)
        {
            m_outputCmdInfoHandlers += outputCmdInfo;
            return 0;
        }

        public int AddCmdFinishEventSinker(CmdFinishHandler cmdFinish)
        {
            m_cmdFinishHandlers += cmdFinish;
            return 0;
        }

        public void IsLive()
        {
            // Do nothing
        }

        public bool RunRemoteCommand(string strCmdLine, out string strProcessDumpInfo)
        {
            bool fRet = true;
            strProcessDumpInfo = "";
            try
            {
                m_iOutputLineCount = 0;
                m_strLastOutputLine = null;

                // Start the process
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.WorkingDirectory = m_strCurWorkingDir;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(Process_ErrorDataReceived);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Write the command
                process.StandardInput.WriteLine(strCmdLine);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();

                m_strCurWorkingDir = m_strLastOutputLine.Substring(0, m_strLastOutputLine.Length - 1);

                if (m_cmdFinishHandlers != null)
                {
                    Delegate[] invokeList = m_cmdFinishHandlers.GetInvocationList();
                    IEnumerator ie = invokeList.GetEnumerator();
                    while (ie.MoveNext())
                    {
                        CmdFinishHandler cmdFinish = ie.Current as CmdFinishHandler;
                        try
                        {
                            cmdFinish.Invoke();
                        }
                        catch (Exception)
                        {
                            // m_cmdFinishHandlers -= cmdFinish;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                strProcessDumpInfo = e.Message;
                fRet = false;
            }

            return fRet;
        }

        void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string strErrorLine = e.Data;
            // Invoke the output handlers
            if (m_outputCmdInfoHandlers != null)
            {
                Delegate[] invokeList = m_outputCmdInfoHandlers.GetInvocationList();
                IEnumerator ie = invokeList.GetEnumerator();
                while (ie.MoveNext())
                {
                    OutputCmdInfoHandler outputCmdInfo = ie.Current as OutputCmdInfoHandler;
                    try
                    {
                        outputCmdInfo.Invoke(strErrorLine);
                    }
                    catch (Exception)
                    {
                        // m_outputCmdInfoHandlers -= outputCmdInfo;
                    }
                }
            }
        }

        void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            m_iOutputLineCount++;
            // Neglect the first 4 lines
            // "Microsft copyright.
            //  xxxxx
            //  <blank line>
            // c:\><command>"
            if (m_iOutputLineCount > 4)
            {
                string strOutputLine = e.Data;
                // We output the privious line instead of current one, because we need to strip the last line which is the current directory
                if (strOutputLine != null)
                {
                    // Invoke the output handlers
                    if (m_outputCmdInfoHandlers != null)
                    {
                        Delegate[] invokeList = m_outputCmdInfoHandlers.GetInvocationList();
                        IEnumerator ie = invokeList.GetEnumerator();
                        while (ie.MoveNext())
                        {
                            OutputCmdInfoHandler outputCmdInfo = ie.Current as OutputCmdInfoHandler;
                            try
                            {
                                outputCmdInfo.Invoke(m_strLastOutputLine);
                            }
                            catch (Exception)
                            {
                                // m_outputCmdInfoHandlers -= outputCmdInfo;
                            }
                        }
                    }

                    m_strLastOutputLine = strOutputLine;
                }
            }
        }

        #endregion
    }
}
