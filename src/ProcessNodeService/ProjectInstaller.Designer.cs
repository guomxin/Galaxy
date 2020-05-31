namespace ProcessNodeService
{
    partial class ProjectInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.pnServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // pnServiceProcessInstaller
            // 
            this.pnServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.pnServiceProcessInstaller.Password = null;
            this.pnServiceProcessInstaller.Username = null;
            // 
            // pnServiceInstaller
            // 
            this.pnServiceInstaller.Description = "The process node of Galaxy";
            this.pnServiceInstaller.DisplayName = "Galaxy Process Node";
            this.pnServiceInstaller.ServiceName = "GalaxyPNService";
            this.pnServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.pnServiceProcessInstaller,
            this.pnServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller pnServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller pnServiceInstaller;
    }
}