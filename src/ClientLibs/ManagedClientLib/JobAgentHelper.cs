using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using Galaxy.RemoteInterfaces;
using System.IO;

namespace Galaxy.ManagedClientLib
{
    public class JobAgentHelper
    {
        #region Variables
        private IJobAgent m_jobAgent = null;
        #endregion

        #region Initializer and Constructors

        public JobAgentHelper()
        {
            m_jobAgent = null;
        }

        public bool Initialize(string strProcessNodeName, int iPortNumber)
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
                m_jobAgent = Activator.GetObject(
                    typeof(IJobAgent),
                    "tcp://" + strProcessNodeName + ":" + iPortNumber.ToString() + "/GalaxyJobAgent") as IJobAgent;
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
        public int AddJobStartListener(GalaxyJobStartEventHandler jobStartEventHandler)
        {
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                if (m_jobAgent.AddJobStartEventSinker(jobStartEventHandler) != 0)
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
        public int AddJobFinishListener(GalaxyJobFinishEventHandler jobFinishEventHandler)
        {
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                if (m_jobAgent.AddJobFinishEventSinker(jobFinishEventHandler) != 0)
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
        public int RemoveJobStartListener(GalaxyJobStartEventHandler jobStartEventHandler)
        {
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                if (m_jobAgent.RemoveJobStartEventSinker(jobStartEventHandler) != 0)
                {
                    return -1;
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
        public int RemoveJobFinishListener(GalaxyJobFinishEventHandler jobFinishEventHandler)
        {
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                if (m_jobAgent.RemoveJobFinishEventSinker(jobFinishEventHandler) != 0)
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
        public int GetDebugInfo(out string strDebugInfo)
        {
            int iRet = 0;
            strDebugInfo = "";
            if (m_jobAgent == null) { return -1; }
            try
            {
                strDebugInfo = m_jobAgent.GetDebugInfo();
            }
            catch (System.Net.Sockets.SocketException)
            {
                iRet = 1;
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet ;
        }

        /// <returns>
        ///     -2 - something wrong
        ///     -1 - the job doesn't exist on the job agent
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        public int GetJobStatus(Guid jobId, out GalaxyJobStatusInfo jobStatusInfo)
        {
            int iRet = 0;
            jobStatusInfo = new GalaxyJobStatusInfo(GalaxyJobStatus.Unknown, "");
            if (m_jobAgent == null) { return -2; }
            try
            {
                iRet = m_jobAgent.GetJobStatus(jobId, out jobStatusInfo);
            }
            catch (System.Net.Sockets.SocketException)
            {
                iRet = 1;
            }
            catch (Exception)
            {
                iRet = -2;
            }

            return iRet;
        }

        /// <returns>
        ///     -2 - something wrong
        ///     -1 - the job doesn't exist on the job agent
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        public int GetJobProperties(Guid jobId, out GalaxyJobProperties jobProps)
        {
            int iRet = 0;
            jobProps = new GalaxyJobProperties();
            if (m_jobAgent == null) { return -2; }
            try
            {
                iRet = m_jobAgent.GetJobProperties(jobId, out jobProps);
            }
            catch (System.Net.Sockets.SocketException)
            {
                iRet = 1;
            }
            catch (Exception)
            {
                iRet = -2;
            }

            return iRet;
        }

        /// <returns>
        ///     -1 - something wrong
        ///     0 - successfully
        ///     1 - job agent is offline
        /// </returns>
        public int ApplyForNewJob(GalaxyJobBasicInfo jobBasicInfo, out string strDataRootDir)
        {
            int iRet = 0;
            jobBasicInfo.m_jobId = Guid.Empty;
            strDataRootDir = null;
            if (m_jobAgent == null) { return -1; }
            try
            {
                iRet = m_jobAgent.ApplyForNewJob(jobBasicInfo, out strDataRootDir);
                if (iRet != 0)
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
        public int AppendJobRequest(GalaxyJobStartInfo jobStartInfo)
        {
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                iRet = m_jobAgent.AppendJobRequest(jobStartInfo);
                if (iRet != 0)
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
        public int StopJob(Guid jobId)
        {
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                iRet = m_jobAgent.StopJob(jobId);
                if (iRet != 0)
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
        public int RestartJob(Guid jobId)
        {
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                iRet = m_jobAgent.RestartJob(jobId);
                if (iRet != 0)
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
        public int GetWaitingJobCount(out int iWaitingJobCount)
        {
            iWaitingJobCount = 0;
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                iWaitingJobCount = m_jobAgent.GetWaitingJobCount();
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
        public int GetRunningJobCount(out int iRunningJobCount)
        {
            iRunningJobCount = 0;
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                iRunningJobCount = m_jobAgent.GetRunningJobCount();
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
        public int GetFinishedJobCount(out int iSuccessfulJobCount, out int iFailedJobCount)
        {
            iSuccessfulJobCount = 0;
            iFailedJobCount = 0;
            int iRet = 0;
            if (m_jobAgent == null) { return -1; }
            try
            {
                m_jobAgent.GetFinishedJobCount(out iSuccessfulJobCount, out iFailedJobCount);
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
        public int GetProcessNodePerfInfo(out ProcessNodePerfInfo perfInfo)
        {
            int iRet = 0;
            perfInfo = null;
            if (m_jobAgent == null) { return -1; }
            try
            {
                perfInfo = m_jobAgent.GetPerfInfo();
                if (perfInfo == null)
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


        /// <summary>
        /// Transport resource files to the process node which is needed by the job
        /// </summary>
        /// <param name="jobStartInfo">The start info of the job</param>
        /// <param name="strFileNameInLocal">The absolute path of the local file</param>
        /// <param name="strFileNameOnProcessNode">The destination file name on the process node</param>
        /// <param name="strRelativePathOnProcessNode">
        ///     The relative path to the job data root directory,
        ///     empty means job data root directory
        /// </param>
        /// <param name="iFileBufSize">The buffer size which is used to transport the file</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - something wrong
        ///     1 - job agent is offline
        /// </returns>
        public int TransportFile(
            string strProjectDataRootDir,
            string strFileNameInLocal, 
            string strFileNameOnProcessNode, 
            string strRelativePathOnProcessNode, 
            int iFileBufSize)
        {
            if (m_jobAgent == null) { return -1; }
            if (!File.Exists(strFileNameInLocal))
            {
                return -1;
            }
            // Const
            byte[] rgDataBuf;
            BinaryReader fileReader = new BinaryReader(File.Open(strFileNameInLocal, FileMode.Open, FileAccess.Read, FileShare.Read));
            bool fFirstTransport = true;
            bool fFinishTransport = false;
            while (!fFinishTransport)
            {
                rgDataBuf = fileReader.ReadBytes(iFileBufSize);
                bool fAppendData = true;
                if (fFirstTransport)
                {
                    fAppendData = false;
                    fFirstTransport = false;
                }

                // Transport the data to the job agent
                int iTransportStatus = 0;
                try
                {
                    iTransportStatus = m_jobAgent.TransportData(strProjectDataRootDir, rgDataBuf, strFileNameOnProcessNode, strRelativePathOnProcessNode, fAppendData);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    return 1;
                }
                catch (Exception)
                {
                    return -1;
                }
                if (iTransportStatus != 0)
                {
                    return -1;
                }

                if (rgDataBuf.Length < iFileBufSize)
                {
                    fFinishTransport = true;
                }
            }

            fileReader.Close();

            return 0;
        }

        #endregion

    }
}
