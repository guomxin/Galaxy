namespace JobDistributor.UI
{
    partial class FrmAbout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkLblTeam = new System.Windows.Forms.LinkLabel();
            this.linkLblMe = new System.Windows.Forms.LinkLabel();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(56, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Job Distributor, ver 1.00.2006.1122";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.linkLblMe);
            this.groupBox1.Controls.Add(this.linkLblTeam);
            this.groupBox1.Location = new System.Drawing.Point(12, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 75);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Authors";
            // 
            // linkLblTeam
            // 
            this.linkLblTeam.AutoSize = true;
            this.linkLblTeam.Location = new System.Drawing.Point(20, 20);
            this.linkLblTeam.Name = "linkLblTeam";
            this.linkLblTeam.Size = new System.Drawing.Size(129, 13);
            this.linkLblTeam.TabIndex = 0;
            this.linkLblTeam.TabStop = true;
            this.linkLblTeam.Text = "WSM Infrastructure Team";
            this.linkLblTeam.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLblTeam_LinkClicked);
            // 
            // linkLblMe
            // 
            this.linkLblMe.AutoSize = true;
            this.linkLblMe.Location = new System.Drawing.Point(20, 42);
            this.linkLblMe.Name = "linkLblMe";
            this.linkLblMe.Size = new System.Drawing.Size(65, 13);
            this.linkLblMe.TabIndex = 1;
            this.linkLblMe.TabStop = true;
            this.linkLblMe.Text = "Guomao Xin";
            this.linkLblMe.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLblMe_LinkClicked);
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(105, 127);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // FrmAbout
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 165);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAbout";
            this.Text = "About Job Distributor";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel linkLblMe;
        private System.Windows.Forms.LinkLabel linkLblTeam;
        private System.Windows.Forms.Button btnOk;
    }
}