using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using Galaxy.RemoteInterfaces;
using System.Diagnostics;

namespace Galaxy.ManagedClientLib
{
    public class JobStatusHelper
    {
        #region Variables
        private static IJobStatusManager ms_jobStatusManager = null;
        #endregion

        #region Initializer
        public static bool Initialize(int iPortNumber)
        {
            try
            {
                TcpChannel tcpChannel = new TcpChannel();
                ChannelServices.RegisterChannel(tcpChannel, true);
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
                // We have already registered a channel
            }
            catch (Exception)
            {
                // Other error
                return false;
            }
            try
            {
                ms_jobStatusManager = Activator.GetObject(
                    typeof(IJobStatusManager),
                    // The remote object should be at the same machine
                    "tcp://localhost:" + iPortNumber.ToString() + "/GalaxyJobStatusManager") as IJobStatusManager;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Functions

        /// <returns>
        ///     -1 - something wrong
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        public static int SetJobProperty(string strPropName, string strPropValue)
        {
            int iRet = 0;
            if (ms_jobStatusManager == null) { return -1; }
            try
            {
                int iProcessId = Process.GetCurrentProcess().Id;
                if (ms_jobStatusManager.SetJobProperty(iProcessId, strPropName, strPropValue) != 0)
                {
                    iRet = -1;
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                iRet = 1;
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }

        /// <returns>
        ///     -1 - something wrong
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        public static int GetJobProperties(int iProcessId, out GalaxyJobProperties jobProperties)
        {
            int iRet = 0;
            jobProperties = null;
            if (ms_jobStatusManager == null) { return -1; }
            try
            {
                if (ms_jobStatusManager.GetJobProperties(iProcessId, out jobProperties) != 0)
                {
                    iRet = -1;
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                iRet = 1;
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }

        /// <returns>
        ///     -1 - something wrong
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        public static int GetJobCount(out int iJobCount)
        {
            int iRet = 0;
            iJobCount = -1;
            if (ms_jobStatusManager == null) { return -1; }
            try
            {
                iJobCount = ms_jobStatusManager.GetJobCount();
            }
            catch (System.Net.Sockets.SocketException)
            {
                iRet = 1;
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }

        /// <returns>
        ///     -1 - something wrong
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        public static int RemoveJob(int iProcessId)
        {
            int iRet = 0;
            if (ms_jobStatusManager == null) { return -1; }
            try
            {
                if (ms_jobStatusManager.RemoveJob(iProcessId) != 0)
                {
                    iRet = -1;
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                iRet = 1;
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }

        /// <returns>
        ///     -1 - something wrong
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        public static int ClearJobs()
        {
            int iRet = 0;
            if (ms_jobStatusManager == null) { return -1; }
            try
            {
                ms_jobStatusManager.ClearJobs();
            }
            catch (System.Net.Sockets.SocketException)
            {
                iRet = 1;
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }
        #endregion
    }
}
