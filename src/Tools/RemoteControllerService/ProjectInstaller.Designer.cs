namespace RemoteControllerService
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
            this.rcServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.rcServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // rcServiceProcessInstaller
            // 
            this.rcServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.rcServiceProcessInstaller.Password = null;
            this.rcServiceProcessInstaller.Username = null;
            // 
            // rcServiceInstaller
            // 
            this.rcServiceInstaller.Description = "The remote controller service of Galaxy";
            this.rcServiceInstaller.DisplayName = "Galaxy Remote Controller Service";
            this.rcServiceInstaller.ServiceName = "GalaxyRCService";
            this.rcServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.rcServiceProcessInstaller,
            this.rcServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller rcServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller rcServiceInstaller;
    }
}