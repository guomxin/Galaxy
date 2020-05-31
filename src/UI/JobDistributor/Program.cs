using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.IO;

namespace JobDistributor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            JobDistributorConfig config = new JobDistributorConfig();
            if (!config.ParseFromConfigFile(Constants.ms_strConfigFileName))
            {
                MessageBox.Show("Parse config file:" + Constants.ms_strConfigFileName + " error!");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}