namespace JobDistributor.UI
{
    partial class FrmJobInfo
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
            this.lvJobInfo = new System.Windows.Forms.ListView();
            this.jobPropertyName = new System.Windows.Forms.ColumnHeader();
            this.jobPropertyValue = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // lvJobInfo
            // 
            this.lvJobInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.jobPropertyName,
            this.jobPropertyValue});
            this.lvJobInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvJobInfo.FullRowSelect = true;
            this.lvJobInfo.Location = new System.Drawing.Point(0, 0);
            this.lvJobInfo.Name = "lvJobInfo";
            this.lvJobInfo.Size = new System.Drawing.Size(361, 381);
            this.lvJobInfo.TabIndex = 0;
            this.lvJobInfo.UseCompatibleStateImageBehavior = false;
            this.lvJobInfo.View = System.Windows.Forms.View.Details;
            // 
            // jobPropertyName
            // 
            this.jobPropertyName.Text = "PropertyName";
            this.jobPropertyName.Width = 150;
            // 
            // jobPropertyValue
            // 
            this.jobPropertyValue.Text = "PropertyValue";
            this.jobPropertyValue.Width = 200;
            // 
            // FrmJobInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 381);
            this.Controls.Add(this.lvJobInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmJobInfo";
            this.Text = "Job Information";
            this.Load += new System.EventHandler(this.FrmJobInfo_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvJobInfo;
        private System.Windows.Forms.ColumnHeader jobPropertyName;
        private System.Windows.Forms.ColumnHeader jobPropertyValue;
    }
}