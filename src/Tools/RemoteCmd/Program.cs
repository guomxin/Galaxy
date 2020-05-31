using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Activation;
using Galaxy.RemoteInterfaces;
using Galaxy.Tools;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Configuration;
using System.Net.Sockets;
using System.IO;

using Galaxy.ManagedClientLib;
using System.Threading;

namespace RemoteCmd
{
    public class Program
    {
        #region Variables
        private RemoteControllerSponsor m_sponsor;
        IDictionary<string, RemoteController> m_remoteControllerDict;

        private CmdOutputEventSinker m_cmdOutputEventSinker;
        private AutoResetEvent m_waitingOutputFinishEvent;

        private static int ms_iCheckTime = 2 * 60 * 1000; // 2 mins
        private int m_iCheckTimes;
        private RemoteController m_curRemoteController;
        private string m_strCurRemoteControllerName;

        private List<string> m_commandList;
        #endregion

        static void PrintUsage()
        {
            Console.WriteLine("RemoteCmd.exe -f <command_file>");
            Console.WriteLine("RemoteCmd.exe -c <commands (command1&command2&...)>");
            Console.WriteLine("RemoteCmd.exe");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            /*
             * RemoteCmd.exe -f <command_file>
             * RemoteCmd.exe -c <commands (command1&command2&...)>
            */
            Console.WriteLine("Remote Command [Version 1.0.1213]");
            Console.WriteLine("(C) Copyright 2006 Microsoft Research Asia, Web Search and Mining Group.");
            Console.WriteLine();

            List<string> commandList = new List<string>() ;

            if (args.Length == 0)
            {
                // Do nothing
            }
            else if (args.Length == 2)
            {
                string strFuncSpec = args[0].ToLower();
                if (strFuncSpec == "-f")
                {
                    string strCommandFileName = args[1];
                    if (File.Exists(strCommandFileName))
                    {
                        // Read the commands from the file
                        StreamReader commandFile = new StreamReader(strCommandFileName);
                        string strCommand = null;
                        while ((strCommand = commandFile.ReadLine()) != null)
                        {
                            commandList.Add(strCommand);
                        }
                        commandFile.Close();
                    }
                }
                else if (strFuncSpec == "-c")
                {
                    string strCommands = args[1];
                    string[] rgCommands = strCommands.Split(new char[] { '&' });
                    for (int i = 0; i < rgCommands.Length; i++)
                    {
                        commandList.Add(rgCommands[i]);
                    }
                }
                else
                {
                    PrintUsage();
                    return;
                }
            }
            else
            {
                PrintUsage();
                return;
            }

            // Run
            Program remoteCmd = new Program();
            remoteCmd.Run(commandList);
        }

        private void MonitorCurRemoteController(object objPara)
        {
            if (m_curRemoteController != null)
            {
                try
                {
                    m_curRemoteController.IsLive();
                }
                catch (Exception)
                {
                    m_iCheckTimes++;
                    if (m_iCheckTimes >= 2)
                    {
                        // We have lost the connection to the remote controller for at least 2 mins
                        m_curRemoteController = null;
                        m_remoteControllerDict.Remove(m_strCurRemoteControllerName);
                        Console.WriteLine("Lost connection to the machine!");
                        m_waitingOutputFinishEvent.Set();
                    }
                }
            }
        }

        public void Run(List<string> commandList)
        {
            m_commandList = commandList;
            
            // Read the configuration items
            int iPortNumber = Int32.Parse(ConfigurationManager.AppSettings["Port"]);
            // Register the channel for the sponsor
            try
            {
                BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
                serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();
                IDictionary props = new Hashtable();
                props["port"] = iPortNumber;
                TcpChannel tcpChannel = new TcpChannel(props, clientProv, serverProv);
                ChannelServices.RegisterChannel(tcpChannel, true);
            }
            catch (RemotingException)
            {
                Console.WriteLine("The channel tcp:" + iPortNumber.ToString() + " is already existed!");
                return;
            }
            catch (Exception)
            {
                Console.WriteLine("Register channel tcp:" + iPortNumber.ToString() + " error!");
                return;
            }

            // Initialize the variables
            m_sponsor = new RemoteControllerSponsor();
            m_remoteControllerDict = new Dictionary<string, RemoteController>();
            m_cmdOutputEventSinker = new CmdOutputEventSinker();
            m_cmdOutputEventSinker.OutputCmdInfoCallBack += new OutputCmdInfoHandler(this.OutputCmdInfo);
            m_cmdOutputEventSinker.CmdFinishCallBack += new CmdFinishHandler(this.CmdFinish);
            m_waitingOutputFinishEvent = new AutoResetEvent(false);
            TimerCallback timerCallBack = new TimerCallback(this.MonitorCurRemoteController);
            Timer timer = new Timer(timerCallBack, null, ms_iCheckTime, ms_iCheckTime);

            // Run the commands
            Console.Write(">");
            string strCommand = null;
            strCommand = GetCommand();
            m_curRemoteController = null;
            while (strCommand.ToLower() != "exit")
            {
                m_iCheckTimes = 0;
                // Parse the command
                bool fCmdStatus = true;
                if ((strCommand.Length > 3) && (strCommand.Substring(0, 3) == "use"))
                {
                    // Parse the machine and port
                    string strMachineName = "";
                    int iRCPort = -1;
                    string strCmdRemain = strCommand.Substring(3).Trim();
                    int iQuesMarkPos = strCmdRemain.IndexOf(":");
                    if (iQuesMarkPos == -1)
                    {
                        fCmdStatus = false;
                    }
                    else
                    {
                        strMachineName = strCmdRemain.Substring(0, iQuesMarkPos);
                        try
                        {
                            iRCPort = Int32.Parse(strCmdRemain.Substring(iQuesMarkPos + 1));
                        }
                        catch (Exception)
                        {
                            fCmdStatus = false;
                        }
                    }

                    // Execute the command
                    if (!fCmdStatus)
                    {
                        Console.WriteLine("Command is error.[ using machinename:port ]");
                    }
                    else
                    {
                        RemoteController remoteController = GetRemoteController(strMachineName, iRCPort);
                        if (remoteController == null)
                        {
                            Console.WriteLine("Failed to connect to " + strMachineName + ":" + iRCPort.ToString() + ".");
                        }
                        else
                        {
                            m_curRemoteController = remoteController;
                            m_strCurRemoteControllerName = strMachineName;
                            Console.WriteLine("Connect to " + strMachineName + ":" + iRCPort.ToString() + " successfully.");
                        }
                    }
                }
                else
                {
                    // Other commands (e.g. dir)
                    if (m_curRemoteController == null)
                    {
                        Console.WriteLine("Use < using machinename:port > to select a machine.");
                    }
                    else
                    {
                        m_waitingOutputFinishEvent.Reset();
                        string strOutputInfo;
                        RunRemoteCommandHandler runRemoteCommand = new RunRemoteCommandHandler(m_curRemoteController.RunRemoteCommand);
                        runRemoteCommand.BeginInvoke(strCommand, out strOutputInfo, null, null);
                        m_waitingOutputFinishEvent.WaitOne();
                    }
                }

                if (m_curRemoteController != null)
                {
                    string strPrompt = m_curRemoteController.MachineName + ":" + m_curRemoteController.Port.ToString() + ">>" + m_curRemoteController.WorkingDir;
                    Console.Write(strPrompt);
                }
                Console.Write(">");
                strCommand = GetCommand();
            }
        }

        #region Util functions

        private void OutputCmdInfo(string strCmdInfo)
        {
            if (strCmdInfo != null)
            {
                Console.WriteLine(strCmdInfo);
            }
        }

        private void CmdFinish()
        {
           m_waitingOutputFinishEvent.Set();
        }

        private string GetCommand()
        {
            string strCommand = null;
            if ((m_commandList != null) && (m_commandList.Count > 0))
            {
                strCommand = m_commandList[0];
                m_commandList.Remove(strCommand);
                // Have read the command from the command file
                Console.WriteLine(strCommand);
            }
            else
            {
                strCommand = Console.ReadLine();
            }

            return strCommand;
        }

        /// <returns></returns>
        private RemoteController CreateRemoteController(string strMachineName, int iPortNumber)
        {
            RemoteController remoteController = null;
            try
            {
                string strRemoteObjUrl = "tcp://" + strMachineName + ":" + iPortNumber.ToString() + "/GalaxyRemoteController";
                UrlAttribute urlAttr = new UrlAttribute(strRemoteObjUrl);
                object[] rgAct = { urlAttr };
                remoteController = Activator.CreateInstance(Type.GetType("Galaxy.Tools.RemoteController, RemoteImpLib"), null, rgAct) as RemoteController;
                remoteController.AddCmdOutputEventSinker(m_cmdOutputEventSinker.OnCmdOutput);
                remoteController.AddCmdFinishEventSinker(m_cmdOutputEventSinker.OnCmdFinish);
                remoteController.IsLive();

            }
            catch (Exception e)
            {
                // The remote server is offline
                remoteController = null;
                Console.WriteLine(e.Message);
            }

            // Register the lease sponsor
            if (remoteController != null)
            {
                remoteController.Port = iPortNumber;
                remoteController.MachineName = strMachineName;
                ILease lease = RemotingServices.GetLifetimeService(remoteController) as ILease;
                lease.Register(m_sponsor);
            }

            return remoteController;
        }

        /// <returns>
        ///     null - something wrong
        /// </returns>
        private RemoteController GetRemoteController(string strMachineName, int iPortNumber)
        {
            RemoteController remoteController = null;
            if (m_remoteControllerDict.ContainsKey(strMachineName))
            {
                // We have cached the remote controller
                remoteController = m_remoteControllerDict[strMachineName];
                try
                {
                    remoteController.IsLive();
                }
                catch (Exception)
                {
                    // The cached remote controller has something wrong, we need to create a new one
                    remoteController = null;
                }
                if (remoteController == null)
                {
                    remoteController = CreateRemoteController(strMachineName, iPortNumber);
                    if (remoteController == null)
                    {
                        // Remove the key
                        m_remoteControllerDict.Remove(strMachineName);
                    }
                    else
                    {
                        // Update the key
                        m_remoteControllerDict[strMachineName] = remoteController;
                    }
                }
            }
            else
            {
                // We need to create a remote controller
                remoteController = CreateRemoteController(strMachineName, iPortNumber);
                if (remoteController != null)
                {
                    // Add a new item to the dict
                    m_remoteControllerDict.Add(strMachineName, remoteController);
                }
            }

            return remoteController;
        }

        #endregion
    }
}
