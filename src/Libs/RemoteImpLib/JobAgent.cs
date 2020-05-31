using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.RemoteInterfaces;
using Galaxy.ManagedClientLib;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using CommonLib.IO.GMLogFile;
using System.Management;
using System.Xml;

namespace Galaxy.ProcessNode
{
    /// <summary>
    /// Galaxy In-Progress jobs
    /// </summary>
    class GalaxyInProgressJobs
    {
        #region Variables
        private List<GalaxyJobStartInfo> m_inProgressJobList; // FIFO
        #endregion

        public GalaxyInProgressJobs()
        {
            m_inProgressJobList = new List<GalaxyJobStartInfo>();
        }

        #region Properties

        public List<GalaxyJobStartInfo> InProgressJobList
        {
            get { return m_inProgressJobList; }
        }

        #endregion

        #region Public methods

        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        public int LoadFromFile(string strInProgressJobFileName)
        {
            int iRet = 0;
            try
            {
                XmlDocument jobDoc = new XmlDocument();
                jobDoc.Load(strInProgressJobFileName);
                XmlElement root = jobDoc.DocumentElement;
                XmlNodeList jobNodeList = root.SelectNodes("GalaxyInProgressJob");
                foreach (XmlNode jobNode in jobNodeList)
                {
                    XmlNode jobIdNode = jobNode.SelectSingleNode("JobId");
                    Guid jobId = new Guid(jobIdNode.InnerText.Trim());
                    XmlNode userNameNode = jobNode.SelectSingleNode("UserName");
                    string strUserName = userNameNode.InnerText.Trim();
                    XmlNode autoReportStatusNode = jobNode.SelectSingleNode("AutoReportStatus");
                    bool fAutoReportStatus = bool.Parse(autoReportStatusNode.InnerText.Trim());
                    XmlNode allowLongIdleTimeNode = jobNode.SelectSingleNode("AllowLongIdleTime");
                    bool fAllowLongIdleTime = bool.Parse(allowLongIdleTimeNode.InnerText.Trim());
                    XmlNode exeNameNode = jobNode.SelectSingleNode("ExeName");
                    string strExeName = exeNameNode.InnerText.Trim();
                    XmlNode argsNode = jobNode.SelectSingleNode("Arguments");
                    string strArgs = argsNode.InnerText.Trim();
                    XmlNode exeRelativePathNode = jobNode.SelectSingleNode("ExeRelativePath");
                    string strExeRelativePath = exeRelativePathNode.InnerText.Trim();
                    XmlNode dataRootDirNode = jobNode.SelectSingleNode("DataRootDir");
                    string strDataRootDir = dataRootDirNode.InnerText.Trim();

                    // Enqueue the job
                    GalaxyJobStartInfo jobStartInfo = new GalaxyJobStartInfo();
                    jobStartInfo.m_jobBasicInfo.m_jobId = jobId;
                    jobStartInfo.UserName = strUserName;
                    jobStartInfo.ExecutableFileName = strExeName;
                    jobStartInfo.Arguments = strArgs;
                    jobStartInfo.DataRootDir = strDataRootDir;
                    jobStartInfo.AutoReportJobStatus = fAutoReportStatus;
                    jobStartInfo.AllowLongIdleTime = fAllowLongIdleTime;
                    jobStartInfo.RelativePath = strExeRelativePath;
                    m_inProgressJobList.Add(jobStartInfo);
                }
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }

        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        public int FlushToDisk(string strInProgressJobFileName)
        {
            int iRet = 0;
            try
            {
                XmlDocument jobDoc = new XmlDocument();
                XmlDeclaration docDeclaration = jobDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                jobDoc.AppendChild(docDeclaration);
                XmlElement root = jobDoc.CreateElement("GalaxyInProgressJobs");
                jobDoc.AppendChild(root);
                // Flush each jobs
                foreach (GalaxyJobStartInfo jobStartInfo in m_inProgressJobList)
                {
                    XmlElement jobElement = jobDoc.CreateElement("GalaxyInProgressJob");

                    XmlElement jobIdElement = jobDoc.CreateElement("JobId");
                    jobIdElement.InnerText = jobStartInfo.m_jobBasicInfo.m_jobId.ToString();
                    jobElement.AppendChild(jobIdElement);

                    XmlElement projectNameElement = jobDoc.CreateElement("ProjectName");
                    projectNameElement.InnerText = jobStartInfo.m_jobBasicInfo.m_strProjectName;
                    jobElement.AppendChild(projectNameElement);

                    XmlElement jobNameElement = jobDoc.CreateElement("JobName");
                    jobNameElement.InnerText = jobStartInfo.m_jobBasicInfo.m_strJobName;
                    jobElement.AppendChild(jobNameElement);

                    XmlElement userNameElement = jobDoc.CreateElement("UserName");
                    userNameElement.InnerText = jobStartInfo.UserName;
                    jobElement.AppendChild(userNameElement);

                    XmlElement autoReportStatusElement = jobDoc.CreateElement("AutoReportStatus");
                    autoReportStatusElement.InnerText = jobStartInfo.AutoReportJobStatus.ToString();
                    jobElement.AppendChild(autoReportStatusElement);

                    XmlElement allowLongIdleTimeElement = jobDoc.CreateElement("AllowLongIdleTime");
                    allowLongIdleTimeElement.InnerText = jobStartInfo.AllowLongIdleTime.ToString();
                    jobElement.AppendChild(allowLongIdleTimeElement);

                    XmlElement exeNameElement = jobDoc.CreateElement("ExeName");
                    exeNameElement.InnerText = jobStartInfo.ExecutableFileName;
                    jobElement.AppendChild(exeNameElement);

                    XmlElement argsElement = jobDoc.CreateElement("Arguments");
                    XmlCDataSection argsData = jobDoc.CreateCDataSection(jobStartInfo.Arguments);
                    argsElement.AppendChild(argsData);
                    jobElement.AppendChild(argsElement);

                    XmlElement exeRelativePathElement = jobDoc.CreateElement("ExeRelativePath");
                    XmlCDataSection exeRelativePathData = jobDoc.CreateCDataSection(jobStartInfo.RelativePath);
                    exeRelativePathElement.AppendChild(exeRelativePathData);
                    jobElement.AppendChild(exeRelativePathElement);

                    XmlElement dataRootDirElement = jobDoc.CreateElement("DataRootDir");
                    XmlCDataSection dataRootDirData = jobDoc.CreateCDataSection(jobStartInfo.DataRootDir);
                    dataRootDirElement.AppendChild(dataRootDirData);
                    jobElement.AppendChild(dataRootDirElement);

                    root.AppendChild(jobElement);
                }
                jobDoc.Save(strInProgressJobFileName);
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }

        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        public int AddInProgressJob(GalaxyJobStartInfo jobStartInfo)
        {
            m_inProgressJobList.Add(jobStartInfo);

            return 0;
        }

        /// <returns>
        ///     null - failed
        /// </returns>
        public void RemoveInProgressJob(Guid jobId)
        {
            int iTargetJobIndex = -1;
            for (int i = 0; (i < m_inProgressJobList.Count) && (iTargetJobIndex == -1); i++)
            {
                if (m_inProgressJobList[i].m_jobBasicInfo.m_jobId == jobId)
                {
                    iTargetJobIndex = i;
                }
            }
            if (iTargetJobIndex != -1)
            {
                m_inProgressJobList.RemoveAt(iTargetJobIndex);
            }
        }

        #endregion

    }

    class GalaxyFinishedJobs
    {
        #region Variables
        private Dictionary<Guid, GalaxyJobFinishInfo> m_finishedJobDict;
        #endregion

        #region Properties
        public Dictionary<Guid, GalaxyJobFinishInfo> FinishedJobDictionary
        {
            get { return m_finishedJobDict; }
        }
        #endregion

        public GalaxyFinishedJobs()
        {
            m_finishedJobDict = new Dictionary<Guid, GalaxyJobFinishInfo>();
        }

        #region Public methods
        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        public int LoadFromFile(string strFinishedJobFileName)
        {
            int iRet = 0;
            try
            {
                XmlDocument jobDoc = new XmlDocument();
                jobDoc.Load(strFinishedJobFileName);
                XmlElement root = jobDoc.DocumentElement;
                XmlNodeList jobNodeList = root.SelectNodes("GalaxyFinishedJob");
                foreach (XmlNode jobNode in jobNodeList)
                {
                    XmlNode jobIdNode = jobNode.SelectSingleNode("JobId");
                    Guid jobId = new Guid(jobIdNode.InnerText.Trim());
                    XmlNode projectNameNode = jobNode.SelectSingleNode("ProjectId");
                    String strProjectName = projectNameNode.InnerText;
                    XmlNode jobNameNode = jobNode.SelectSingleNode("JobName");
                    String strJobName = jobNameNode.InnerText;
                    XmlNode userNameNode = jobNode.SelectSingleNode("UserName");
                    string strUserName = userNameNode.InnerText.Trim();
                    XmlNode jobRunResultNode = jobNode.SelectSingleNode("JobRunResult");
                    GalaxyJobRunResult jobRunResult = GalaxyJobFinishInfo.String2JobRunResult(jobRunResultNode.InnerText.Trim());
                    XmlNode exeNameNode = jobNode.SelectSingleNode("ExeName");
                    string strExeName = exeNameNode.InnerText.Trim();
                    XmlNode argsNode = jobNode.SelectSingleNode("Arguments");
                    string strArgs = argsNode.InnerText.Trim();
                    XmlNode outputDirNode = jobNode.SelectSingleNode("OutputBaseDir");
                    string strOutputBaseDir = outputDirNode.InnerText.Trim();
                    XmlNode dataRootDirNode = jobNode.SelectSingleNode("DataRootDir");
                    string strDataRootDir = dataRootDirNode.InnerText.Trim();
                    XmlNode autoReportStatusNode = jobNode.SelectSingleNode("AutoReportStatus");
                    bool fAutoReportStatus = bool.Parse(autoReportStatusNode.InnerText.Trim());
                    XmlNode allowLongIdleTimeNode = jobNode.SelectSingleNode("AllowLongIdleTime");
                    bool fAllowLongIdleTime = bool.Parse(allowLongIdleTimeNode.InnerText.Trim());
                    XmlNode exeRelativePathNode = jobNode.SelectSingleNode("ExeRelativePath");
                    string strExeRelativePath = exeRelativePathNode.InnerText.Trim();

                    if (m_finishedJobDict.ContainsKey(jobId))
                    {
                        return -1;
                    }
                    else
                    {
                        GalaxyJobFinishInfo jobFinishInfo = new GalaxyJobFinishInfo();
                        jobFinishInfo.m_jobBasicInfo = new GalaxyJobBasicInfo();
                        jobFinishInfo.m_jobBasicInfo.m_jobId = jobId;
                        jobFinishInfo.m_jobBasicInfo.m_strProjectName = strProjectName;
                        jobFinishInfo.m_jobBasicInfo.m_strJobName = strJobName;
                        jobFinishInfo.UserName = strUserName;
                        jobFinishInfo.ExecutableFileName = strExeName;
                        jobFinishInfo.ExeRelativePath = strExeRelativePath;
                        jobFinishInfo.AutoReportJobStatus = fAutoReportStatus;
                        jobFinishInfo.AllowLongIdleTime = fAllowLongIdleTime;
                        jobFinishInfo.Arguments = strArgs;
                        jobFinishInfo.DataRootDir = strDataRootDir;
                        jobFinishInfo.JobRunResult = jobRunResult;
                        jobFinishInfo.JobOutputBaseDir = strOutputBaseDir;
                        m_finishedJobDict.Add(jobId, jobFinishInfo);
                    }
                }
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }

        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        public int FlushToDisk(string strFinishedJobFileName)
        {
            int iRet = 0;
            try
            {
                XmlDocument jobDoc = new XmlDocument();
                XmlDeclaration docDeclaration = jobDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                jobDoc.AppendChild(docDeclaration);
                XmlElement root = jobDoc.CreateElement("GalaxyFinishedJobs");
                jobDoc.AppendChild(root);
                // Flush each jobs
                foreach (GalaxyJobFinishInfo jobFinishInfo in m_finishedJobDict.Values)
                {
                    XmlElement jobElement = jobDoc.CreateElement("GalaxyFinishedJob");

                    XmlElement jobIdElement = jobDoc.CreateElement("JobId");
                    jobIdElement.InnerText = jobFinishInfo.m_jobBasicInfo.m_jobId.ToString();
                    jobElement.AppendChild(jobIdElement);

                    XmlElement projectNameElement = jobDoc.CreateElement("ProjectName");
                    projectNameElement.InnerText = jobFinishInfo.m_jobBasicInfo.m_strProjectName;
                    jobElement.AppendChild(projectNameElement);

                    XmlElement jobNameElement = jobDoc.CreateElement("JobName");
                    jobNameElement.InnerText = jobFinishInfo.m_jobBasicInfo.m_strJobName;
                    jobElement.AppendChild(jobNameElement);

                    XmlElement userNameElement = jobDoc.CreateElement("UserName");
                    userNameElement.InnerText = jobFinishInfo.UserName;
                    jobElement.AppendChild(userNameElement);

                    XmlElement jobRunResultElement = jobDoc.CreateElement("JobRunResult");
                    jobRunResultElement.InnerText = GalaxyJobFinishInfo.JobRunResult2String(jobFinishInfo.JobRunResult);
                    jobElement.AppendChild(jobRunResultElement);

                    XmlElement exeNameElement = jobDoc.CreateElement("ExeName");
                    exeNameElement.InnerText = jobFinishInfo.ExecutableFileName;
                    jobElement.AppendChild(exeNameElement);

                    XmlElement exeRelativePathElement = jobDoc.CreateElement("ExeRelativePath");
                    XmlCDataSection exeRelativePathData = jobDoc.CreateCDataSection(jobFinishInfo.ExeRelativePath);
                    exeRelativePathElement.AppendChild(exeRelativePathData);
                    jobElement.AppendChild(exeRelativePathElement);

                    XmlElement argsElement = jobDoc.CreateElement("Arguments");
                    XmlCDataSection argsData = jobDoc.CreateCDataSection(jobFinishInfo.Arguments);
                    argsElement.AppendChild(argsData);
                    jobElement.AppendChild(argsElement);

                    XmlElement dataRootDirElement = jobDoc.CreateElement("DataRootDir");
                    XmlCDataSection dataRootDirData = jobDoc.CreateCDataSection(jobFinishInfo.DataRootDir);
                    dataRootDirElement.AppendChild(dataRootDirData);
                    jobElement.AppendChild(dataRootDirElement);

                    XmlElement autoReportStatusElement = jobDoc.CreateElement("AutoReportStatus");
                    autoReportStatusElement.InnerText = jobFinishInfo.AutoReportJobStatus.ToString();
                    jobElement.AppendChild(autoReportStatusElement);

                    XmlElement allowLongIdleTimeElement = jobDoc.CreateElement("AllowLongIdleTime");
                    allowLongIdleTimeElement.InnerText = jobFinishInfo.AllowLongIdleTime.ToString();
                    jobElement.AppendChild(allowLongIdleTimeElement);

                    XmlElement outputDirElement = jobDoc.CreateElement("OutputBaseDir");
                    XmlCDataSection outputDirData = jobDoc.CreateCDataSection(jobFinishInfo.JobOutputBaseDir);
                    outputDirElement.AppendChild(outputDirData);
                    jobElement.AppendChild(outputDirElement);

                    root.AppendChild(jobElement);
                }
                jobDoc.Save(strFinishedJobFileName);
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }

        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        public int AddFinishedJob(GalaxyJobFinishInfo jobFinishInfo)
        {
            if (m_finishedJobDict.ContainsKey(jobFinishInfo.JobId)) { return -1; }
            m_finishedJobDict.Add(jobFinishInfo.JobId, jobFinishInfo);

            return 0;
        }

        /// <returns>
        ///     null - no such job
        /// </returns>
        public GalaxyJobFinishInfo RemoveFinishedJob(Guid jobId)
        {
            GalaxyJobFinishInfo removedJobFinishInfo = null;
            if (m_finishedJobDict.ContainsKey(jobId))
            {
                removedJobFinishInfo = m_finishedJobDict[jobId];
                m_finishedJobDict.Remove(jobId);
            }

            return removedJobFinishInfo;
        }

        /// <returns>
        ///     null - failed
        /// </returns>
        public GalaxyJobFinishInfo GetFinishedJob(Guid jobId)
        {
            GalaxyJobFinishInfo jobFinishInfo = null;
            if (m_finishedJobDict.ContainsKey(jobId))
            {
                jobFinishInfo = m_finishedJobDict[jobId];
            }

            return jobFinishInfo;
        }

        #endregion

    }

    class JobRunningInfo
    {
        #region Variables
        
        private GalaxyJobStartInfo m_jobStartInfo;
        private Process m_jobProcess;
        private int m_iProcessId;
        private double m_dblTotalProcessorTime;
        private int m_iCheckTimes;
        private bool m_fIsUserStopped;
        private bool m_fAllowLongIdleTime;
        
        #endregion

        #region Properties

        public GalaxyJobStartInfo JobStartInfo
        {
            get { return m_jobStartInfo; }
            set { m_jobStartInfo = value; }
        }

        public Process JobProcess
        {
            get { return m_jobProcess; }
            set { m_jobProcess = value; }
        }

        public double TotalProcessorTime
        {
            get { return m_dblTotalProcessorTime; }
            set { m_dblTotalProcessorTime = value; }
        }

        public int CheckTimes
        {
            get { return m_iCheckTimes; }
            set { m_iCheckTimes = value; }
        }

        public bool IsUserStopped
        {
            get { return m_fIsUserStopped; }
            set { m_fIsUserStopped = value; }
        }

        public bool AllowLongIdleTime
        {
            get { return m_fAllowLongIdleTime; }
            set { m_fAllowLongIdleTime = value; }
        }

        public int ProcessId
        {
            get { return m_iProcessId; }
            set { m_iProcessId = value; }
        }

        #endregion

        public JobRunningInfo()
        {
            JobStartInfo = null;
            JobProcess = null;
            TotalProcessorTime = -1.0;
            CheckTimes = 0;
            IsUserStopped = false;
            AllowLongIdleTime = false;
            m_iProcessId = -1;
        }

        public JobRunningInfo(GalaxyJobStartInfo jobStartInfo, Process jobProcess, bool fAllowLongIdleTime)
        {
            JobStartInfo = jobStartInfo;
            JobProcess = jobProcess;
            AllowLongIdleTime = fAllowLongIdleTime;
            TotalProcessorTime = -1.0;
            CheckTimes = 0;
            IsUserStopped = false;
            m_iProcessId = -1;
        }
    }

    class JobAgent:MarshalByRefObject, IJobAgent
    {
        #region Variables
        // The lock to access waiting job queue
        private static object ms_accessWaitingJobQueueLock;
        // The lock to access job running info dictionary
        private static object ms_accessJobRunningInfoDictLock;
        // The lock to access finished job list
        private static object ms_accessFinishedJobListLock;
        // The lock to access the event handler list
        private static object ms_accessJobFinishSinkers;
        private static object ms_accessJobStartSinkers;
        // The event of receiving a new job request
        private AutoResetEvent m_newJobReceivedEvent;
        // The event of some executing resource of the process node is freed
        private AutoResetEvent m_allowJobRunningEvent;
        // The queue of the waiting jobs
        private Queue<GalaxyJobStartInfo> m_waitingJobQueue;
        // We record the in progress jobs to restart the in-progress job when pn is restarted
        private GalaxyInProgressJobs m_inProgressJobs;
        // The current running jobs
        // This data structure is used to detect the job which will cause a run-time error
        // The job with runtime error will never finish util you click the buttons on the popuped error dialog,
        // so the job will hang on there. The method which we use to detect this issue is to check the job's idle time.
        // If the idle time is long enough, we will kill this job and set the status of the job to failed.
        private Dictionary<Guid, JobRunningInfo> m_jobRunningInfoDict;
        // The finished jobs of the process node
        private GalaxyFinishedJobs m_finishedJobs;
        private int m_iCheckJobStatusPeriod; // milliseconds
        // The maximum count of the concurrent job
        private int m_iMaxConcurrentJobCount;
        // The data directories of the job agent
        private List<JobAgentDataDir> m_dataDirList;
        // The log file name
        private static string ms_strLogFileName = "GalaxyJobAgent.txt";
        // The finished job file, the process node has the memory to remember which jobs it has finished
        private static string ms_strFinishedJobFileName = "GalaxyFinishedJobs.xml";
        // We record the in progress jobs to restart the in-progress job when pn is restarted
        private static string ms_strInProgressJobFileName = "GalaxyInProgressJobs.xml";

        // The events
        private event GalaxyJobFinishEventHandler OnJobFinish;
        private event GalaxyJobStartEventHandler OnJobStart;
        #endregion

        #region Properties

        public int MaximumJobIdleTime
        {
            set { m_iCheckJobStatusPeriod = value * 60 * 1000; }
        }

        #endregion

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public JobAgent()
        {
            ms_accessWaitingJobQueueLock = null;
            ms_accessJobRunningInfoDictLock = null;
            ms_accessFinishedJobListLock = null;
            ms_accessJobFinishSinkers = null;
            ms_accessJobStartSinkers = null;
            m_newJobReceivedEvent = null;
            m_allowJobRunningEvent = null;

            m_waitingJobQueue = null;
            m_inProgressJobs = null;
            m_jobRunningInfoDict = null;
            m_finishedJobs = null;
            m_iMaxConcurrentJobCount = 0;
            OnJobFinish = null;
            OnJobStart = null;
        }

        #region Run job

        private void RunJob(object threadPara)
        {
            GalaxyJobStartInfo jobStartInfo = threadPara as GalaxyJobStartInfo;
            if (jobStartInfo == null)
            {
                // Wrong parameter
                return;
            }

            // Invoke the call back
            lock (ms_accessJobStartSinkers)
            {
                if (OnJobStart != null)
                {
                    System.Delegate[] invokeList = OnJobStart.GetInvocationList();
                    IEnumerator ie = invokeList.GetEnumerator();
                    while (ie.MoveNext())
                    {
                        GalaxyJobStartEventHandler jobStartListener = ie.Current as GalaxyJobStartEventHandler;
                        try
                        {
                            jobStartListener.BeginInvoke(jobStartInfo, null, null);
                        }
                        catch (Exception)
                        {
                            GMThreadSafeLogFile.Log("The job:" + jobStartInfo.m_jobBasicInfo.ToString() + " calls started listener error!");
                            OnJobStart -= jobStartListener;
                        }
                    }
                }
            }

            GalaxyJobBasicInfo jobBasicInfo = jobStartInfo.m_jobBasicInfo;
            Guid jobId = jobBasicInfo.m_jobId;
            JobAgentDataDir jobAgentDataDir = GetJobAgentDataDir(jobStartInfo.DataRootDir);
            Debug.Assert(jobAgentDataDir != null);
            string strJobDataDir = jobStartInfo.DataRootDir + "\\" + jobBasicInfo.m_strProjectName + "\\" + jobBasicInfo.m_strJobName;
            Process jobProcess = new Process();
            jobProcess.StartInfo.FileName = strJobDataDir + "\\" + jobStartInfo.RelativePath + "\\" + jobStartInfo.ExecutableFileName;
            jobProcess.StartInfo.Arguments = jobStartInfo.Arguments;
            jobProcess.StartInfo.WorkingDirectory = strJobDataDir + "\\" + jobStartInfo.RelativePath;
            jobProcess.StartInfo.UseShellExecute = false;
            jobProcess.StartInfo.RedirectStandardError = true;
            jobProcess.StartInfo.RedirectStandardOutput = true;
            jobProcess.StartInfo.CreateNoWindow = true;
            jobProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            
            // Update the job process info
            JobRunningInfo jobRunningInfo;
            if (GetJobRunningInfo(jobId, out jobRunningInfo))
            {
                jobRunningInfo.JobProcess = jobProcess;
            }
            else
            {
                GMThreadSafeLogFile.LogError("The job:" + jobBasicInfo.ToString() + " should exist in the running job list.");
                return;
            }

            // Run the job process
            bool fJobStartedSuccessfully = true;
            int iExitCode = -1;
            string strProcessOutputInfo = "";
            string strProcessErrorInfo = "";
            try
            {
                jobProcess.Start();
            }
            catch (Exception)
            {
                // Exception occus when starting the job
                GMThreadSafeLogFile.Log("Start job:" + jobBasicInfo.ToString() + " error!");
                fJobStartedSuccessfully = false;
            }

            if (fJobStartedSuccessfully)
            {
                jobRunningInfo.ProcessId = jobProcess.Id;
                // We wait the job to be finished only when the job has been started successfully
                jobProcess.WaitForExit();
                iExitCode = jobProcess.ExitCode;
                if (jobRunningInfo.CheckTimes < 2)
                {
                    // job exit normally
                    strProcessOutputInfo = jobProcess.StandardOutput.ReadToEnd();
                    strProcessErrorInfo = jobProcess.StandardError.ReadToEnd();
                }
                jobProcess.Close();
            }
            RemoveJobRunningInfo(jobId);

            // Dump the job's output
            StreamWriter jobOutputFile = new StreamWriter(strJobDataDir + "\\" + jobBasicInfo.m_strJobName.ToString() + "_Output.txt");
            jobOutputFile.WriteLine("Job info ************************");
            jobOutputFile.WriteLine("\tExecutable binary file name: " + jobStartInfo.ExecutableFileName);
            jobOutputFile.WriteLine("\tAutoReportStatus: " + jobStartInfo.AutoReportJobStatus.ToString());
            if (jobStartInfo.Arguments != null)
            {
                jobOutputFile.WriteLine("\tArguments: " + jobStartInfo.Arguments);
            }
            jobOutputFile.WriteLine("\tWorking directory: " + strJobDataDir + "\\" + jobStartInfo.RelativePath);
            jobOutputFile.WriteLine();
            if (fJobStartedSuccessfully)
            {
                if (jobRunningInfo.IsUserStopped)
                {
                    // User has stopped the job
                    jobOutputFile.WriteLine("Job is stopped by the user!");
                }
                else if (jobRunningInfo.CheckTimes >= 2)
                {
                    // Runtime error
                    jobOutputFile.WriteLine("Job runtime error!");
                }
                else
                {
                    jobOutputFile.WriteLine("Job is normally finished.");
                    jobOutputFile.WriteLine("Exit code: " + iExitCode.ToString());
                }
            }
            else
            {
                jobOutputFile.WriteLine("Job is started failed!");
            }
            jobOutputFile.WriteLine();
            jobOutputFile.WriteLine("***Standard output info***");
            jobOutputFile.WriteLine(strProcessOutputInfo);
            jobOutputFile.WriteLine();
            jobOutputFile.WriteLine("***Standard error info***");
            jobOutputFile.WriteLine(strProcessErrorInfo);

            jobOutputFile.Close();

            // Job is finished, add it to finished job list and remove it from the running list
            GalaxyJobRunResult jobRunResult;
            if (fJobStartedSuccessfully)
            {
                if (jobRunningInfo.IsUserStopped)
                {
                    // The job is stopped by the user manually
                    GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " is stopped by the user.");
                    jobRunResult = GalaxyJobRunResult.Failed;
                }
                else if (jobRunningInfo.CheckTimes >= 2)
                {
                    // The job has a runtime error
                    GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " failed (too long idle time).");
                    jobRunResult = GalaxyJobRunResult.Failed;
                }
                else
                {
                    if (jobStartInfo.AutoReportJobStatus)
                    {
                        GalaxyJobProperties jobProps;
                        if (JobStatusHelper.GetJobProperties(jobRunningInfo.ProcessId, out jobProps) == 0)
                        {
                            if (jobProps.Properties.ContainsKey(GalaxyJobProperties.ms_strJobStatusPropName))
                            {
                                string strJobStatusPropValue = jobProps.Properties[GalaxyJobProperties.ms_strJobStatusPropName];
                                if (strJobStatusPropValue == GalaxyJobProperties.ms_strPropValueOfSucJob)
                                {
                                    GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " finished successfully (AutoReportJobStatus).");
                                    jobRunResult = GalaxyJobRunResult.Successful;
                                }
                                else if (strJobStatusPropValue == GalaxyJobProperties.ms_strPropValueOfFailedJob)
                                {
                                    GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " failed (AutoReportJobStatus).");
                                    jobRunResult = GalaxyJobRunResult.Failed;
                                }
                                else
                                {
                                    GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " failed (AutoReportJobStatus-AbnormalStatus).");
                                    jobRunResult = GalaxyJobRunResult.Failed;
                                }
                            }
                            else
                            {
                                GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " failed (AutoReportJobStatus-NotFoundStatus).");
                                jobRunResult = GalaxyJobRunResult.Failed;
                            }
                        }
                        else
                        {
                            // Get job status error, we simply think the job is finished failed
                            GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " finished successfully (AutoReportJobStatus-GetJobStatusError).");
                            jobRunResult = GalaxyJobRunResult.Failed;
                        }

                        // Remove the job status from the job status manager
                        JobStatusHelper.RemoveJob(jobRunningInfo.ProcessId);
                    }
                    else
                    {
                        // If the job doesn't want to resport its status to the job status manager
                        // We simply think the job runs successfully when the job process is finished
                        GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " finished successfully (ProcessFinished).");
                        jobRunResult = GalaxyJobRunResult.Successful;
                    }
                }
            }
            else
            {
                // The job process is not started successfully
                GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " failed (JobProcessStartedFailed).");
                jobRunResult = GalaxyJobRunResult.Failed;
            }

            // Add finished jobs
            GalaxyJobFinishInfo jobFinishInfo = new GalaxyJobFinishInfo();
            jobFinishInfo.ExecutableFileName = jobStartInfo.ExecutableFileName;
            jobFinishInfo.Arguments = jobStartInfo.Arguments;
            jobFinishInfo.JobId = jobStartInfo.m_jobBasicInfo.m_jobId;
            jobFinishInfo.ExeRelativePath = jobStartInfo.RelativePath;
            jobFinishInfo.DataRootDir = jobStartInfo.DataRootDir;
            jobFinishInfo.AutoReportJobStatus = jobStartInfo.AutoReportJobStatus;
            jobFinishInfo.UserName = jobStartInfo.UserName;
            jobFinishInfo.ProcessNodeName = Environment.MachineName;
            // Get the share name of the data dir root
            jobFinishInfo.JobOutputBaseDir = "\\\\" + Environment.MachineName + "\\" + jobAgentDataDir.DataDirShareName
                + "\\" + jobStartInfo.m_jobBasicInfo.m_strProjectName + "\\" + jobStartInfo.m_jobBasicInfo.m_strJobName + "\\";
            jobFinishInfo.JobRunResult = jobRunResult;
            AddFinishedJob(jobFinishInfo);
            // Invoke the call back
            lock (ms_accessJobFinishSinkers)
            {
                if (OnJobFinish != null)
                {
                    System.Delegate[] invokeList = OnJobFinish.GetInvocationList();
                    IEnumerator ie = invokeList.GetEnumerator();
                    while (ie.MoveNext())
                    {
                        GalaxyJobFinishEventHandler jobFinishListener = ie.Current as GalaxyJobFinishEventHandler;
                        try
                        {
                            jobFinishListener.BeginInvoke(jobFinishInfo, null, null);
                        }
                        catch (Exception)
                        {
                            GMThreadSafeLogFile.Log("The job:" + jobBasicInfo.ToString() + " calls finished listener error!");
                            OnJobFinish -= jobFinishListener;
                        }
                    }
                }
            }

            m_allowJobRunningEvent.Set();
        }

        #endregion

        #region Monitor job status

        private void MonitorJobStatus(object objPara)
        {
            lock (ms_accessJobRunningInfoDictLock)
            {
                foreach (Guid jobId in m_jobRunningInfoDict.Keys)
                {
                    JobRunningInfo jobRunningInfo = m_jobRunningInfoDict[jobId];
                    // Check whether the job has no response during this period
                    Process jobProcess = jobRunningInfo.JobProcess;
                    // We firstly initialize the job process info to null and will update it in the job thread,
                    // so sometimes this will enter the region between the two phrases
                    if ((jobProcess != null) && (!jobRunningInfo.AllowLongIdleTime))
                    {
                        double dblCurrentTotalProcessorTime = jobProcess.TotalProcessorTime.TotalMilliseconds;
                        if ((jobRunningInfo.TotalProcessorTime != -1.0)
                            && ((dblCurrentTotalProcessorTime - jobRunningInfo.TotalProcessorTime) < 0.0001))
                        {
                            jobRunningInfo.CheckTimes++;
                            // The job has no reponse during this period
                            GMThreadSafeLogFile.Log("The job:" + jobId.ToString() + " is checked for " + jobRunningInfo.CheckTimes.ToString() + " times.");
                            if (jobRunningInfo.CheckTimes >= 2)
                            {
                                // The job has no reponse for at least one duration of the timer,
                                // then we kill the process
                                GMThreadSafeLogFile.Log("The job:" + jobId.ToString() + " has been idle for too long time, we terminate it.");
                                jobProcess.Kill();
                            }
                        }
                        else
                        {
                            // Update the processor time
                            jobRunningInfo.TotalProcessorTime = dblCurrentTotalProcessorTime;
                        }
                    }
                }
            }
        }

        #endregion

        #region IJobAgent

        public int AddJobFinishEventSinker(GalaxyJobFinishEventHandler jobFinishEventSinker)
        {
            lock (ms_accessJobFinishSinkers)
            {
                OnJobFinish += jobFinishEventSinker;
            }
            return 0;
        }

        public int RemoveJobFinishEventSinker(GalaxyJobFinishEventHandler jobFinishEventSinker)
        {
            lock (ms_accessJobFinishSinkers)
            {
                OnJobFinish -= jobFinishEventSinker;
            }
            return 0;
        }

        public int AddJobStartEventSinker(GalaxyJobStartEventHandler jobStartEventSinker)
        {
            lock (ms_accessJobStartSinkers)
            {
                OnJobStart += jobStartEventSinker;
            }
            return 0;
        }

        public int RemoveJobStartEventSinker(GalaxyJobStartEventHandler jobStartEventSinker)
        {
            lock (ms_accessJobStartSinkers)
            {
                OnJobStart -= jobStartEventSinker;
            }
            return 0;
        }

        public int GetJobProperties(Guid jobId, out GalaxyJobProperties jobProperties)
        {
            int iRet = 0;
            jobProperties = null;
            JobRunningInfo jobRunningInfo;
            if (GetJobRunningInfo(jobId, out jobRunningInfo))
            {
                if (JobStatusHelper.GetJobProperties(jobRunningInfo.ProcessId, out jobProperties) != 0)
                {
                    iRet = -1;
                }
            }
            else
            {
                iRet = -1;
            }

            return iRet;
        }

        public int GetJobStatus(Guid jobId, out GalaxyJobStatusInfo jobStatusInfo)
        {
            int iRet = 0;
            jobStatusInfo = new GalaxyJobStatusInfo(GalaxyJobStatus.Unknown, "");

            // Checking the waiting job queue
            lock (ms_accessWaitingJobQueueLock)
            {
                foreach (GalaxyJobStartInfo jobStartInfo in m_waitingJobQueue)
                {
                    if (jobStartInfo.m_jobBasicInfo.m_jobId == jobId)
                    {
                        jobStatusInfo.JobStatus = GalaxyJobStatus.Queued;
                        break;
                    }
                }
            }

            // Checking the running job dictionary
            if (jobStatusInfo.JobStatus == GalaxyJobStatus.Unknown)
            {
                lock (ms_accessJobRunningInfoDictLock)
                {
                    if (m_jobRunningInfoDict.ContainsKey(jobId))
                    {
                        jobStatusInfo.JobStatus = GalaxyJobStatus.Running;
                    }
                }
            }

            // Checking the finished jobs
            if (jobStatusInfo.JobStatus == GalaxyJobStatus.Unknown)
            {
                lock (ms_accessFinishedJobListLock)
                {
                    if (m_finishedJobs.FinishedJobDictionary.ContainsKey(jobId))
                    {
                        GalaxyJobFinishInfo jobFinishInfo = m_finishedJobs.FinishedJobDictionary[jobId];
                        jobStatusInfo.OutputBaseDir = jobFinishInfo.JobOutputBaseDir;
                        if (jobFinishInfo.JobRunResult == GalaxyJobRunResult.Failed)
                        {
                            jobStatusInfo.JobStatus = GalaxyJobStatus.Failed;
                        }
                        else if (jobFinishInfo.JobRunResult == GalaxyJobRunResult.Successful)
                        {
                            jobStatusInfo.JobStatus = GalaxyJobStatus.Successful;
                        }
                    }
                }
            }

            if (jobStatusInfo.JobStatus == GalaxyJobStatus.Unknown)
            {
                iRet = -1;
            }

            return iRet;
        }

        public string GetDebugInfo()
        {
            int iJobStartEventListenerCount = 0;
            int iJobFinishEventListenerCount = 0;

            if (OnJobStart != null)
            {
                iJobStartEventListenerCount = OnJobStart.GetInvocationList().Length;
            }
            if (OnJobFinish != null)
            {
                iJobFinishEventListenerCount = OnJobFinish.GetInvocationList().Length;
            }

            return iJobStartEventListenerCount.ToString() + ":" + iJobFinishEventListenerCount;
        }

        public int Run(int iPortNumber, List<JobAgentDataDir> dataDirList, int iMaxConcurrentJobCount)
        {
            ms_accessWaitingJobQueueLock = new object();
            ms_accessJobRunningInfoDictLock = new object();
            ms_accessFinishedJobListLock = new object();
            ms_accessJobFinishSinkers = new object();
            ms_accessJobStartSinkers = new object();
            m_newJobReceivedEvent = new AutoResetEvent(false);
            m_allowJobRunningEvent = new AutoResetEvent(false);
            m_iMaxConcurrentJobCount = iMaxConcurrentJobCount;
            m_waitingJobQueue = new Queue<GalaxyJobStartInfo>();
            m_inProgressJobs = new GalaxyInProgressJobs();
            m_jobRunningInfoDict = new Dictionary<Guid, JobRunningInfo>();
            m_finishedJobs = new GalaxyFinishedJobs();
            m_dataDirList = dataDirList;

            // Connect to the JobStatusManager
            if (!JobStatusHelper.Initialize(iPortNumber)) { return -1; }
            // Prepare the data directory
            string strMainDataDir = null; // The first data dir is the main data dir, because this isn't important
            foreach (JobAgentDataDir dataDir in m_dataDirList)
            {
                string strDataDir = dataDir.DataDirName;
                if (strMainDataDir == null)
                {
                    strMainDataDir = strDataDir;
                }
                if (!Directory.Exists(strDataDir))
                {
                    try
                    {
                        Directory.CreateDirectory(strDataDir);
                    }
                    catch (Exception)
                    {
                        // Create data directory error
                        return -1;
                    }
                }
            }
            if (strMainDataDir == null)
            {
                return -1;
            }

            // Initialize the log file
            if (!GMThreadSafeLogFile.Open(strMainDataDir + "\\" + ms_strLogFileName))
            {
                return -1;
            }

            // Loading the finished jobs
            ms_strFinishedJobFileName = strMainDataDir + "\\" + ms_strFinishedJobFileName;
            if (File.Exists(ms_strFinishedJobFileName))
            {
                if (m_finishedJobs.LoadFromFile(ms_strFinishedJobFileName) != 0)
                {
                    return -1;
                }
            }

            TimerCallback jobStatusMonitorCallback = null;
            Timer jobStatusMonitor = null; 
            // Initialize a timer to check the status of the job periodly
            jobStatusMonitorCallback = new TimerCallback(MonitorJobStatus);
            jobStatusMonitor = new Timer(MonitorJobStatus, null, m_iCheckJobStatusPeriod, m_iCheckJobStatusPeriod);

            // It should never stop when the machine works well
            GMThreadSafeLogFile.Log("Instance of process node is running ...\r\n"
                + "\tInstance name: " + Environment.MachineName + "\r\n"
                + "\tPort number: " + iPortNumber.ToString() + "\r\n"
                + "\tMax concurrent job count: " + iMaxConcurrentJobCount.ToString() + "\r\n"
                + "\tMaximum job idle time: " + (m_iCheckJobStatusPeriod / 60 / 1000).ToString() + " mins\r\n"
                + "\tPrevious in-progress jobs: " + m_inProgressJobs.InProgressJobList.Count.ToString() + "\r\n"
                + "\tPrevious finished jobs: " + m_finishedJobs.FinishedJobDictionary.Count.ToString());
            GMThreadSafeLogFile.Log("\tData directories:");
            for (int iDataDirIndex = 1; iDataDirIndex <= m_dataDirList.Count; iDataDirIndex++)
            {
                JobAgentDataDir dataDir = m_dataDirList[iDataDirIndex - 1];
                GMThreadSafeLogFile.Log("\t\tData dir" + iDataDirIndex.ToString() + ":" + dataDir.DataDirName
                    + "\tShare name:" + dataDir.DataDirShareName);
            }
            GMThreadSafeLogFile.Log("-------------------------------------------------");

            // Loading the in-progress jobs
            ms_strInProgressJobFileName = strMainDataDir + "\\" + ms_strInProgressJobFileName;
            if (File.Exists(ms_strInProgressJobFileName))
            {
                if (m_inProgressJobs.LoadFromFile(ms_strInProgressJobFileName) != 0)
                {
                    return -1;
                }
            }
            foreach (GalaxyJobStartInfo jobStartInfo in m_inProgressJobs.InProgressJobList)
            {
                if (GetJobAgentDataDir(jobStartInfo.DataRootDir) == null)
                {
                    GMThreadSafeLogFile.Log("Job:" + jobStartInfo.m_jobBasicInfo.ToString() + " 's data root dir:"
                        + jobStartInfo.DataRootDir + " is invalid!");
                }
                else
                {
                    m_waitingJobQueue.Enqueue(jobStartInfo);
                }
            }


            bool fAlwaysRunning = true;
            while (fAlwaysRunning)
            {
                // Reset the events
                m_newJobReceivedEvent.Reset();
                m_allowJobRunningEvent.Reset();

                int iWaitingJobCount = GetWaitingJobCount();
                int iRunningJobCount = GetRunningJobCount();
                // We have some executing resource and have some waiting jobs
                int iToRunJobCount = m_iMaxConcurrentJobCount - iRunningJobCount;
                if (iToRunJobCount > iWaitingJobCount)
                {
                    // We have not enough waiting job
                    iToRunJobCount = iWaitingJobCount;
                }
                for (int iToRunJobIndex = 0; iToRunJobIndex < iToRunJobCount; iToRunJobIndex++)
                {
                    GalaxyJobStartInfo jobStartInfo = DequeueWaitingJobList();
                    // Start a new thread for each job
                    GMThreadSafeLogFile.Log("Running a job:\r\n"
                        + "\tExecutable file name: " + jobStartInfo.ExecutableFileName + "\r\n"
                        + "\tArguments: " + jobStartInfo.Arguments + "\r\n"
                        + "\tJobId: " + jobStartInfo.m_jobBasicInfo.ToString());

                    // Add a job running info, we will update the job process info in the job thread
                    JobRunningInfo jobRunningInfo = new JobRunningInfo(jobStartInfo, null, jobStartInfo.AllowLongIdleTime);
                    AddJobRunningInfo(jobStartInfo.m_jobBasicInfo, jobRunningInfo);

                    ParameterizedThreadStart runJobThreadStart = new ParameterizedThreadStart(this.RunJob);
                    Thread runJobThread = new Thread(runJobThreadStart);
                    runJobThread.Start(jobStartInfo);
                }
                
                // Sometimes we need to waiting for the signals
                if (iWaitingJobCount <= 0)
                {
                    // If there is no waiting jobs, waiting for the new jobs
                    m_newJobReceivedEvent.WaitOne();
                }
                if (iRunningJobCount >= m_iMaxConcurrentJobCount)
                {
                    // If there is no executing resource for the new job,waiting for the resource
                    m_allowJobRunningEvent.WaitOne();
                }
            }

            return 0;
        }

        public int StopJob(Guid jobId)
        {
            int iRet = 0;
            lock (ms_accessJobRunningInfoDictLock)
            {
                if (m_jobRunningInfoDict.ContainsKey(jobId))
                {
                    JobRunningInfo jobRunningInfo = m_jobRunningInfoDict[jobId];
                    Process jobProcess = jobRunningInfo.JobProcess;
                    if (jobProcess != null)
                    {
                        jobRunningInfo.IsUserStopped = true;
                        jobProcess.Kill();
                    }
                }
            }

            return iRet;
        }

        public int RestartJob(Guid jobId)
        {
            int iRet = 0;
            GalaxyJobFinishInfo jobFinishInfo = RemoveFinishedJob(jobId);
            if (jobFinishInfo == null) { return -1; }
            
            // Reconstruct a job start info
            GalaxyJobStartInfo jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.JobId = jobFinishInfo.JobId;
            jobStartInfo.ExecutableFileName = jobFinishInfo.ExecutableFileName;
            jobStartInfo.RelativePath = jobFinishInfo.ExeRelativePath;
            jobStartInfo.Arguments = jobFinishInfo.Arguments;
            jobStartInfo.UserName = jobFinishInfo.UserName;
            jobStartInfo.AutoReportJobStatus = jobFinishInfo.AutoReportJobStatus;
            jobStartInfo.DataRootDir = jobFinishInfo.DataRootDir;

            if (GetJobAgentDataDir(jobStartInfo.DataRootDir) == null)
            {
                GMThreadSafeLogFile.Log("Job:" + jobStartInfo.JobId.ToString() + " 's data root dir:"
                       + jobStartInfo.DataRootDir + " is invalid!");
                iRet = -1;
            }
            else
            {
                // Enqueue the job start info
                AppendJobRequest(jobStartInfo);
            }

            return iRet;
        }

        public int ApplyForNewJob(GalaxyJobBasicInfo jobBasicInfo, out string strDataRootDir)
        {
            // Allocate a new guid
            jobBasicInfo.m_jobId = Guid.NewGuid();
            // Set a device which has lower loads for the job
            JobAgentDataDir dataDirWithLowerLoads = GetDataDirWithLowerLoads();
            strDataRootDir = dataDirWithLowerLoads.DataDirName;
            // Make the data directory for the job
            string strJobDataDir = strDataRootDir + "\\" + jobBasicInfo.m_strProjectName + "\\" + jobBasicInfo.m_strJobName;
            if (Directory.Exists(strJobDataDir))
            {
                // There should no such directory
                GMThreadSafeLogFile.LogError("The directory: " + strJobDataDir + " should not exist!");
                return -1;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(strJobDataDir);
                }
                catch (Exception)
                {
                    // Failed to create the data directory for the job
                    GMThreadSafeLogFile.LogError("Create directory: " + strJobDataDir + " error!");
                    return -1;
                }
            }
            GMThreadSafeLogFile.Log("Create data directory for job: " + jobBasicInfo.ToString() + " successfully.");

            return 0;
        }

        public int AppendJobRequest(GalaxyJobStartInfo jobStartInfo)
        {
            // Append the job request to the waiting queue
            GMThreadSafeLogFile.Log("The job:" + jobStartInfo.JobId.ToString() + " is added.");
            EnqueueWaitingJobList(jobStartInfo);
            // Activate the job agent
            m_newJobReceivedEvent.Set();

            return 0;
        }

        public int GetWaitingJobCount()
        {
            int iWaitingJobCount = 0;
            lock (ms_accessWaitingJobQueueLock)
            {
                iWaitingJobCount = m_waitingJobQueue.Count;
            }

            return iWaitingJobCount;
        }

        public int TransportData(string strProjectDataRootDir, byte[] rgBuf, string strFileName, string strRelativeDir, bool fAppend)
        {
            // If the directory doesn't exist, we create it
            //string strJobDataDir = strDataRootDir + "\\" + jobId.ToString();
            //string strFileDir = strJobDataDir + "\\" + strRelativeDir;
            string strFileDir = strProjectDataRootDir + "\\" + strRelativeDir;
            if (!Directory.Exists(strFileDir))
            {
                try
                {
                    Directory.CreateDirectory(strFileDir);
                }
                catch (Exception)
                {
                    // Create directory error
                    return -1;
                }

            }

            // Write the data to the file
            if (fAppend)
            {
                if (!File.Exists(strFileDir + "\\" + strFileName))
                {
                    // The file doesn't exist while we want to append something to it
                    return -1;
                }
            }
            FileMode fileMode = FileMode.Create;
            if (fAppend)
            {
                fileMode = FileMode.Append;
            }
            try
            {
                BinaryWriter fileWriter = new BinaryWriter(File.Open(strFileDir + "\\" + strFileName, fileMode, FileAccess.Write));
                fileWriter.Write(rgBuf);
                fileWriter.Close();
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }

        public int GetRunningJobCount()
        {
            int iRunningJobCount = 0;
            lock (ms_accessJobRunningInfoDictLock)
            {
                iRunningJobCount = m_jobRunningInfoDict.Count;
            }
            
            return iRunningJobCount;
        }

        public void GetFinishedJobCount(out int iSuccessfulJobCount, out int iFailedJobCount)
        {
            iSuccessfulJobCount = 0;
            iFailedJobCount = 0;
            lock (ms_accessFinishedJobListLock)
            {
                foreach (GalaxyJobFinishInfo jobFinishInfo in m_finishedJobs.FinishedJobDictionary.Values)
                {
                    if (jobFinishInfo.JobRunResult == GalaxyJobRunResult.Successful)
                    {
                        iSuccessfulJobCount++;
                    }
                    else if (jobFinishInfo.JobRunResult == GalaxyJobRunResult.Failed)
                    {
                        iFailedJobCount++;
                    }
                }
            }
        }

        public ProcessNodePerfInfo GetPerfInfo()
        {
            ProcessNodePerfInfo perfInfo = new ProcessNodePerfInfo();
            
            try
            {
                perfInfo.WaitingJobCount = GetWaitingJobCount();
                perfInfo.RunningJobCount = GetRunningJobCount();

                // Get the free space on the disk
                perfInfo.DiskFreeSpace = 0.0;
                List<string> deviceIdList = new List<string>();
                foreach (JobAgentDataDir dataDir in m_dataDirList)
                {
                    string strDeviceId = dataDir.DataDirName.Substring(0, 2).ToLower();
                    if (!deviceIdList.Contains(strDeviceId))
                    {
                        deviceIdList.Add(strDeviceId);
                    }
                    
                }
                foreach (string strDeviceId in deviceIdList)
                {
                    perfInfo.DiskFreeSpace += GetFreeDiskSpaceOnDevice(strDeviceId);
                }

                // Get the cpu usage
                ManagementObjectSearcher processor =
                    new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor");
                foreach (ManagementObject processorInfo in processor.Get())
                {
                    string strProcessorName = processorInfo["Name"].ToString();
                    if (strProcessorName.CompareTo("_Total") == 0)
                    {
                        perfInfo.CPUUsage = double.Parse(processorInfo["PercentProcessorTime"].ToString());
                        perfInfo.CPUUsage = (int)(perfInfo.CPUUsage * 100) / 100.0;
                    }
                }
                
                // Get the memory info
                ManagementObjectSearcher memory =
                   new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PerfFormattedData_PerfOS_Memory");
                foreach (ManagementObject memoryInfo in memory.Get())
                {
                    perfInfo.AvailablePhysicalMemory = double.Parse(memoryInfo["AvailableMBytes"].ToString());
                    perfInfo.AvailablePhysicalMemory = (int)(perfInfo.AvailablePhysicalMemory * 100) / 100.0;
                    perfInfo.AvailableVirtualMemory = double.Parse(memoryInfo["CommitLimit"].ToString()) / 1024 / 1024
                        - double.Parse(memoryInfo["CommittedBytes"].ToString()) / 1024 / 1024;
                    perfInfo.AvailableVirtualMemory = (int)(perfInfo.AvailableVirtualMemory * 100) / 100.0;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return perfInfo;
        }

        #endregion

        #region Threadsafe functions
        
        private void EnqueueWaitingJobList(GalaxyJobStartInfo jobStartInfo)
        {
            lock (ms_accessWaitingJobQueueLock)
            {
                m_waitingJobQueue.Enqueue(jobStartInfo);
                // Add the in progress job
                m_inProgressJobs.AddInProgressJob(jobStartInfo);
                m_inProgressJobs.FlushToDisk(ms_strInProgressJobFileName);
            }
        }

        private GalaxyJobStartInfo DequeueWaitingJobList()
        {
            lock (ms_accessWaitingJobQueueLock)
            {
                return m_waitingJobQueue.Dequeue();
            }
        }

        private bool GetJobRunningInfo(Guid guid, out JobRunningInfo jobRunningInfo)
        {
            jobRunningInfo = null;
            lock (ms_accessJobRunningInfoDictLock)
            {
                if (m_jobRunningInfoDict.ContainsKey(guid))
                {
                    jobRunningInfo = m_jobRunningInfoDict[guid];
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool AddJobRunningInfo(GalaxyJobBasicInfo jobBasicInfo, JobRunningInfo jobRunningInfo)
        {
            lock (ms_accessJobRunningInfoDictLock)
            {
                if (m_jobRunningInfoDict.ContainsKey(jobBasicInfo.JobId))
                {
                    return false;
                }
                else
                {
                    m_jobRunningInfoDict.Add(jobBasicInfo.JobId, jobRunningInfo);
                }
            }

            return true;
        }

        private bool RemoveJobRunningInfo(Guid guid)
        {
            lock (ms_accessJobRunningInfoDictLock)
            {
                if (!m_jobRunningInfoDict.ContainsKey(guid)) { return false; }
                m_jobRunningInfoDict.Remove(guid);
            }

            return true;
        }

        private void AddFinishedJob(GalaxyJobFinishInfo jobFinishInfo)
        {
            lock (ms_accessFinishedJobListLock)
            {
                m_finishedJobs.AddFinishedJob(jobFinishInfo);
                m_finishedJobs.FlushToDisk(ms_strFinishedJobFileName);

                // Remove the in-progress jobs
                m_inProgressJobs.RemoveInProgressJob(jobFinishInfo.JobId);
                m_inProgressJobs.FlushToDisk(ms_strInProgressJobFileName);
            }
        }

        private GalaxyJobFinishInfo RemoveFinishedJob(Guid jobId)
        {
            GalaxyJobFinishInfo removedJobFinishInfo = null;
            lock (ms_accessFinishedJobListLock)
            {
                removedJobFinishInfo = m_finishedJobs.RemoveFinishedJob(jobId);
                if (removedJobFinishInfo != null)
                {
                    m_finishedJobs.FlushToDisk(ms_strFinishedJobFileName);
                }
            }

            return removedJobFinishInfo;
        }

        #endregion

        #region Util functions
        /// <returns>
        ///     null - no corresponding job agent data dir
        /// </returns>
        private JobAgentDataDir GetJobAgentDataDir(string strDataRootDir)
        {
            JobAgentDataDir retJobAgentDataDir = null;
            for (int i = 0; (i < m_dataDirList.Count) && (retJobAgentDataDir == null); i++)
            {
                JobAgentDataDir jobAgentDataDir = m_dataDirList[i];
                if (jobAgentDataDir.DataDirName.ToLower() == strDataRootDir.ToLower())
                {
                    retJobAgentDataDir = jobAgentDataDir;
                }
            }

            return retJobAgentDataDir;
        }

        private double GetFreeDiskSpaceOnDevice(string strDeviceId)
        {
            ManagementObject disk = new
                        ManagementObject("win32_logicaldisk.deviceid=\'" + strDeviceId + "'", null);
            disk.Get();
            double dblDiskFreeSpace = double.Parse(disk["FreeSpace"].ToString()) / 1024.0 / 1024.0;
            dblDiskFreeSpace = (int)(dblDiskFreeSpace * 100) / 100.0;

            return dblDiskFreeSpace;
        }

        private JobAgentDataDir GetDataDirWithLowerLoads()
        {
            JobAgentDataDir dataDirWithLowerLoads = m_dataDirList[0];
            double dblMaxFreeSpace = GetFreeDiskSpaceOnDevice(dataDirWithLowerLoads.DataDirName.Substring(0, 2));
            for (int i = 1; i < m_dataDirList.Count; i++)
            {
                JobAgentDataDir dataDir = m_dataDirList[i];
                double dblFreeSpace = GetFreeDiskSpaceOnDevice(dataDir.DataDirName.Substring(0, 2));
                if (dblFreeSpace > dblMaxFreeSpace)
                {
                    dblMaxFreeSpace = dblFreeSpace;
                    dataDirWithLowerLoads = dataDir;
                }
            }
            return dataDirWithLowerLoads;
        }
        #endregion
    }
}
