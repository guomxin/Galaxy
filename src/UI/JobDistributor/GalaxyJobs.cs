using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.RemoteInterfaces;
using System.Xml;
using System.IO;

namespace JobDistributor
{
    class GalaxyJobs
    {
        #region Variables
        private Dictionary<string, GalaxyJob> m_jobDictRefByJobName;
        private object m_writeFileLock;
        #endregion

        #region Properties
        public Dictionary<string, GalaxyJob> JobDictionaryRefByJobName
        {
            get { return m_jobDictRefByJobName; }
        }
        #endregion

        public GalaxyJobs()
        {
            m_jobDictRefByJobName = new Dictionary<string, GalaxyJob>();
            m_writeFileLock = new object();
        }

        #region Public methods
        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        public int LoadFromFile(string strJobFileName)
        {
            int iRet = 0;
            try
            {
                XmlDocument jobDoc = new XmlDocument();
                jobDoc.Load(strJobFileName);
                XmlElement root = jobDoc.DocumentElement;
                XmlNodeList jobNodeList = root.SelectNodes("GalaxyJob");
                foreach (XmlNode jobNode in jobNodeList)
                {
                    XmlNode jobNameNode = jobNode.SelectSingleNode("JobName");
                    string strJobName = jobNameNode.InnerText.Trim();
                    XmlNode jobInstanceNameNode = jobNode.SelectSingleNode("PNInstanceName");
                    string strPNInstanceName = jobInstanceNameNode.InnerText.Trim();
                    XmlNode jobIdNode = jobNode.SelectSingleNode("JobId");
                    Guid jobId = new Guid(jobIdNode.InnerText.Trim());
                    XmlNode jobStatusNode = jobNode.SelectSingleNode("JobStatus");
                    GalaxyJobStatus jobStatus = GalaxyJob.String2JobStatus(jobStatusNode.InnerText.Trim());
                    XmlNode exeNameNode = jobNode.SelectSingleNode("ExeFileName");
                    string strExeFileName = exeNameNode.InnerText.Trim();
                    XmlNode relPathNode = jobNode.SelectSingleNode("RelativePath");
                    string strRelativePath = relPathNode.InnerText.Trim();
                    XmlNode argsNode = jobNode.SelectSingleNode("Arguments");
                    string strArgs = argsNode.InnerText.Trim();
                    XmlNode dataRootDirNode = jobNode.SelectSingleNode("DataRootDir");
                    string strProjectDataRootDir = dataRootDirNode.InnerText.Trim();
                    XmlNode userNameNode = jobNode.SelectSingleNode("UserName");
                    string strUserName = userNameNode.InnerText.Trim();
                    XmlNode autoReportStatusNode = jobNode.SelectSingleNode("AutoReportStatus");
                    bool fAutoReportStatus = bool.Parse(autoReportStatusNode.InnerText.Trim());
                    XmlNode allowLongIdleTimeNode = jobNode.SelectSingleNode("AllowLongIdleTime");
                    bool fAllowLongIdleTime = bool.Parse(allowLongIdleTimeNode.InnerText.Trim());
                    XmlNode outputDirNode = jobNode.SelectSingleNode("OutputBaseDir");
                    string strOutputBaseDir = outputDirNode.InnerText.Trim();
                    // Get resource file list
                    XmlNode resourceFilesNode = jobNode.SelectSingleNode("ResourceFiles");
                    XmlNodeList resourceFileNodeList = resourceFilesNode.SelectNodes("ResourceFile");
                    List<ResourceFilePair> resourceFileList = new List<ResourceFilePair>();
                    foreach (XmlNode resourceFileNode in resourceFileNodeList)
                    {
                        XmlNode srcFileNode = resourceFileNode.SelectSingleNode("SrcFileName");
                        XmlNode dstRelFileNode = resourceFileNode.SelectSingleNode("DstRelativeFileName");
                        ResourceFilePair resourceFile = new ResourceFilePair(srcFileNode.InnerText.Trim(), dstRelFileNode.InnerText.Trim());
                        resourceFileList.Add(resourceFile);
                    }
                    // Get dependent jobs
                    XmlNode dependentJobsNode = jobNode.SelectSingleNode("DependentJobs");
                    XmlNodeList dependentJobNodeList = dependentJobsNode.SelectNodes("DependentJob");
                    List<string> dependentJobs = new List<string>();
                    foreach (XmlNode dependentJobNode in dependentJobNodeList)
                    {
                        string strDependentJobName = dependentJobNode.InnerText.Trim();
                        dependentJobs.Add(strDependentJobName);
                    }
                    if (m_jobDictRefByJobName.ContainsKey(strJobName))
                    {
                        return -1;
                    }
                    else
                    {
                        GalaxyJob job = new GalaxyJob();
                        job.AllowLongIdleTime = fAllowLongIdleTime;
                        job.Arguments = strArgs;
                        job.AutoReportStatus = fAutoReportStatus;
                        job.DependentJobs = dependentJobs;
                        job.ExeFileName = strExeFileName;
                        job.JobId = jobId;
                        job.JobStatus = jobStatus;
                        job.OutputBaseDir = strOutputBaseDir;
                        job.PNInstanceName = strPNInstanceName;
                        job.RelativePath = strRelativePath;
                        job.ResourceFileList = resourceFileList;
                        job.JobName = strJobName;
                        job.UserName = strUserName;
                        job.ProjectDataRootDir = strProjectDataRootDir;
                        AddJob(job);
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
        public int FlushToDisk(string strJobFileName)
        {
            int iRet = 0;
            lock (m_writeFileLock)
            {
                try
                {
                    XmlDocument jobDoc = new XmlDocument();
                    XmlDeclaration docDeclaration = jobDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                    jobDoc.AppendChild(docDeclaration);
                    XmlElement root = jobDoc.CreateElement("GalaxyJobs");
                    jobDoc.AppendChild(root);
                    // Flush each jobs
                    foreach (GalaxyJob job in m_jobDictRefByJobName.Values)
                    {
                        XmlElement jobElement = jobDoc.CreateElement("GalaxyJob");

                        XmlElement jobNameElement = jobDoc.CreateElement("JobName");
                        XmlCDataSection jobNameData = jobDoc.CreateCDataSection(job.JobName);
                        jobNameElement.AppendChild(jobNameData);
                        jobElement.AppendChild(jobNameElement);

                        XmlElement pnInstanceNameElement = jobDoc.CreateElement("PNInstanceName");
                        pnInstanceNameElement.InnerText = job.PNInstanceName;
                        jobElement.AppendChild(pnInstanceNameElement);

                        XmlElement jobIdElement = jobDoc.CreateElement("JobId");
                        jobIdElement.InnerText = job.JobId.ToString();
                        jobElement.AppendChild(jobIdElement);

                        XmlElement jobStatusElement = jobDoc.CreateElement("JobStatus");
                        jobStatusElement.InnerText = GalaxyJob.JobStatus2String(job.JobStatus);
                        jobElement.AppendChild(jobStatusElement);

                        XmlElement exeNameElement = jobDoc.CreateElement("ExeFileName");
                        exeNameElement.InnerText = job.ExeFileName;
                        jobElement.AppendChild(exeNameElement);

                        XmlElement relPathElement = jobDoc.CreateElement("RelativePath");
                        XmlCDataSection relPathData = jobDoc.CreateCDataSection(job.RelativePath);
                        relPathElement.AppendChild(relPathData);
                        jobElement.AppendChild(relPathElement);

                        XmlElement userNameElement = jobDoc.CreateElement("UserName");
                        userNameElement.InnerText = job.UserName;
                        jobElement.AppendChild(userNameElement);

                        XmlElement argsElement = jobDoc.CreateElement("Arguments");
                        XmlCDataSection argsData = jobDoc.CreateCDataSection(job.Arguments);
                        argsElement.AppendChild(argsData);
                        jobElement.AppendChild(argsElement);

                        XmlElement autoReportStatusElement = jobDoc.CreateElement("AutoReportStatus");
                        autoReportStatusElement.InnerText = job.AutoReportStatus.ToString();
                        jobElement.AppendChild(autoReportStatusElement);

                        XmlElement allowLongIdleTimeElement = jobDoc.CreateElement("AllowLongIdleTime");
                        allowLongIdleTimeElement.InnerText = job.AllowLongIdleTime.ToString();
                        jobElement.AppendChild(allowLongIdleTimeElement);

                        XmlElement outputDirElement = jobDoc.CreateElement("OutputBaseDir");
                        XmlCDataSection outputDirData = jobDoc.CreateCDataSection(job.OutputBaseDir);
                        outputDirElement.AppendChild(outputDirData);
                        jobElement.AppendChild(outputDirElement);

                        XmlElement dataRootDirElement = jobDoc.CreateElement("DataRootDir");
                        XmlCDataSection dataRootDirData = jobDoc.CreateCDataSection(job.ProjectDataRootDir);
                        dataRootDirElement.AppendChild(dataRootDirData);
                        jobElement.AppendChild(dataRootDirElement);

                        // Add resource files
                        XmlElement resourceFilesElement = jobDoc.CreateElement("ResourceFiles");
                        foreach (ResourceFilePair resourceFile in job.ResourceFileList)
                        {
                            XmlElement resourceFileElement = jobDoc.CreateElement("ResourceFile");
                            // source file name
                            XmlElement srcFileElement = jobDoc.CreateElement("SrcFileName");
                            XmlCDataSection srcFileData = jobDoc.CreateCDataSection(resourceFile.SrcFileName);
                            srcFileElement.AppendChild(srcFileData);
                            resourceFileElement.AppendChild(srcFileElement);
                            // destination relative file name
                            XmlElement dstRelFileElement = jobDoc.CreateElement("DstRelativeFileName");
                            XmlCDataSection dstRelFileData = jobDoc.CreateCDataSection(resourceFile.DstRelativeFileName);
                            dstRelFileElement.AppendChild(dstRelFileData);
                            resourceFileElement.AppendChild(dstRelFileElement);
                            resourceFilesElement.AppendChild(resourceFileElement);
                        }
                        jobElement.AppendChild(resourceFilesElement);

                        // Add dependent jobs
                        XmlElement dependentJobsElement = jobDoc.CreateElement("DependentJobs");
                        foreach (string strDependentJobName in job.DependentJobs)
                        {
                            XmlElement dependentJobElement = jobDoc.CreateElement("DependentJob");
                            XmlCDataSection dependentJobNameData = jobDoc.CreateCDataSection(strDependentJobName);
                            dependentJobElement.AppendChild(dependentJobNameData);
                            dependentJobsElement.AppendChild(dependentJobElement);
                        }
                        jobElement.AppendChild(dependentJobsElement);

                        root.AppendChild(jobElement);
                    }
                    jobDoc.Save(strJobFileName);
                }
                catch (Exception)
                {
                    iRet = -1;
                }
            }

            return iRet;
        }

        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        public int AddJob(GalaxyJob job)
        {
            if (m_jobDictRefByJobName.ContainsKey(job.JobName)) { return -1; }
            m_jobDictRefByJobName.Add(job.JobName, job);

            return 0;
        }

        /// <returns>
        ///     null - failed
        /// </returns>
        public GalaxyJob GetJob(string strJobName)
        {
            GalaxyJob job = null;
            if (m_jobDictRefByJobName.ContainsKey(strJobName))
            {
                job = m_jobDictRefByJobName[strJobName];
            }

            return job;
        }


        public bool IsJobExist(string strJobName)
        {
            return m_jobDictRefByJobName.ContainsKey(strJobName);
        }

        /// <returns>
        ///     null - failed
        /// </returns>
        public GalaxyJob GetJob(Guid jobId)
        {
            GalaxyJob retJob = null;
            foreach (GalaxyJob job in m_jobDictRefByJobName.Values)
            {
                if (job.JobId == jobId)
                {
                    retJob = job;
                    break;
                }
            }

            return retJob;
        }

        #endregion

    }

    public class ResourceFilePair
    {
        #region Variables
        private string m_strSrcFileName;
        private string m_strDstRelativeFileName;
        #endregion

        public ResourceFilePair(string strSrcFileName, string strDstRelativeFileName)
        {
            m_strSrcFileName = strSrcFileName;
            m_strDstRelativeFileName = strDstRelativeFileName;
        }

        #region Properties
        public string SrcFileName
        {
            get { return m_strSrcFileName; }
        }

        public string DstRelativeFileName
        {
            get { return m_strDstRelativeFileName; }
        }
        #endregion
    }

    public class GalaxyJob
    {
        #region Variables
        private string m_strProjectName;
        private string m_strJobName;
        private string m_strExeFileName;
        private string m_strRelativePath;
        private string m_strArguments;
        // The data root dir on the job agent
        private string m_strProjectDataRootDir;
        private string m_strUserName;
        private bool m_fAutoReportStatus;
        private bool m_fAllowLongIdleTime;
        private string m_strOutputBaseDir;
        private string m_strPNInstanceName;
        private Guid m_jobId;
        private GalaxyJobStatus m_jobStatus;
        // !!!Currently we cannot make sure the resource files are updated if we update them after the job is deployed at the first time
        List<ResourceFilePair> m_resourceFileList; // empty not NULL, if NONE
        List<string> m_dependentJobs; // empty not NULL, if NONE
        GalaxyJobProperties m_jobProps; // !!!NoSerialized
        #endregion

        #region Properties
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

        public string PNInstanceName
        {
            get { return m_strPNInstanceName; }
            set { m_strPNInstanceName = value; }
        }

        public Guid JobId
        {
            get { return m_jobId; }
            set { m_jobId = value; }
        }

        public GalaxyJobStatus JobStatus
        {
            get { return m_jobStatus; }
            set { m_jobStatus = value; }
        }

        public string ExeFileName
        {
            get { return m_strExeFileName; }
            set { m_strExeFileName = value; }
        }

        public string RelativePath
        {
            get { return m_strRelativePath; }
            set { m_strRelativePath = value; }
        }

        public string Arguments
        {
            get { return m_strArguments; }
            set { m_strArguments = value; }
        }

        public string ProjectDataRootDir
        {
            get { return m_strProjectDataRootDir; }
            set { m_strProjectDataRootDir = value; }
        }

        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }

        public bool AutoReportStatus
        {
            get { return m_fAutoReportStatus; }
            set { m_fAutoReportStatus = value; }
        }

        public bool AllowLongIdleTime
        {
            get { return m_fAllowLongIdleTime; }
            set { m_fAllowLongIdleTime = value; }
        }

        public string OutputBaseDir
        {
            get { return m_strOutputBaseDir; }
            set { m_strOutputBaseDir = value; }
        }

        public List<ResourceFilePair> ResourceFileList
        {
            get { return m_resourceFileList; }
            set { m_resourceFileList = value; }
        }

        public List<string> DependentJobs
        {
            get { return m_dependentJobs; }
            set { m_dependentJobs = value; }
        }

        public GalaxyJobProperties JobProperties
        {
            get { return m_jobProps; }
            set { m_jobProps = value; }
        }

        #endregion

        public GalaxyJob()
        {
            m_resourceFileList = new List<ResourceFilePair>();
            m_dependentJobs = new List<string>();
            m_jobProps = new GalaxyJobProperties();
        }

        #region Static methods

        public static string JobStatus2String(GalaxyJobStatus jobStatus)
        {
            string strJobStatus = "";
            switch (jobStatus)
            {
                case GalaxyJobStatus.Unknown:
                    strJobStatus = "Unknown";
                    break;
                case GalaxyJobStatus.Waiting:
                    strJobStatus = "Waiting";
                    break;
                case GalaxyJobStatus.Queued:
                    strJobStatus = "Queued";
                    break;
                case GalaxyJobStatus.Running:
                    strJobStatus = "Running";
                    break;
                case GalaxyJobStatus.Successful:
                    strJobStatus = "Successful";
                    break;
                case GalaxyJobStatus.Failed:
                    strJobStatus = "Failed";
                    break;
            }

            return strJobStatus;
        }

        public static GalaxyJobStatus String2JobStatus(string strJobStatus)
        {
            GalaxyJobStatus jobStatus = GalaxyJobStatus.Unknown;
            if (strJobStatus == "Queued")
            {
                jobStatus = GalaxyJobStatus.Queued;
            }
            else if (strJobStatus == "Waiting")
            {
                jobStatus = GalaxyJobStatus.Waiting;
            }
            else if (strJobStatus == "Running")
            {
                jobStatus = GalaxyJobStatus.Running;
            }
            else if (strJobStatus == "Successful")
            {
                jobStatus = GalaxyJobStatus.Successful;
            }
            else if (strJobStatus == "Failed")
            {
                jobStatus = GalaxyJobStatus.Failed;
            }

            return jobStatus;
        }

        #endregion
    }
}
