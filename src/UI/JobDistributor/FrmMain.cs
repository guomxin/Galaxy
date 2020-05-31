using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Galaxy.ManagedClientLib;
using Galaxy.RemoteInterfaces;
using CommonLib.IO.GMFile;
using System.Diagnostics;
using JobDistributor.UI;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace JobDistributor
{
    enum JobItemIndex
    {
        JobName = 0,
        ExeName,
        Arguments,
        PNName,
        Status,
        OutputBaseDir,

        JobPropertyStart
    }

    enum PNItemIndex
    {
        InstanceName = 0,
        WaitingJobCount,
        RunningJobCount,
        CPUUsage,
        FreeDiskSpace,
        AvailablePhysicalMemory,
        AvailableVirtualMemory,
        QueryTime,

        PNItemCount
    }

    enum OutputInfoCat
    {
        Information = 0,
        Error,

        OutputInfoCatCount
    }

    public partial class FrmMain : Form
    {
        #region User defined variables
        private JobEventSinker m_jobEventSinker;
        private GalaxyJobStartEventHandler m_jobStartEventHandler;
        private GalaxyJobFinishEventHandler m_jobFinishEventHandler;

        private static object ms_refreshJobListLock;
        private static object ms_accessJobAgentHelperDictLock;
        private Dictionary<string, PNInstance> m_pnDict;
        private Dictionary<string, JobAgentHelper> m_jobAgentHelperDict;

        // Item sorters
        PNInfoListItemSorter m_pnInfoListItemSorter;
        JobInfoListItemSorter m_jobInfoListItemSorter;

        private GalaxyJobs m_jobs;
        #endregion

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            // Parse config file
            JobDistributorConfig config = new JobDistributorConfig();
            if (!config.ParseFromConfigFile(Constants.ms_strConfigFileName))
            {
                MessageBox.Show("Parse config file:" + Constants.ms_strConfigFileName + " error!");
                return;
            }
            m_pnDict = config.PNDictionary;

            // Set the sorter and fill the pn list view
            m_pnInfoListItemSorter = new PNInfoListItemSorter();
            lvPNStatuses.ListViewItemSorter = m_pnInfoListItemSorter;
            FillPNView();
            // Set the sorter of job list view
            m_jobInfoListItemSorter = new JobInfoListItemSorter();
            lvJobs.ListViewItemSorter = m_jobInfoListItemSorter;

            foreach (string strPNName in m_pnDict.Keys)
            {
                clbPNInstances.Items.Add(strPNName);
            }

            // Initialize the listening channel
            int iPort = config.ListeningPort;
            m_jobEventSinker = new JobEventSinker();
            if (!m_jobEventSinker.Initialize(iPort))
            {
                MessageBox.Show("Initialize event sinker at port:" + iPort.ToString() + " error!");
                Application.Exit();
            }
            m_jobStartEventHandler = new GalaxyJobStartEventHandler(m_jobEventSinker.OnJobStarted);
            m_jobFinishEventHandler = new GalaxyJobFinishEventHandler(m_jobEventSinker.OnJobFinished);
            m_jobEventSinker.JobStartCallBack += new JobStartEventHandler(this.JobStarted);
            m_jobEventSinker.JobFinishCallBack += new JobFinishEventHandler(this.JobFinished);

            // Initialize the lock
            ms_accessJobAgentHelperDictLock = new object();
            ms_refreshJobListLock = new object();

            // Load the existing jobs
            cbJobExeName.Items.Add("__AllJob__");
            txbRefreshInterval.Text = (Constants.ms_iCheckJobStatusInterval / 1000).ToString();
            m_jobs = new GalaxyJobs();
            if (File.Exists(Constants.ms_strJobFileName))
            {
                if (m_jobs.LoadFromFile(Constants.ms_strJobFileName) != 0)
                {
                    MessageBox.Show("Loading jobs error!");
                    Application.Exit();
                }
            }
            // Query the status of the existed job
            foreach (GalaxyJob job in m_jobs.JobDictionaryRefByJobName.Values)
            {
                AddJob(job);
            }
            QueryJobStatusHandler queryJobStatus = new QueryJobStatusHandler(this.QueryJobStatus);
            queryJobStatus.BeginInvoke(null, null);

            // Set the interval of the timer for updating job status
            tmUpdateStatus.Interval = Constants.ms_iCheckJobStatusInterval;
        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Remove the listeners
            if (m_jobAgentHelperDict != null)
            {
                foreach (string strPNName in m_jobAgentHelperDict.Keys)
                {
                    JobAgentHelper jobAgentHelper = m_jobAgentHelperDict[strPNName];

                    // Unadvise the event handlers
                    jobAgentHelper.RemoveJobStartListener(m_jobStartEventHandler);
                    jobAgentHelper.RemoveJobFinishListener(m_jobFinishEventHandler);
                }
            }
        }

        private void tsmnuiExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void tsmnuiAboutMe_Click(object sender, EventArgs e)
        {
            FrmAbout frmAbout = new FrmAbout();
            frmAbout.Show();
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private void ScrollTextBoxEnd(RichTextBox tb)
        {
            const int WM_VSCROLL = 277;
            const int SB_BOTTOM = 7;

            IntPtr ptrWparam = new IntPtr(SB_BOTTOM);
            IntPtr ptrLparam = new IntPtr(0);
            SendMessage(tb.Handle, WM_VSCROLL, ptrWparam, ptrLparam);
        }

        private delegate void AppendOutputInfoHandler(string strText, OutputInfoCat infoCat);

        private void AppendOutputInfo(string strText, OutputInfoCat infoCat)
        {
            Color color = Color.Black;
            switch (infoCat)
            {
                case OutputInfoCat.Information:
                    color = Color.Black;
                    break;
                case OutputInfoCat.Error:
                    color = Color.Red;
                    break;
            }
            int iSelectionStart = txbInfo.Text.Length;
            string strAppendString = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "   " + strText + "\r";
            txbInfo.AppendText(strAppendString);
            txbInfo.SelectionStart = iSelectionStart;
            txbInfo.SelectionLength = strAppendString.Length;
            txbInfo.SelectionColor = color;
            ScrollTextBoxEnd(txbInfo);
        }

        #region Submit job tab page

        private bool ValidateJobScript(GalaxyJobScript jobScript)
        {
            if ((jobScript.NewJobs.Count == 0) && (jobScript.UpdateJobs.Count == 0))
            {
                AppendOutputInfo("Job script is empty!", OutputInfoCat.Error);
                return false;
            }
            if (jobScript.NewJobs.Count > 0)
            {
                foreach (GalaxyNewJob newJob in jobScript.NewJobs)
                {
                    if (newJob.PNInstanceNames.Count == 0)
                    {
                        AppendOutputInfo(newJob.JobShortName + " has no PN Instance!", OutputInfoCat.Error);
                        return false;
                    }
                    else
                    {
                        foreach (string strPNInstanceName in newJob.PNInstanceNames)
                        {
                            string strJobName = newJob.JobShortName + "@" + strPNInstanceName;
                            if (m_jobs.IsJobExist(strJobName))
                            {
                                AppendOutputInfo(newJob.JobShortName + " on " + strPNInstanceName + " is alreay existed!", OutputInfoCat.Error);
                                return false;
                            }
                        }
                    }
                }
            }
            if (jobScript.UpdateJobs.Count > 0)
            {
                foreach (GalaxyUpdateJob updateJob in jobScript.UpdateJobs)
                {
                    if (!m_jobs.IsJobExist(updateJob.JobName))
                    {
                        AppendOutputInfo(updateJob.JobName + " doesn't exist!", OutputInfoCat.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private void btnValidateJobScriptFile_Click(object sender, EventArgs e)
        {
            GalaxyJobScript jobScriptFile = new GalaxyJobScript();
            if (jobScriptFile.LoadScript(txbJobScriptFileName.Text) == 0)
            {
                if (ValidateJobScript(jobScriptFile))
                {
                    AppendOutputInfo("Job script file is correct.", OutputInfoCat.Information);
                }
                else
                {
                    AppendOutputInfo("Job script file is wrong!", OutputInfoCat.Error);
                }
            }
            else
            {
                AppendOutputInfo("Job script file is wrong!", OutputInfoCat.Error);
            }
        }

        private void btnBrowseJobScriptFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();
            dlgOpen.RestoreDirectory = true;
            dlgOpen.Title = "Designate a job script file";
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                txbJobScriptFileName.Text = dlgOpen.FileName;
            }
        }

        private void btnSubmitJobScript_Click(object sender, EventArgs e)
        {
            GalaxyJobScript jobScriptFile = new GalaxyJobScript();
            if (jobScriptFile.LoadScript(txbJobScriptFileName.Text) == 0)
            {
                if (ValidateJobScript(jobScriptFile))
                {
                    // Submit new jobs
                    foreach (GalaxyNewJob newJobReq in jobScriptFile.NewJobs)
                    {
                        string strExecutableFileName = newJobReq.RunExeName;
                        string strExecutableShortFileName = "";
                        string strExecutableRelativeFilePath = "";
                        int iLastSlashPos = strExecutableFileName.LastIndexOf('\\');
                        if (iLastSlashPos == -1)
                        {
                            strExecutableShortFileName = strExecutableFileName;
                            strExecutableRelativeFilePath = "";
                        }
                        else
                        {
                            strExecutableShortFileName = strExecutableFileName.Substring(iLastSlashPos + 1);
                            strExecutableRelativeFilePath = strExecutableFileName.Substring(0, iLastSlashPos);
                        }

                        foreach (string strPNInstanceName in newJobReq.PNInstanceNames)
                        {
                            GalaxyJob job = new GalaxyJob();
                            job.AllowLongIdleTime = newJobReq.AllowLongIdleTime;
                            job.Arguments = newJobReq.Arguments;
                            job.AutoReportStatus = newJobReq.AutoReportStatus;
                            job.ExeFileName = strExecutableShortFileName;
                            job.JobId = Guid.Empty;
                            job.JobName = newJobReq.JobShortName + "@" + strPNInstanceName;
                            job.JobStatus = GalaxyJobStatus.Queued;
                            job.OutputBaseDir = "";
                            job.PNInstanceName = strPNInstanceName;
                            job.RelativePath = strExecutableRelativeFilePath;
                            job.ResourceFileList = newJobReq.ResourceFiles;
                            job.UserName = Environment.UserDomainName + "\\" + Environment.UserName;
                            AddJob(job);

                            RunJobOnPNInstanceHandler runJobOnPN = new RunJobOnPNInstanceHandler(this.RunJobOnPNInstance);
                            runJobOnPN.BeginInvoke(job, null, null);
                        }
                    }

                    // Update jobs
                    foreach (GalaxyUpdateJob updateJob in jobScriptFile.UpdateJobs)
                    {
                        UpdateResourceFilesHandler updateResourceFiles = new UpdateResourceFilesHandler(this.UpdateResourceFiles);
                        updateResourceFiles.BeginInvoke(updateJob.JobName, updateJob.ResourceFiles, null, null);
                    }
                }
                else
                {
                    AppendOutputInfo("Job script file is wrong!", OutputInfoCat.Error);
                }
            }
            else
            {
                AppendOutputInfo("Job script file is wrong!", OutputInfoCat.Error);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();
            dlgOpen.RestoreDirectory = true;
            dlgOpen.Title = "Designate a resource file for the job";
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                txbSrcFileName.Text = dlgOpen.FileName;
                txbDstFileName.Text = GMFileInfo.GetFileShortName(txbSrcFileName.Text, true);
            }
        }

        private void btnAddResourceFile_Click(object sender, EventArgs e)
        {
            ListViewItem srcItem = new ListViewItem(txbSrcFileName.Text);
            srcItem.SubItems.Add(txbDstFileName.Text);
            lvResourceFiles.Items.Add(srcItem);
        }

        private void tsmnuiRcDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem delItem in lvResourceFiles.SelectedItems)
            {
                lvResourceFiles.Items.Remove(delItem);
            }
        }

        private void btnImportDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlgFolder = new FolderBrowserDialog();
            if (dlgFolder.ShowDialog() == DialogResult.OK)
            {
                string strDirName = dlgFolder.SelectedPath;
                List<string> fileList = GMFileInfo.GetFilesUnderDir(strDirName, true);
                foreach (string strFileName in fileList)
                {
                    string strDstFileName = strFileName.Substring(strDirName.Length + 1);
                    ListViewItem srcItem = new ListViewItem(strFileName);
                    srcItem.SubItems.Add(strDstFileName);
                    lvResourceFiles.Items.Add(srcItem);
                }
            }
        }

        private delegate void UpdateResourceFilesHandler(string strJobName, List<ResourceFilePair> resourceFiles);

        private void UpdateResourceFiles(string strJobName, List<ResourceFilePair> resourceFiles)
        {
            GalaxyJob job = m_jobs.GetJob(strJobName);
            Debug.Assert(job != null);
            AppendOutputInfoHandler appendOutputInfo = new AppendOutputInfoHandler(this.AppendOutputInfo);

            // Get the job agent helper
            JobAgentHelper jobAgentHelper = null;
            string strPNInstanceName = job.PNInstanceName;
            string strOutputString;
            if (m_pnDict.ContainsKey(strPNInstanceName))
            {
                string strPNName = m_pnDict[strPNInstanceName].MachineName;
                int iPortNumber = m_pnDict[strPNInstanceName].Port;
                jobAgentHelper = GetJobAgentHelper(strPNInstanceName, strPNName, iPortNumber);
            }
            if (jobAgentHelper == null)
            {
                strOutputString = "Error occurs when connecting " + strPNInstanceName + " !";
                this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Error });
            }

            // Transport the resoure files
            int iLastSlashPos = -1;
            foreach (ResourceFilePair rcPair in resourceFiles)
            {
                string strSrcFileName = rcPair.SrcFileName;
                string strDstFileName = rcPair.DstRelativeFileName;
                string strDstShortFileName = "";
                string strDstRelativeFilePath = "";
                iLastSlashPos = strDstFileName.LastIndexOf('\\');
                if (iLastSlashPos == -1)
                {
                    strDstShortFileName = strDstFileName;
                    strDstRelativeFilePath = "";
                }
                else
                {
                    strDstShortFileName = strDstFileName.Substring(iLastSlashPos + 1);
                    strDstRelativeFilePath = strDstFileName.Substring(0, iLastSlashPos);
                }
                if (jobAgentHelper.TransportFile(job.ProjectDataRootDir, strSrcFileName, strDstShortFileName, strDstRelativeFilePath, 1024 * 1024) != 0)
                {
                    strOutputString = "Transport file " + strSrcFileName + " to " + strPNInstanceName + " error!";
                    this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Error});
                }
                else
                {
                    strOutputString = "Transport file " + strSrcFileName + " to " + strPNInstanceName + " successfully.";
                    this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Information });
                }
            }
        }

        private void btnUpdateJob_Click(object sender, EventArgs e)
        {
            List<string> jobNameList = new List<string>();
            for (int iItemId = 0; iItemId < clbAllJobs.Items.Count; iItemId++)
            {
                if (clbAllJobs.GetItemChecked(iItemId))
                {
                    jobNameList.Add(clbAllJobs.Items[iItemId].ToString());
                }
            }

            // Collect the resource files
            List<ResourceFilePair> resourceFilePairs = new List<ResourceFilePair>();
            foreach (ListViewItem rcItem in lvResourceFiles.Items)
            {
                string strSrcFileName = rcItem.Text;
                string strDstFileName = rcItem.SubItems[1].Text;
                ResourceFilePair rcPair = new ResourceFilePair(strSrcFileName, strDstFileName);
                resourceFilePairs.Add(rcPair);
            }

            foreach (string strJobName in jobNameList)
            {
                UpdateResourceFilesHandler updateResourceFiles = new UpdateResourceFilesHandler(this.UpdateResourceFiles);
                updateResourceFiles.BeginInvoke(strJobName, resourceFilePairs, null, null);
            }
        }

        private delegate void RunJobOnPNInstanceHandler(GalaxyJob job);

        private void RunJobOnPNInstance(GalaxyJob job)
        {
            // Initialize the print info call back
            string strOutputString;
            AppendOutputInfoHandler appendOutputInfo = new AppendOutputInfoHandler(this.AppendOutputInfo);

            // Get the job agent helper
            string strPNInstanceName = job.PNInstanceName;
            Debug.Assert(m_pnDict.ContainsKey(strPNInstanceName));
            PNInstance pnInstance = m_pnDict[strPNInstanceName];
            string strPNName = pnInstance.MachineName;
            int iPortNumber = pnInstance.Port;
            string strDataRootDir;
            JobAgentHelper jobAgentHelper = GetJobAgentHelper(strPNInstanceName, strPNName, iPortNumber);
            if (jobAgentHelper == null)
            {
                strOutputString = "Error occurs when connecting " + strPNName + ":" + iPortNumber.ToString() + "!";
                this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Error });
            }
            else
            {
                // Apply a new job id
                GalaxyJobBasicInfo jobBasicInfo = new GalaxyJobBasicInfo();
                jobBasicInfo.m_strProjectName = job.ProjectName;
                jobBasicInfo.m_strJobName = job.JobName;
                if (jobAgentHelper.ApplyForNewJob(jobBasicInfo, out strDataRootDir) != 0)
                {
                    strOutputString = "Apply job id error on " + strPNInstanceName + " !";
                    this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Error });
                    return;
                }

                // Construct a job start info
                GalaxyJobStartInfo jobStartInfo = new GalaxyJobStartInfo();
                jobStartInfo.AllowLongIdleTime = job.AllowLongIdleTime;
                jobStartInfo.Arguments = job.Arguments;
                jobStartInfo.AutoReportJobStatus = job.AutoReportStatus;
                jobStartInfo.ExecutableFileName = job.ExeFileName;
                jobStartInfo.RelativePath = job.RelativePath;
                jobStartInfo.UserName = job.UserName;
                jobStartInfo.DataRootDir = strDataRootDir;
                jobStartInfo.m_jobBasicInfo = jobBasicInfo;
                if (job.DependentJobs.Count > 0)
                {
                    for (int i = 1; i <= job.DependentJobs.Count; i++)
                    {
                        GalaxyJob dependentJob = m_jobs.GetJob(job.DependentJobs[i - 1]);
                        jobStartInfo.Arguments += (" -D" + i.ToString() + " " + dependentJob.OutputBaseDir);
                    }
                }

                // Update job info
                job.JobId = jobBasicInfo.JobId;
                job.Arguments = jobStartInfo.Arguments;
                job.ProjectDataRootDir = strDataRootDir + "\\" + job.ProjectName;

                // Transport resource files
                int iLastSlashPos = -1;
                foreach (ResourceFilePair rcPair in job.ResourceFileList)
                {
                    string strSrcFileName = rcPair.SrcFileName;
                    string strDstFileName = rcPair.DstRelativeFileName;
                    string strDstShortFileName = "";
                    string strDstRelativeFilePath = "";
                    iLastSlashPos = strDstFileName.LastIndexOf('\\');
                    if (iLastSlashPos == -1)
                    {
                        strDstShortFileName = strDstFileName;
                        strDstRelativeFilePath = "";
                    }
                    else
                    {
                        strDstShortFileName = strDstFileName.Substring(iLastSlashPos + 1);
                        strDstRelativeFilePath = strDstFileName.Substring(0, iLastSlashPos);
                    }
                    if (jobAgentHelper.TransportFile(job.ProjectDataRootDir, strSrcFileName, strDstShortFileName, strDstRelativeFilePath, 1024 * 1024) != 0)
                    {
                        strOutputString = "Transport file " + strSrcFileName + " to " + strPNInstanceName + " error!";
                        this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Error });
                        return;
                    }
                    else
                    {
                        strOutputString = "Transport file " + strSrcFileName + " to " + strPNInstanceName + " successfully.";
                        this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Information });
                    }
                }

                // Submit the job
                if (jobAgentHelper.AppendJobRequest(jobStartInfo) != 0)
                {
                    strOutputString = "Submit job to " + strPNInstanceName + " error!";
                    this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Error });
                    return;
                }

                // Update the job status
                job.JobStatus = GalaxyJobStatus.Queued;
                m_jobs.FlushToDisk(Constants.ms_strJobFileName);

                strOutputString = "Submit job to " + strPNInstanceName + " successfully.";
                this.BeginInvoke(appendOutputInfo, new object[] { strOutputString, OutputInfoCat.Information });
            }
        }

        private void btnSubmitJob_Click(object sender, EventArgs e)
        {
            string strJobShortName = txbJobName.Text.Trim();
            if (strJobShortName.Length == 0)
            {
                AppendOutputInfo("Job name is empty!", OutputInfoCat.Error);
                return;
            }

            List<string> pnInstanceList = new List<string>();
            // Collect the selected instances
            for (int iItemId = 0; iItemId < clbPNInstances.Items.Count; iItemId++)
            {
                if (clbPNInstances.GetItemChecked(iItemId))
                {
                    pnInstanceList.Add(clbPNInstances.Items[iItemId].ToString());
                }
            }
            // Run the job on each instance
            for (int iInstanceId = 0; iInstanceId < pnInstanceList.Count; iInstanceId++)
            {
                string strPNInstanceName = pnInstanceList[iInstanceId];
                string strJobName = strJobShortName + "@" + strPNInstanceName;

                if (m_jobs.IsJobExist(strJobName))
                {
                    AppendOutputInfo(strJobName + " is already existed!", OutputInfoCat.Error);
                    continue;
                }
   
                // Collect the resource files
                List<ResourceFilePair> resourceFilePairs = new List<ResourceFilePair>();
                foreach (ListViewItem rcItem in lvResourceFiles.Items)
                {
                    string strSrcFileName = rcItem.Text;
                    string strDstFileName = rcItem.SubItems[1].Text;
                    ResourceFilePair rcPair = new ResourceFilePair(strSrcFileName, strDstFileName);
                    resourceFilePairs.Add(rcPair);
                }

                // Collect the dependent jobs
                List<string> dependentJobs = new List<string>();
                foreach (string strDependentJobName in lbDepJobs.Items)
                {
                    dependentJobs.Add(strDependentJobName);
                }

                // Record the job
                // Decide whether the job can be run now
                //  1. Have no dependent jobs
                //  2. All the dependent jobs have been finished
                bool fJobIsReadyToRun = true;
                foreach (string strDependentJobName in dependentJobs)
                {
                    if (m_jobs.JobDictionaryRefByJobName.ContainsKey(strDependentJobName))
                    {
                        GalaxyJob dependentJob = m_jobs.JobDictionaryRefByJobName[strDependentJobName];
                        if (dependentJob.JobStatus != GalaxyJobStatus.Successful)
                        {
                            fJobIsReadyToRun = false;
                            break;
                        }
                    }
                    else
                    {
                        fJobIsReadyToRun = false;
                        break;
                    }
                }
                string strExecutableFileName = txbExecutableFileName.Text;
                string strExecutableShortFileName = "";
                string strExecutableRelativeFilePath = "";
                int iLastSlashPos = strExecutableFileName.LastIndexOf('\\');
                if (iLastSlashPos == -1)
                {
                    strExecutableShortFileName = strExecutableFileName;
                    strExecutableRelativeFilePath = "";
                }
                else
                {
                    strExecutableShortFileName = strExecutableFileName.Substring(iLastSlashPos + 1);
                    strExecutableRelativeFilePath = strExecutableFileName.Substring(0, iLastSlashPos);
                }
                GalaxyJob job = new GalaxyJob();
                job.AllowLongIdleTime = cbAllowLongIdleTime.Checked;
                job.Arguments = txbArguments.Text;
                job.AutoReportStatus = cbAutoReportStatus.Checked;
                job.DependentJobs = dependentJobs;
                job.ExeFileName = strExecutableShortFileName;
                job.JobId = Guid.Empty;
                job.JobName = strJobName;
                if (fJobIsReadyToRun)
                {
                    job.JobStatus = GalaxyJobStatus.Queued;
                }
                else
                {
                    job.JobStatus = GalaxyJobStatus.Waiting;
                }
                job.OutputBaseDir = "";
                job.PNInstanceName = strPNInstanceName;
                job.RelativePath = strExecutableRelativeFilePath;
                job.ResourceFileList = resourceFilePairs;
                job.UserName = Environment.UserDomainName + "\\" + Environment.UserName;
                AddJob(job);

                // Submit the job
                if (fJobIsReadyToRun)
                {
                    RunJobOnPNInstanceHandler runJobOnPN = new RunJobOnPNInstanceHandler(this.RunJobOnPNInstance);
                    runJobOnPN.BeginInvoke(job, null, null);
                }
                else
                {
                    AppendOutputInfo("Put the job:" + job.JobName.ToString() + " to the waiting list.", OutputInfoCat.Information);
                }
            }
        }

        private void btnSelectDepJob_Click(object sender, EventArgs e)
        {
            List<string> selJobIdList = new List<string>();
            foreach (string strJobName in lbAllJobs.SelectedItems)
            {
                selJobIdList.Add(strJobName);
            }

            foreach (string strJobName in selJobIdList)
            {
                lbAllJobs.Items.Remove(strJobName);
                lbDepJobs.Items.Add(strJobName);
            }
        }

        private void btnUnselectDepJob_Click(object sender, EventArgs e)
        {
            List<string> selJobIdList = new List<string>();
            foreach (string strJobName in lbDepJobs.SelectedItems)
            {
                selJobIdList.Add(strJobName);
            }

            foreach (string strJobName in selJobIdList)
            {
                lbDepJobs.Items.Remove(strJobName);
                lbAllJobs.Items.Add(strJobName);
            }
        }

        private JobAgentHelper CreateJobAgentHelper(string strPNName, int iPortNumber)
        {
            JobAgentHelper jobAgentHelper = new JobAgentHelper();
            if (!jobAgentHelper.Initialize(strPNName, iPortNumber))
            {
                return null;
            }

            // Add the event sinkers
            if (jobAgentHelper.AddJobStartListener(m_jobStartEventHandler) != 0)
            {
                return null;
            }
            if (jobAgentHelper.AddJobFinishListener(m_jobFinishEventHandler) != 0)
            {
                return null;
            }

            return jobAgentHelper;
        }

        /// <summary>
        /// Get the job agent helper
        /// </summary>
        /// <returns>
        ///     null - failed
        /// </returns>
        private JobAgentHelper GetJobAgentHelper(string strPNInstanceName, string strPNName, int iPortNumber)
        {
            JobAgentHelper jobAgentHelper = null;
            lock (ms_accessJobAgentHelperDictLock)
            {
                if ((m_jobAgentHelperDict != null) && (m_jobAgentHelperDict.ContainsKey(strPNInstanceName)))
                {
                    // We have already had an helper
                    jobAgentHelper = m_jobAgentHelperDict[strPNInstanceName];

                    // Test the healthy of existing helper
                    int iWaitingJobCount;
                    if (jobAgentHelper.GetWaitingJobCount(out iWaitingJobCount) != 0)
                    {
                        // Remove the dictionary entry
                        m_jobAgentHelperDict.Remove(strPNInstanceName);

                        // We need to re-initialize a new helper
                        jobAgentHelper = CreateJobAgentHelper(strPNName, iPortNumber);
                        if (jobAgentHelper == null)
                        {
                            return null;
                        }
                        // Update the dictionary
                        m_jobAgentHelperDict.Add(strPNInstanceName, jobAgentHelper);
                    }

                }
                else
                {
                    jobAgentHelper = CreateJobAgentHelper(strPNName, iPortNumber);
                    if (jobAgentHelper == null)
                    {
                        return null;
                    }

                    // Add the job agent helper to the dictionary
                    if (m_jobAgentHelperDict == null)
                    {
                        m_jobAgentHelperDict = new Dictionary<string, JobAgentHelper>();
                    }
                    m_jobAgentHelperDict.Add(strPNInstanceName, jobAgentHelper);
                }
            }
            return jobAgentHelper;
        }

        private void lbAllJobs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lbAllJobs.SelectedItem != null)
            {
                string strJobName = lbAllJobs.SelectedItem.ToString();
                Debug.Assert(m_jobs.JobDictionaryRefByJobName.ContainsKey(strJobName));
                GalaxyJob job = m_jobs.JobDictionaryRefByJobName[strJobName];
                FrmJobInfo frmJobInfo = new FrmJobInfo();
                frmJobInfo.Job = job;
                frmJobInfo.Show();
            }
        }

        private void lbDepJobs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lbDepJobs.SelectedItem != null)
            {
                string strJobName = lbDepJobs.SelectedItem.ToString();
                Debug.Assert(m_jobs.JobDictionaryRefByJobName.ContainsKey(strJobName));
                GalaxyJob job = m_jobs.JobDictionaryRefByJobName[strJobName];
                FrmJobInfo frmJobInfo = new FrmJobInfo();
                frmJobInfo.Job = job;
                frmJobInfo.Show();
            }
        }

        #endregion

        #region View job status tab page

        private void cbJobExeName_SelectedIndexChanged(object sender, EventArgs e)
        {
            QueryJobStatusHandler queryJobStatus = new QueryJobStatusHandler(this.QueryJobStatus);
            queryJobStatus.BeginInvoke(null, null);
        }

        private void btnSetInterval_Click(object sender, EventArgs e)
        {
            int iRefreshInterval = -1;
            try
            {
                iRefreshInterval = Int32.Parse(txbRefreshInterval.Text);
            }
            catch (Exception)
            {
            }
            if (iRefreshInterval > 0)
            {
                tmUpdateStatus.Interval = iRefreshInterval * 1000;
            }
        }

        private void tmUpdateStatus_Tick(object sender, EventArgs e)
        {
            QueryJobStatusHandler queryJobStatus = new QueryJobStatusHandler(this.QueryJobStatus);
            queryJobStatus.BeginInvoke(null, null);
        }

        /// <summary>
        /// Whether the waiting job is ready to be run now?
        /// </summary>
        private void CheckWaitingJob(GalaxyJob job)
        {
            // Decide whether the job can be run now
            //  1. Have no dependent jobs
            //  2. All the dependent jobs have been finished
            bool fJobIsReadyToRun = true;
            foreach (string strDependentJobName in job.DependentJobs)
            {
                if (m_jobs.JobDictionaryRefByJobName.ContainsKey(strDependentJobName))
                {
                    GalaxyJob dependentJob = m_jobs.JobDictionaryRefByJobName[strDependentJobName];
                    if (dependentJob.JobStatus != GalaxyJobStatus.Successful)
                    {
                        fJobIsReadyToRun = false;
                        break;
                    }
                }
                else
                {
                    fJobIsReadyToRun = false;
                    break;
                }
            }

            // Submit the job
            if (fJobIsReadyToRun)
            {
                RunJobOnPNInstanceHandler runJobOnPN = new RunJobOnPNInstanceHandler(this.RunJobOnPNInstance);
                runJobOnPN.BeginInvoke(job, null, null);
            }
        }

        private delegate void ShowJobStatusHandler();

        private void ShowJobStatus()
        {
            lock (ms_refreshJobListLock)
            {
                string strSelectedJobExeName = "";
                if ((cbJobExeName.SelectedIndex != -1) && (cbJobExeName.SelectedIndex != 0))
                {
                    strSelectedJobExeName = cbJobExeName.Text;
                }
                else if (cbJobExeName.SelectedIndex == 0)
                {
                    strSelectedJobExeName = null;
                }

                // Clear the list
                lvJobs.Items.Clear();
                for (int iColIndex = (int)JobItemIndex.JobPropertyStart; iColIndex < lvJobs.Columns.Count; iColIndex++)
                {
                    lvJobs.Columns.RemoveAt(iColIndex);
                }

                // Add the items which satisfy the condition
                foreach (GalaxyJob job in m_jobs.JobDictionaryRefByJobName.Values)
                {
                    if ((strSelectedJobExeName == null) || (strSelectedJobExeName.CompareTo(job.ExeFileName) == 0))
                    {
                        ListViewItem item = new ListViewItem(job.JobName.ToString());
                        item.Tag = job.JobName;
                        string strFullExeFileName = job.ExeFileName;
                        if (job.RelativePath.Length > 0)
                        {
                            strFullExeFileName = job.RelativePath + "\\" + job.ExeFileName;
                        }
                        item.SubItems.Add(strFullExeFileName);
                        item.SubItems.Add(job.Arguments);
                        item.SubItems.Add(job.PNInstanceName);
                        item.SubItems.Add(GalaxyJob.JobStatus2String(job.JobStatus));
                        item.SubItems.Add(job.OutputBaseDir);
                        GalaxyJobProperties jobProps = job.JobProperties;
                        if ((job.JobStatus == GalaxyJobStatus.Successful) || (job.JobStatus == GalaxyJobStatus.Failed))
                        {
                            // Don't update the properties for finished jobs
                        }
                        else if (jobProps != null)
                        {
                            foreach (string strPropName in jobProps.Properties.Keys)
                            {
                                // Find the column of the property name
                                int iColIndex = -1;
                                for (int iCol = (int)JobItemIndex.JobPropertyStart; iCol < lvJobs.Columns.Count; iCol++)
                                {
                                    if (lvJobs.Columns[iCol].Text.CompareTo(strPropName) == 0)
                                    {
                                        iColIndex = iCol;
                                        break;
                                    }
                                }
                                // If the column doesn't exist, add a new column
                                if (iColIndex == -1)
                                {
                                    lvJobs.Columns.Add(strPropName);
                                    iColIndex = lvJobs.Columns.Count - 1;
                                }
                                for (int iFillColIndex = item.SubItems.Count; iFillColIndex <= iColIndex; iFillColIndex++)
                                {
                                    item.SubItems.Add("");
                                }
                                string strPropValue = jobProps.Properties[strPropName];
                                item.SubItems[iColIndex].Text = strPropValue;
                            }
                        }
                        lvJobs.Items.Add(item);
                    }
                }
            }
        }

        private delegate void QueryJobStatusHandler();

        private void QueryJobStatus()
        {
            foreach (GalaxyJob job in m_jobs.JobDictionaryRefByJobName.Values)
            {
                // If the job is finished, reserve its status
                GalaxyJobStatusInfo jobStatusInfo = new GalaxyJobStatusInfo(GalaxyJobStatus.Unknown, "");
                GalaxyJobProperties jobProps = new GalaxyJobProperties();
                if ((job.JobStatus == GalaxyJobStatus.Successful)
                    || (job.JobStatus == GalaxyJobStatus.Failed))
                {
                    // Do nothing
                }
                else if (job.JobStatus == GalaxyJobStatus.Waiting)
                {
                    CheckWaitingJob(job);
                }
                else
                {
                    if (m_pnDict.ContainsKey(job.PNInstanceName))
                    {
                        PNInstance pnInstance = m_pnDict[job.PNInstanceName];
                        JobAgentHelper jobAgentHelper = GetJobAgentHelper(job.PNInstanceName, pnInstance.MachineName, pnInstance.Port);
                        if (jobAgentHelper != null)
                        {
                            jobAgentHelper.GetJobStatus(job.JobId, out jobStatusInfo);
                            jobAgentHelper.GetJobProperties(job.JobId, out jobProps);
                        }
                    }
                    job.JobStatus = jobStatusInfo.JobStatus;
                    job.OutputBaseDir = jobStatusInfo.OutputBaseDir;
                    job.JobProperties = jobProps;
                }
            }
            m_jobs.FlushToDisk(Constants.ms_strJobFileName);

            // Show the job status
            ShowJobStatusHandler showJobStatus = new ShowJobStatusHandler(this.ShowJobStatus);
            this.BeginInvoke(showJobStatus);

            // Append output info
            AppendOutputInfoHandler appendOutput = new AppendOutputInfoHandler(this.AppendOutputInfo);
            string strUpdateJobStatusInfo = "Finished update job status.";
            this.BeginInvoke(appendOutput, new object[] { strUpdateJobStatusInfo, OutputInfoCat.Information });
        }
 
        private bool AddJob(GalaxyJob job)
        {
            lbAllJobs.Items.Add(job.JobName);
            if (!cbJobExeName.Items.Contains(job.ExeFileName))
            {
                cbJobExeName.Items.Add(job.ExeFileName);
            }
            clbAllJobs.Items.Add(job.JobName);
            //
            m_jobs.AddJob(job);
            m_jobs.FlushToDisk(Constants.ms_strJobFileName);

            return true;
        }

        public void JobStarted(GalaxyJobStartInfo jobStartInfo)
        {
            GalaxyJob job = m_jobs.GetJob(jobStartInfo.JobId);
            if (job != null)
            {
                job.JobStatus = GalaxyJobStatus.Running;
            }

            m_jobs.FlushToDisk(Constants.ms_strJobFileName);

            QueryJobStatusHandler queryJobStatus = new QueryJobStatusHandler(this.QueryJobStatus);
            queryJobStatus.BeginInvoke(null, null);
        }

        public void JobFinished(GalaxyJobFinishInfo jobFinishInfo)
        {
            GalaxyJobStatus jobStatus = GalaxyJobStatus.Unknown;
            if (jobFinishInfo.JobRunResult == GalaxyJobRunResult.Failed)
            {
                jobStatus = GalaxyJobStatus.Failed;
            }
            else if (jobFinishInfo.JobRunResult == GalaxyJobRunResult.Successful)
            {
                jobStatus = GalaxyJobStatus.Successful;
            }
            GalaxyJob job = m_jobs.GetJob(jobFinishInfo.JobId);
            if (job != null)
            {
                job.JobStatus = jobStatus;
                job.OutputBaseDir = jobFinishInfo.JobOutputBaseDir;

                // Check if we can run the jobs which are dependent on this job
                foreach (GalaxyJob dependentOnJob in m_jobs.JobDictionaryRefByJobName.Values)
                {
                    if (dependentOnJob.DependentJobs.Contains(job.JobName))
                    {
                        CheckWaitingJob(dependentOnJob);
                    }
                }
            }

            m_jobs.FlushToDisk(Constants.ms_strJobFileName);

            QueryJobStatusHandler queryJobStatus = new QueryJobStatusHandler(this.QueryJobStatus);
            queryJobStatus.BeginInvoke(null, null);
        }

        private void tsmnuiDelete_Click(object sender, EventArgs e)
        {
            List<string> delJobIdList = new List<string>();
            foreach (ListViewItem delItem in lvJobs.SelectedItems)
            {
                string strJobName = delItem.Tag.ToString();
                delJobIdList.Add(strJobName);
                Debug.Assert(m_jobs.JobDictionaryRefByJobName.ContainsKey(strJobName));
                m_jobs.JobDictionaryRefByJobName.Remove(strJobName);
                lvJobs.Items.Remove(delItem);
                m_jobs.FlushToDisk(Constants.ms_strJobFileName);
            }

            foreach (string strJobName in delJobIdList)
            {
                // The deleted job can exist in either of the list
                lbAllJobs.Items.Remove(strJobName);
                lbDepJobs.Items.Remove(strJobName);
                clbAllJobs.Items.Remove(strJobName);
            }
        }

        private void tsmnuiViewOutputDir_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvJobs.SelectedItems)
            {
                string strOutputBaseDir = item.SubItems[(int)JobItemIndex.OutputBaseDir].Text;
                if (strOutputBaseDir.Trim().Length > 0)
                {
                    Process.Start("iexplore.exe", strOutputBaseDir);
                }
            }
        }

        private void tsmnuiStopJob_Click(object sender, EventArgs e)
        {
            StopJobsHandler stopJobs = new StopJobsHandler(this.StopJobs);
            stopJobs.BeginInvoke(null, null);
        }

        private delegate void StopJobsHandler();

        private void StopJobs()
        {
            AppendOutputInfoHandler appendOutputInfo = new AppendOutputInfoHandler(this.AppendOutputInfo);
            foreach (ListViewItem stopItem in lvJobs.SelectedItems)
            {
                string strJobName = stopItem.Tag.ToString();
                GalaxyJob job = m_jobs.GetJob(strJobName);
                Debug.Assert(job != null);
                Guid jobId = job.JobId;
                string strPNInstanceName = stopItem.SubItems[(int)JobItemIndex.PNName].Text;
                string strJobStatus = stopItem.SubItems[(int)JobItemIndex.Status].Text;
                if ((GalaxyJob.String2JobStatus(strJobStatus) == GalaxyJobStatus.Running) && (m_pnDict.ContainsKey(strPNInstanceName)))
                {
                    PNInstance pnInstance = m_pnDict[strPNInstanceName];
                    JobAgentHelper jobAgentHelper = GetJobAgentHelper(strPNInstanceName, pnInstance.MachineName, pnInstance.Port);
                    if ((jobAgentHelper != null) && (jobAgentHelper.StopJob(jobId) == 0))
                    {
                        string strExeName = stopItem.SubItems[(int)JobItemIndex.ExeName].Text;
                        string strOutputInfo = strExeName + " on " + strPNInstanceName + " is stopped!";
                        this.BeginInvoke(appendOutputInfo, new object[] { strOutputInfo, OutputInfoCat.Information });
                    }
                }
            }
        }

        private void tsmnuiRestartJob_Click(object sender, EventArgs e)
        {
            RestartJobsHandler restartJobs = new RestartJobsHandler(this.RestartJobs);
            restartJobs.BeginInvoke(null, null);
        }

        private delegate void RestartJobsHandler();

        private void RestartJobs()
        {
            AppendOutputInfoHandler appendOutputInfo = new AppendOutputInfoHandler(this.AppendOutputInfo);
            foreach (ListViewItem restartItem in lvJobs.SelectedItems)
            {
                string strJobName = restartItem.Tag.ToString();
                GalaxyJob job = m_jobs.GetJob(strJobName);
                Debug.Assert(job != null);
                Guid jobId = job.JobId;
                string strPNInstanceName = restartItem.SubItems[(int)JobItemIndex.PNName].Text;
                string strJobStatus = restartItem.SubItems[(int)JobItemIndex.Status].Text;
                if (((GalaxyJob.String2JobStatus(strJobStatus) == GalaxyJobStatus.Failed) || (GalaxyJob.String2JobStatus(strJobStatus) == GalaxyJobStatus.Successful))
                    && (m_pnDict.ContainsKey(strPNInstanceName)))
                {
                    PNInstance pnInstance = m_pnDict[strPNInstanceName];
                    JobAgentHelper jobAgentHelper = GetJobAgentHelper(strPNInstanceName, pnInstance.MachineName, pnInstance.Port);
                    if ((jobAgentHelper != null) && (jobAgentHelper.RestartJob(jobId) == 0))
                    {
                        string strExeName = restartItem.SubItems[(int)JobItemIndex.ExeName].Text;
                        string strOutputInfo = strExeName + " on " + strPNInstanceName + " is restarted";
                        this.BeginInvoke(appendOutputInfo, new object[] { strOutputInfo, OutputInfoCat.Information });
                    }
                }
            }
        }

        private void lvJobs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((lvJobs.SelectedItems != null) && (lvJobs.SelectedItems.Count > 0))
            {
                foreach (ListViewItem selItem in lvJobs.SelectedItems)
                {
                    string strSelJobName = selItem.SubItems[(int)JobItemIndex.JobName].Text;
                    Debug.Assert(m_jobs.JobDictionaryRefByJobName.ContainsKey(strSelJobName));
                    GalaxyJob job = m_jobs.JobDictionaryRefByJobName[strSelJobName];
                    FrmJobInfo frmJobInfo = new FrmJobInfo();
                    frmJobInfo.Job = job;
                    frmJobInfo.Show();
                }
            }
        }

        private void lvJobs_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column < (int)JobItemIndex.JobPropertyStart)
            {
                if (e.Column == m_jobInfoListItemSorter.SortColumn)
                {
                    if (m_jobInfoListItemSorter.Order == SortOrder.Ascending)
                    {
                        m_jobInfoListItemSorter.Order = SortOrder.Descending;
                    }
                    else
                    {
                        m_jobInfoListItemSorter.Order = SortOrder.Ascending;
                    }
                }
                else
                {
                    m_jobInfoListItemSorter.SortColumn = e.Column;
                    m_jobInfoListItemSorter.Order = SortOrder.Ascending;
                }

                lvJobs.Sort();
            }
        }
        #endregion

        #region View PN Status tab page

        private delegate void QueryPNStatusesHandler();
        private delegate void UpdatePNStatusHandler(string strPNName, ProcessNodePerfInfo pnInfo);

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            QueryPNStatusesHandler queryPNStatuses = new QueryPNStatusesHandler(this.QueryPNStatuses);
            queryPNStatuses.BeginInvoke(null, null);
        }

        private void FillPNView()
        {
            foreach (string strPNInstanceName in m_pnDict.Keys)
            {
                ListViewItem newItem = new ListViewItem(strPNInstanceName);
                newItem.Tag = strPNInstanceName;
                for (int i = (int)PNItemIndex.WaitingJobCount; i < (int)PNItemIndex.PNItemCount; i++)
                {
                    newItem.SubItems.Add("---");
                }
                lvPNStatuses.Items.Add(newItem);
            }
        }

        private void QueryPNStatuses()
        {
            foreach (string strPNInstanceName in m_pnDict.Keys)
            {
                PNInstance pnInstance = m_pnDict[strPNInstanceName];

                // Get information for the PN
                ProcessNodePerfInfo perfInfo = null;
                JobAgentHelper jobAgentHelper = GetJobAgentHelper(strPNInstanceName, pnInstance.MachineName, pnInstance.Port);
                if (jobAgentHelper != null)
                {
                    if (jobAgentHelper.GetProcessNodePerfInfo(out perfInfo) != 0)
                    {
                        perfInfo = null;
                    }
                }

                // Update PN status
                UpdatePNStatusHandler updatePNStatus = new UpdatePNStatusHandler(this.UpdatePNStatus);
                this.BeginInvoke(updatePNStatus, new object[] {strPNInstanceName, perfInfo});
            }
        }

        private void UpdatePNStatus(string strPNName, ProcessNodePerfInfo pnInfo)
        {
            foreach (ListViewItem item in lvPNStatuses.Items)
            {
                if (item.Tag.ToString().CompareTo(strPNName) == 0)
                {
                    if (pnInfo != null)
                    {
                        item.SubItems[(int)PNItemIndex.WaitingJobCount].Text = pnInfo.WaitingJobCount.ToString();
                        item.SubItems[(int)PNItemIndex.RunningJobCount].Text = pnInfo.RunningJobCount.ToString();
                        item.SubItems[(int)PNItemIndex.CPUUsage].Text = pnInfo.CPUUsage.ToString();
                        item.SubItems[(int)PNItemIndex.FreeDiskSpace].Text = pnInfo.DiskFreeSpace.ToString();
                        item.SubItems[(int)PNItemIndex.AvailablePhysicalMemory].Text = pnInfo.AvailablePhysicalMemory.ToString();
                        item.SubItems[(int)PNItemIndex.AvailableVirtualMemory].Text = pnInfo.AvailableVirtualMemory.ToString();
                        item.SubItems[(int)PNItemIndex.QueryTime].Text = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
                    }
                    else
                    {
                        for (int i = (int)PNItemIndex.WaitingJobCount; i < (int)PNItemIndex.PNItemCount; i++)
                        {
                            if (i == (int)PNItemIndex.QueryTime)
                            {
                                item.SubItems[i].Text = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
                            }
                            else
                            {
                                item.SubItems[i].Text = "Error";
                            }
                        }
                    }
                }
            }
        }

        private void lvPNStatuses_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == m_pnInfoListItemSorter.SortColumn)
            {
                if (m_pnInfoListItemSorter.Order == SortOrder.Ascending)
                {
                    m_pnInfoListItemSorter.Order = SortOrder.Descending;
                }
                else
                {
                    m_pnInfoListItemSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                m_pnInfoListItemSorter.SortColumn = e.Column;
                m_pnInfoListItemSorter.Order = SortOrder.Ascending;
            }

            lvPNStatuses.Sort();
        }

        #endregion

        private void tsmnuiOutputClear_Click(object sender, EventArgs e)
        {
            txbInfo.Clear();
        }
    }
}