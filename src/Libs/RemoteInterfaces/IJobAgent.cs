using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.RemoteInterfaces
{
    [Serializable]
    public class GalaxyJobBasicInfo
    {
        public Guid m_jobId;
        public string m_strProjectName;
        public string m_strJobName;

        #region Properties

        public Guid JobId
        {
            get { return m_jobId; }
            set { m_jobId = value; }
        }

        public string ProjectName
        {
            get { return m_strProjectName; }
            set { m_strProjectName = value; }
        }

        public string JobName
        {
            get { return m_strJobName; }
            set { m_strJobName = value; }
        }

        #endregion

        public override String ToString()
        {
            return m_strProjectName + "." + m_strJobName + "(" + m_jobId.ToString() + ")";
        }
    }

    [Serializable]
    public class GalaxyJobStartInfo
    {
        #region Variables

        private string m_strExecutableFileName;
        // The relative path of the executable file when it is transported to the process node
        private string m_strRelativePath;
        private string m_strArguments;
        // The data root of the job, the job's data dir is <dataroot>\<jobid>
        private string m_strDataRootDir;
        // Whether the job report its status to job status manager
        private bool m_fAutoReportJobStatus;
        // Whether the job needs to wait for something for a long time
        private bool m_fAllowLongIdleTime;
        // The user name of the job submitter
        private string m_strUserName;

        public GalaxyJobBasicInfo m_jobBasicInfo;

        #endregion

        #region Properties

        public Guid JobId
        {
            get { return m_jobBasicInfo.m_jobId; }
            set { m_jobBasicInfo.m_jobId = value; }
        }

        public bool AllowLongIdleTime
        {
            get { return m_fAllowLongIdleTime; }
            set { m_fAllowLongIdleTime = value; }
        }

        public bool AutoReportJobStatus
        {
            get { return m_fAutoReportJobStatus; }
            set { m_fAutoReportJobStatus = value; }
        }

        public string RelativePath
        {
            get { return m_strRelativePath; }
            set { m_strRelativePath = value; }
        }

        public string DataRootDir
        {
            get { return m_strDataRootDir; }
            set { m_strDataRootDir = value; }
        }

        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }

        public string ExecutableFileName
        {
            get { return m_strExecutableFileName; }
            set { m_strExecutableFileName = value; }
        }

        public string Arguments
        {
            get { return m_strArguments; }
            set { m_strArguments = value; }
        }

        #endregion

    }

    public enum GalaxyJobRunResult
    {
        Failed = 0,
        Successful
    }

    public enum GalaxyJobStatus
    {
        Unknown = 0,
        Waiting, // Waiting another job to be finished, not in the waiting job list yet
        Queued, // Waiting in the job list to be run
        Running,
        Failed,
        Successful
    }

    [Serializable]
    public class GalaxyJobStatusInfo
    {
        #region Variables
        private GalaxyJobStatus m_jobStatus;
        private string m_strOutputBaseDir;
        #endregion

        public GalaxyJobStatusInfo(GalaxyJobStatus jobStatus, string strOutputBaseDir)
        {
            m_jobStatus = jobStatus;
            m_strOutputBaseDir = strOutputBaseDir;
        }

        #region Properties
        public GalaxyJobStatus JobStatus
        {
            get { return m_jobStatus; }
            set { m_jobStatus = value; }
        }

        public string OutputBaseDir
        {
            get { return m_strOutputBaseDir; }
            set { m_strOutputBaseDir = value; }
        }
        #endregion
    }

    [Serializable]
    public class GalaxyJobFinishInfo
    {
        #region Variables
        private GalaxyJobRunResult m_jobRunResult;
        private string m_strUserName;
        private string m_strExeRelativePath;
        private string m_strExecutableFileName;
        private string m_strArguments;
        // The data root of the job, the job's data dir is <dataroot>\<jobid>
        private string m_strDataRootDir;
        private bool m_fAutoReportJobStatus;
        private bool m_fAllowLongIdleTime;
        private string m_strProcessNodeName;
        private string m_strJobOutputBaseDir;
        #endregion

        public GalaxyJobBasicInfo m_jobBasicInfo;

        #region Properties

        public Guid JobId
        {
            get { return m_jobBasicInfo.m_jobId; }
            set { m_jobBasicInfo.m_jobId = value; }
        }

        public bool AllowLongIdleTime
        {
            get { return m_fAllowLongIdleTime; }
            set { m_fAllowLongIdleTime = value; }
        }

        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }

        public GalaxyJobRunResult JobRunResult
        {
            get { return m_jobRunResult; }
            set { m_jobRunResult = value; }
        }

        public string ExecutableFileName
        {
            get { return m_strExecutableFileName; }
            set { m_strExecutableFileName = value; }
        }

        public string ExeRelativePath
        {
            get { return m_strExeRelativePath; }
            set { m_strExeRelativePath = value; }
        }

        public string Arguments
        {
            get { return m_strArguments; }
            set { m_strArguments = value; }
        }

        public string DataRootDir
        {
            get { return m_strDataRootDir; }
            set { m_strDataRootDir = value; }
        }

        public bool AutoReportJobStatus
        {
            get { return m_fAutoReportJobStatus; }
            set { m_fAutoReportJobStatus = value; }
        }

        public string ProcessNodeName
        {
            get { return m_strProcessNodeName; }
            set { m_strProcessNodeName = value; }
        }

        public string JobOutputBaseDir
        {
            get { return m_strJobOutputBaseDir; }
            set { m_strJobOutputBaseDir = value; }
        }

        #endregion

        #region Static methods

        public static string JobRunResult2String(GalaxyJobRunResult jobRunResult)
        {
            string strJobRunResult = "";
            switch (jobRunResult)
            {
                case GalaxyJobRunResult.Failed:
                    strJobRunResult = "Failed";
                    break;
                case GalaxyJobRunResult.Successful:
                    strJobRunResult = "Successful";
                    break;
            }

            return strJobRunResult;
        }

        public static GalaxyJobRunResult String2JobRunResult(string strJobRunResult)
        {
            GalaxyJobRunResult jobRunResult = GalaxyJobRunResult.Failed;
            if (strJobRunResult == "Failed")
            {
                jobRunResult = GalaxyJobRunResult.Failed;
            }
            else if (strJobRunResult == "Successful")
            {
                jobRunResult = GalaxyJobRunResult.Successful;
            }

            return jobRunResult;
        }

        #endregion
    }

    [Serializable]
    public class ProcessNodePerfInfo
    {
        #region Variables
        private double m_dblCPUUsage; // (%)
        private double m_dblAvailablePhysicalMemory; // (M)
        private double m_dblAvailableVirtualMemory; // (M)
        private double m_dblDiskFreeSpace; // (M)
        private int m_iWaitingJobCount;
        private int m_iRunningJobCount;
        #endregion

        #region Properties
        public double CPUUsage
        {
            get { return m_dblCPUUsage; }
            set { m_dblCPUUsage = value; }
        }

        public double AvailablePhysicalMemory
        {
            get { return m_dblAvailablePhysicalMemory; }
            set { m_dblAvailablePhysicalMemory = value; }
        }

        public double AvailableVirtualMemory
        {
            get { return m_dblAvailableVirtualMemory; }
            set { m_dblAvailableVirtualMemory = value; }
        }

        public double DiskFreeSpace
        {
            get { return m_dblDiskFreeSpace; }
            set { m_dblDiskFreeSpace = value; }
        }

        public int WaitingJobCount
        {
            get { return m_iWaitingJobCount; }
            set { m_iWaitingJobCount = value; }
        }

        public int RunningJobCount
        {
            get { return m_iRunningJobCount; }
            set { m_iRunningJobCount = value; }
        }
        #endregion
    }

    [Serializable]
    public class JobAgentDataDir
    {
        #region Variables
        private string m_strDataDirName;
        // "\\<machine_name>\<data_dir_share_name>\"
        private string m_strDataDirShareName;
        #endregion

        #region Properties
        public string DataDirName
        {
            get { return m_strDataDirName; }
            set { m_strDataDirName = value; }
        }

        public string DataDirShareName
        {
            get { return m_strDataDirShareName; }
            set { m_strDataDirShareName = value; }
        }
        #endregion
    }

    public delegate void GalaxyJobFinishEventHandler(GalaxyJobFinishInfo jobFinishInfo);
    public delegate void GalaxyJobStartEventHandler(GalaxyJobStartInfo jobStartInfo);

    public interface IJobAgent
    {
        int MaximumJobIdleTime
        {
            set;
        }

        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        int AddJobFinishEventSinker(GalaxyJobFinishEventHandler jobFinishEventSinker);
        int RemoveJobFinishEventSinker(GalaxyJobFinishEventHandler jobFinishEventSinker);

        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        int AddJobStartEventSinker(GalaxyJobStartEventHandler jobStartEventSinker);
        int RemoveJobStartEventSinker(GalaxyJobStartEventHandler jobStartEventSinker);

        /// <summary>
        /// Get the status of the job
        /// </summary>
        /// <returns>
        ///     0 - successfully
        ///     -1 - no such job
        /// </returns>
        int GetJobStatus(Guid jobId, out GalaxyJobStatusInfo jobStatusInfo);

        /// <summary>
        /// Get the properties of the job
        /// </summary>
        /// <returns>
        ///     0 - successfully
        ///     -1 - no such job
        /// </returns>
        int GetJobProperties(Guid jobId, out GalaxyJobProperties jobStatusInfo);

        // TODO Guomao: Debug, remove it finally
        string GetDebugInfo();

        /// <summary>
        /// Run the job agent, it should never return
        /// It get the jobs from the waiting job list and run them
        /// </summary>
        /// <param name="iPortNumber">The port number</param>
        /// <param name="strDataDir">The root data dir of the process node</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        int Run(int iPortNumber, List<JobAgentDataDir> dataDirList, int iMaxConcurrentJobCount);

        /// <summary>
        /// Append a job request to waiting job list
        /// </summary>
        /// <param name="job">The starting information of the job</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        int AppendJobRequest(GalaxyJobStartInfo job);

        /// <summary>
        /// Stop a job
        /// </summary>
        /// <param name="jobId">The job id</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - something wrong
        /// </returns>
        int StopJob(Guid jobId);

        /// <summary>
        /// Restart a job
        /// </summary>
        /// <param name="jobId">The job id</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - something wrong
        /// </returns>
        int RestartJob(Guid jobId);

        /// <summary>
        /// Get a id for the new job,
        /// users can use this id to transport the executables and the resource files to the process node
        /// <returns>
        ///     0 - successfully
        ///     -1 - something wrong
        /// </returns>
        /// </summary>
        int ApplyForNewJob(GalaxyJobBasicInfo jobBasicInfo, out string strDataRootDir);

        /// <summary>
        /// The job count in the waiting job list
        /// </summary>
        int GetWaitingJobCount();

        /// <summary>
        /// The running job count
        /// </summary>
        int GetRunningJobCount();

        void GetFinishedJobCount(out int iSuccessfulJobCount, out int iFailedJobCount);

        /// <returns>null, if error</returns>
        ProcessNodePerfInfo GetPerfInfo();

        /// <summary>
        /// Transport job's resource files(config files, etc.) to the process node
        /// </summary>
        /// <param name="rgBuf">The buffer of the bytes</param>
        /// <param name="strFileName">The file name</param>
        /// <param name="strRelativeDir">
        ///     The path relative to the job's data dir, empty means job's data dir
        /// </param>
        /// <param name="fAppend">We append the data to an existing file</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - something wrong
        /// </returns>
        int TransportData(string strProjectDataRootDir, byte[] rgBuf, string strFileName, string strRelativeDir, bool fAppend);
    }
}
