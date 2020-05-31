using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;

using System.Configuration;

namespace ProcessNodeService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}