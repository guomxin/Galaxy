using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GalaxyDeploymentClient
{
    class JobInfo
    {
        public string m_strName;
        public string m_strAppPath;
        public List<string> m_dirs = new List<string>();
    }

    class ProjectInfo
    {
        public string m_strName;
        public List<string> m_dirs = new List<string>();
        public List<JobInfo> m_jobs = new List<JobInfo>();
    }

    class ProcessingNodeInfo
    {
        public int m_id;
        public string m_strHostName;
        public string m_strIPAddress;
        public int m_nPort = 12300;
        public List<string> m_jobList = new List<string>();
    }

    class Options
    {
        public bool m_bIncrementalDeployment = false;
        public bool m_bStopJobsIfRunning = true;
        public bool OverwriteExistingFiles = true;
    }

    class GalaxyDeploymentCfg
    {
        public ProjectInfo m_projectInfo = new ProjectInfo();
        int m_nDefaultDeploymentServerPort = 12300;
        public List<ProcessingNodeInfo> m_procNodes = new List<ProcessingNodeInfo>();
        public Options m_options = new Options();

        public bool Load(string strCfgFile)
        {
            bool bRet = true;
            try
            {
                XmlDocument cfgDoc = new XmlDocument();
                cfgDoc.Load(strCfgFile);

                XmlNode rootElement = cfgDoc.DocumentElement;
                if (!rootElement.LocalName.Equals("GalaxyProjectDeployment", StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }

                // load project information
                XmlNode projectNode = rootElement.SelectSingleNode("Project");
                XmlAttribute attrProjectName = projectNode.Attributes["Name"];
                m_projectInfo.m_strName = attrProjectName.Value;
                XmlNodeList dirNodeList = projectNode.SelectNodes("Dirs/Dir");
                foreach (XmlNode dirNode in dirNodeList)
                {
                    m_projectInfo.m_dirs.Add(dirNode.InnerText);
                }
                XmlNodeList jobNodeList = projectNode.SelectNodes("Jobs/Job");
                foreach (XmlNode jobNode in jobNodeList)
                {
                    JobInfo jobInfo = new JobInfo();
                    jobInfo.m_strName = jobNode.SelectSingleNode("Name").InnerText;
                    jobInfo.m_strAppPath = jobNode.SelectSingleNode("AppPath").InnerText;
                    XmlNodeList jobDirNodeList = jobNode.SelectNodes("Dirs/Dir");
                    foreach(XmlNode jobDirNode in jobDirNodeList)
                    {
                        jobInfo.m_dirs.Add(jobDirNode.InnerText);
                    }
                    m_projectInfo.m_jobs.Add(jobInfo);
                }

                // load processing nodes
                m_nDefaultDeploymentServerPort = Int32.Parse(rootElement.SelectSingleNode("ProcessingNodes/DefaultDeploymentServerPort").InnerText);
                XmlNodeList procNodeList = rootElement.SelectNodes("ProcessingNodes/ProcessNode");
                foreach (XmlNode xmlNodeProcNode in procNodeList)
                {
                    ProcessingNodeInfo procNodeInfo = new ProcessingNodeInfo();
                    procNodeInfo.m_id = Int32.Parse(xmlNodeProcNode.Attributes["Id"].Value);
                    procNodeInfo.m_strHostName = xmlNodeProcNode.Attributes["HostName"].Value;
                    procNodeInfo.m_strIPAddress = xmlNodeProcNode.Attributes["IPAddress"].Value;
                    procNodeInfo.m_nPort = Int32.Parse(xmlNodeProcNode.Attributes["PnPort"].Value);
                    string strJobList = xmlNodeProcNode.Attributes["JobList"].Value;
                    string separator = "+ ";
                    string[] jobList = strJobList.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string strJob in jobList)
                    {
                        procNodeInfo.m_jobList.Add(strJob);
                    }
                    m_procNodes.Add(procNodeInfo);
                }

                // load deployment options
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
                bRet = false;
            }

            return bRet;
        }
    }
}
