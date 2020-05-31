using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.RemoteInterfaces;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;

namespace COMClientLib
{
    [ComVisible(true)]
    [Guid("050F7540-8F37-4ba1-8BD5-436561C7D9E4")]
    public interface IJobStatusHelper
    {
        bool Initialize(int iPortNumber);

        /// <returns>
        ///     -1 - something wrong
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        int SetJobProperty(string strPropName, string strPropValue);

        /// <returns>
        ///     -1 - something wrong
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        int SetJobFinishProperty(bool fSuccessful);
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("7F3D6018-52EA-4f8d-8F30-72024604C101")]
    public class JobStatusHelper : IJobStatusHelper
    {
        #region Variables
        private IJobStatusManager m_jobStatusManager = null;
        #endregion

        public JobStatusHelper()
        {
        }

        #region IJobStatusHelper
        public bool Initialize(int iPortNumber)
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
                m_jobStatusManager = Activator.GetObject(
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

        public int SetJobProperty(string strPropName, string strPropValue)
        {
            int iRet = 0;
            if (m_jobStatusManager == null) { return -1; }
            try
            {
                int iProcessId = Process.GetCurrentProcess().Id;
                if (m_jobStatusManager.SetJobProperty(iProcessId, strPropName, strPropValue) != 0)
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

        public int SetJobFinishProperty(bool fSuccessful)
        {
            int iRet = 0;
            if (fSuccessful)
            {
                iRet = SetJobProperty(GalaxyJobProperties.ms_strJobStatusPropName, GalaxyJobProperties.ms_strPropValueOfSucJob);
            }
            else
            {
                iRet = SetJobProperty(GalaxyJobProperties.ms_strJobStatusPropName, GalaxyJobProperties.ms_strPropValueOfFailedJob);
            }

            return iRet;
        }

        #endregion
    }
}
