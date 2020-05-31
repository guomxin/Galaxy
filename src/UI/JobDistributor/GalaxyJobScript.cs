using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using CommonLib.IO.GMFile;

namespace JobDistributor
{
    class GalaxyNewJob
    {
        #region Variables
        private bool m_fAutoReportStatus;
        private bool m_fAllowLongIdleTime;
        private string m_strJobShortName;
        private string m_strRunExeName;
        private string m_strArguments;
        List<string> m_pnInstanceNameList;
        List<ResourceFilePair> m_resourceFiles;
        #endregion

        public GalaxyNewJob()
        {
            m_pnInstanceNameList = new List<string>();
            m_resourceFiles = new List<ResourceFilePair>();
        }

        #region Properties

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

        public string JobShortName
        {
            get { return m_strJobShortName; }
            set { m_strJobShortName = value; }
        }

        public string RunExeName
        {
            get { return m_strRunExeName; }
            set { m_strRunExeName = value; }
        }

        public string Arguments
        {
            get { return m_strArguments; }
            set { m_strArguments = value; }
        }

        public List<string> PNInstanceNames
        {
            get { return m_pnInstanceNameList; }
            set { m_pnInstanceNameList = value; }
        }

        public List<ResourceFilePair> ResourceFiles
        {
            get { return m_resourceFiles; }
            set { m_resourceFiles = value; }
        }

        #endregion
    }

    class GalaxyUpdateJob
    {
        #region Variables
        private string m_strJobName;
        private List<ResourceFilePair> m_resourceFiles;
        #endregion

        public GalaxyUpdateJob()
        {
            m_resourceFiles = new List<ResourceFilePair>();
        }

        #region Properties

        public string JobName
        {
            get { return m_strJobName; }
            set { m_strJobName = value; }
        }

        public List<ResourceFilePair> ResourceFiles
        {
            get { return m_resourceFiles; }
            set { m_resourceFiles = value; }
        }

        #endregion
    }

    class GalaxyJobScript
    {
        #region Variables
        private List<GalaxyNewJob> m_newJobs;
        private List<GalaxyUpdateJob> m_updateJobs;
        #endregion

        #region Properties
        public List<GalaxyNewJob> NewJobs
        {
            get { return m_newJobs; }
        }

        public List<GalaxyUpdateJob> UpdateJobs
        {
            get { return m_updateJobs; }
        }
        #endregion

        public GalaxyJobScript()
        {
            m_newJobs = new List<GalaxyNewJob>();
            m_updateJobs = new List<GalaxyUpdateJob>();
        }

        /// <returns>
        ///     0 - successfully
        ///     -1 - error
        /// </returns>
        public int LoadScript(string strScriptFileName)
        {
            int iRet = 0;

            try
            {
                XmlDocument scriptDoc = new XmlDocument();
                scriptDoc.Load(strScriptFileName);
                XmlElement rootElement = scriptDoc.DocumentElement;
                XmlNode newJobsNode = rootElement.SelectSingleNode("NewJobs");
                XmlNode updateJobsNode = rootElement.SelectSingleNode("UpdateJobs");

                // Load new jobs
                if (newJobsNode != null)
                {
                    XmlNodeList newJobNodeList = newJobsNode.SelectNodes("NewJob");
                    foreach (XmlNode newJobNode in newJobNodeList)
                    {
                        GalaxyNewJob newJob = new GalaxyNewJob();
                        XmlNode autoReportStatusNode = newJobNode.SelectSingleNode("AutoReportStatus");
                        newJob.AutoReportStatus = bool.Parse(autoReportStatusNode.InnerText.Trim());
                        XmlNode allowLongIdleTimeNode = newJobNode.SelectSingleNode("AllowLongIdleTime");
                        newJob.AllowLongIdleTime = bool.Parse(allowLongIdleTimeNode.InnerText.Trim());
                        XmlNode jobShortNameNode = newJobNode.SelectSingleNode("JobShortName");
                        newJob.JobShortName = jobShortNameNode.InnerText.Trim();
                        XmlNode runExeNode = newJobNode.SelectSingleNode("RunExe");
                        newJob.RunExeName = runExeNode.InnerText.Trim();
                        XmlNode argumentsNode = newJobNode.SelectSingleNode("Arguments");
                        newJob.Arguments = argumentsNode.InnerText.Trim();
                        // Get pn instance name list
                        XmlNode pnInstanceNamesNode = newJobNode.SelectSingleNode("PNInstanceNames");
                        XmlNodeList pnInstanceNameNodeList = pnInstanceNamesNode.SelectNodes("PNInstanceName");
                        foreach (XmlNode pnInstanceNameNode in pnInstanceNameNodeList)
                        {
                            newJob.PNInstanceNames.Add(pnInstanceNameNode.InnerText.Trim());
                        }
                        // Get resource files
                        XmlNode resourceFilesNode = newJobNode.SelectSingleNode("ResourceFiles");
                        XmlNodeList importNodeList = resourceFilesNode.SelectNodes("Import");
                        if (importNodeList != null)
                        {
                            foreach (XmlNode importNode in importNodeList)
                            {
                                string strDirName = importNode.Attributes["dir"].Value;
                                List<string> fileList = GMFileInfo.GetFilesUnderDir(strDirName, true);
                                foreach (string strSrcFileName in fileList)
                                {
                                    string strDstFileName = strSrcFileName.Substring(strDirName.Length + 1);
                                    if (strDirName[strDirName.Length - 1] == '\\')
                                    {
                                        strDstFileName = strSrcFileName.Substring(strDirName.Length);
                                    }
                                    ResourceFilePair resourceFile = new ResourceFilePair(strSrcFileName, strDstFileName);
                                    newJob.ResourceFiles.Add(resourceFile);
                                }
                            }
                        }
                        XmlNodeList resourceFileNodeList = resourceFilesNode.SelectNodes("ResourceFile");
                        if (resourceFileNodeList != null)
                        {
                            foreach (XmlNode resourceFileNode in resourceFileNodeList)
                            {
                                XmlNode srcFileNode = resourceFileNode.SelectSingleNode("SrcFile");
                                string strSrcFileName = srcFileNode.InnerText.Trim();
                                XmlNode dstFileNode = resourceFileNode.SelectSingleNode("DstFile");
                                string strDstFileName = dstFileNode.InnerText.Trim();
                                ResourceFilePair resourceFile = new ResourceFilePair(strSrcFileName, strDstFileName);
                                newJob.ResourceFiles.Add(resourceFile);
                            }
                        }

                        m_newJobs.Add(newJob);
                    }
                }

                // Load update jobs
                if (updateJobsNode != null)
                {
                    XmlNodeList updateJobNodeList = updateJobsNode.SelectNodes("UpdateJob");
                    foreach (XmlNode updateJobNode in updateJobNodeList)
                    {
                        GalaxyUpdateJob updateJob = new GalaxyUpdateJob();
                        XmlNode jobNameNode = updateJobNode.SelectSingleNode("JobName");
                        updateJob.JobName = jobNameNode.InnerText.Trim();

                        // Get resource files
                        XmlNode resourceFilesNode = updateJobNode.SelectSingleNode("ResourceFiles");
                        XmlNodeList importNodeList = resourceFilesNode.SelectNodes("Import");
                        foreach (XmlNode importNode in importNodeList)
                        {
                            string strDirName = importNode.Attributes["dir"].Value;
                            List<string> fileList = GMFileInfo.GetFilesUnderDir(strDirName, true);
                            foreach (string strSrcFileName in fileList)
                            {
                                string strDstFileName = strSrcFileName.Substring(strDirName.Length + 1);
                                if (strDirName[strDirName.Length - 1] == '\\')
                                {
                                    strDstFileName = strSrcFileName.Substring(strDirName.Length);
                                }
                                ResourceFilePair resourceFile = new ResourceFilePair(strSrcFileName, strDstFileName);
                                updateJob.ResourceFiles.Add(resourceFile);
                            }
                        }
                        XmlNodeList resourceFileNodeList = resourceFilesNode.SelectNodes("ResourceFile");
                        foreach (XmlNode resourceFileNode in resourceFileNodeList)
                        {
                            XmlNode srcFileNode = resourceFileNode.SelectSingleNode("SrcFile");
                            string strSrcFileName = srcFileNode.InnerText.Trim();
                            XmlNode dstFileNode = resourceFileNode.SelectSingleNode("DstFile");
                            string strDstFileName = dstFileNode.InnerText.Trim();
                            ResourceFilePair resourceFile = new ResourceFilePair(strSrcFileName, strDstFileName);
                            updateJob.ResourceFiles.Add(resourceFile);
                        }

                        m_updateJobs.Add(updateJob);
                    }
                }
            }
            catch (Exception)
            {
                iRet = -1;
            }

            return iRet;
        }
    }
}
