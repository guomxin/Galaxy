using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using JobDistributor;

namespace JobDistributor.UI
{
    public partial class FrmJobInfo : Form
    {
        #region User defined variables
        private GalaxyJob m_job;
        #endregion

        #region Properties
        public GalaxyJob Job
        {
            get { return m_job; }
            set { m_job = value; }
        }
        #endregion

        public FrmJobInfo()
        {
            InitializeComponent();
        }

        private void FrmJobInfo_Load(object sender, EventArgs e)
        {
            lvJobInfo.Items.Clear();
            // Add job information
            ListViewItem jobIdItem = new ListViewItem("Job Name");
            jobIdItem.SubItems.Add(m_job.JobName.ToString());
            lvJobInfo.Items.Add(jobIdItem);

            ListViewItem exeNameItem = new ListViewItem("Executable File Name");
            exeNameItem.SubItems.Add(m_job.ExeFileName);
            lvJobInfo.Items.Add(exeNameItem);

            ListViewItem relativePathItem = new ListViewItem("Relative Path");
            relativePathItem.SubItems.Add(m_job.RelativePath);
            lvJobInfo.Items.Add(relativePathItem);

            ListViewItem argumentsItem = new ListViewItem("Arguments");
            argumentsItem.SubItems.Add(m_job.Arguments);
            lvJobInfo.Items.Add(argumentsItem);

            ListViewItem autoReportStatusItem = new ListViewItem("AutoReportStatus");
            autoReportStatusItem.SubItems.Add(m_job.AutoReportStatus.ToString());
            lvJobInfo.Items.Add(autoReportStatusItem);

            ListViewItem allowLongIdleTimeItem = new ListViewItem("AllowLongIdleTime");
            allowLongIdleTimeItem.SubItems.Add(m_job.AllowLongIdleTime.ToString());
            lvJobInfo.Items.Add(allowLongIdleTimeItem);

            ListViewItem outputBaseDirItem = new ListViewItem("Output Directory");
            outputBaseDirItem.SubItems.Add(m_job.OutputBaseDir);
            lvJobInfo.Items.Add(outputBaseDirItem);

            ListViewItem pnInstanceNameItem = new ListViewItem("PN Instance Name");
            pnInstanceNameItem.SubItems.Add(m_job.PNInstanceName);
            lvJobInfo.Items.Add(pnInstanceNameItem);

            ListViewItem jobStatusItem = new ListViewItem("Job Status");
            jobStatusItem.SubItems.Add(GalaxyJob.JobStatus2String(m_job.JobStatus));
            lvJobInfo.Items.Add(jobStatusItem);

            ListViewItem dependentJobsItem = new ListViewItem("Dependent Jobs");
            string strDependentJobs = "";
            foreach (string strDependentJobName in m_job.DependentJobs)
            {
                strDependentJobs += (strDependentJobName.ToString() + ";");
            }
            dependentJobsItem.SubItems.Add(strDependentJobs);
            lvJobInfo.Items.Add(dependentJobsItem);
        }
    }
}