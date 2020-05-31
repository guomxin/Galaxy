using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.IO;

namespace TestService
{
    public partial class TestService : ServiceBase
    {
        public TestService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string strValueOfTestKey = System.Configuration.ConfigurationSettings.AppSettings["TestKey"];
            StreamWriter file = new StreamWriter("_Started.txt");
            file.WriteLine(strValueOfTestKey);
            file.Close();
        }

        protected override void OnStop()
        {
            StreamWriter file = new StreamWriter("_Stopped.txt");
            file.Close();
        }
    }
}
