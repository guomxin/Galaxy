using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Galaxy.ManagedClientLib;
using Galaxy.RemoteInterfaces;

namespace GalaxyDeploymentClient
{
    class GalaxyDeploymentClient
    {
        #region public variables
        #endregion //public variables

        #region non-public variables
        protected GalaxyDeploymentCfg m_cfg = new GalaxyDeploymentCfg();
        #endregion //non-public variables

        #region public member functions

        public bool Init(string strCfgFile)
        {
            bool bRet = m_cfg.Load(strCfgFile);
            return bRet;
        }

        public bool Process()
        {
            bool bRet = true;
            GalaxyJobBasicInfo[] projects = new GalaxyJobBasicInfo[m_cfg.m_procNodes.Count];
            string[] projectDataRootDirs = new string[m_cfg.m_procNodes.Count];
            JobAgentHelper[] jobAgentHelpers = new JobAgentHelper[m_cfg.m_procNodes.Count];
            for (int idx = 0; idx < jobAgentHelpers.Length; idx++)
            {
                ProcessingNodeInfo pnInfo = m_cfg.m_procNodes[idx];
                jobAgentHelpers[idx] = new JobAgentHelper();
                bRet = jobAgentHelpers[idx].Initialize(pnInfo.m_strHostName, pnInfo.m_nPort);
                if (!bRet) {
                    return false;
                }

                projects[idx] = new GalaxyJobBasicInfo();
                projects[idx].m_strProjectName = m_cfg.m_projectInfo.m_strName;
                projects[idx].m_strJobName = m_cfg.m_projectInfo.m_jobs[0].m_strName;

                int nRet = jobAgentHelpers[idx].ApplyForNewJob(projects[idx], out projectDataRootDirs[idx]);
                if(nRet != 0) {
                    bRet = false;
                    return false;
                }
                projectDataRootDirs[idx] += ("\\" + m_cfg.m_projectInfo.m_strName);
            }

            for (int idx = 0; bRet && idx < jobAgentHelpers.Length; idx++)
            {
                bRet = TransProjectFiles(jobAgentHelpers[idx], idx, projectDataRootDirs[idx]);
            }

            for (int idx = 0; bRet && idx < jobAgentHelpers.Length; idx++)
            {
                int nRet = jobAgentHelpers[idx].RestartJob(projects[idx].m_jobId);
                if (nRet != 0) {
                    bRet = false;
                    return false;
                }
            }

            return bRet;
        }

        #endregion //public member functions

        #region non-public member functions

        protected bool TransProjectFiles(JobAgentHelper agentHelper, int nProcNodeIdx, string strProjectDataRootDir)
        {
            bool bRet = true;
            foreach (string strProjectDir in m_cfg.m_projectInfo.m_dirs)
            {
                bRet = TransDir(agentHelper, strProjectDataRootDir, strProjectDir, "");
                if (!bRet) {
                    break;
                }
            }
            return bRet;
        }

        protected bool TransDir(JobAgentHelper agentHelper, string strProjectDataRootDir, string strDir, string strRemoteRelaDirName)
        {
            bool bRet = true;
            DirectoryInfo projectDir = new DirectoryInfo(strDir);
            FileInfo[] files = projectDir.GetFiles();
            foreach (FileInfo curFile in files)
            {
                int nTransRet = agentHelper.TransportFile(strProjectDataRootDir, curFile.FullName, curFile.Name, strRemoteRelaDirName, 1024 * 1024);
                if (nTransRet != 0) {
                    bRet = false;
                    break;
                }
            }

            DirectoryInfo[] dirs = projectDir.GetDirectories();
            foreach (DirectoryInfo curDir in dirs)
            {
                string strTempRemoteRelaDirName = strRemoteRelaDirName + "\\" + curDir.Name;
                bool bSucc = TransDir(agentHelper, strProjectDataRootDir, curDir.FullName, strTempRemoteRelaDirName);
                if (!bSucc) {
                    bRet = false;
                    break;
                }
            }

            return bRet;
        }

        #endregion //non-public member functions
    }
}
