using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.ManagedClientLib;
using Galaxy.RemoteInterfaces;
using System.Diagnostics;

namespace ReportFailedJob
{
    class Program
    {
        static void Main(string[] args)
        {
            JobStatusHelper.Initialize(8001);

            JobStatusHelper.SetJobProperty("Prop3", "0");
            int i = 0;
            for (int j = 0; j < 100000; j++)
            {
                for (int k = 0; k < 100000; k++)
                {
                    i += j;
                    i -= j;
                }
            }
            JobStatusHelper.SetJobProperty("Prop3", "1");
            JobStatusHelper.SetJobProperty("Prop4", "1");

            for (int j = 0; j < 100000; j++)
            {
                for (int k = 0; k < 100000; k++)
                {
                    i += j;
                    i -= j;
                }
            }
            JobStatusHelper.SetJobProperty("Prop3", "2");

            for (int j = 0; j < 100000; j++)
            {
                for (int k = 0; k < 100000; k++)
                {
                    i += j;
                    i -= j;
                }
            }

            JobStatusHelper.SetJobProperty(GalaxyJobProperties.ms_strJobStatusPropName, GalaxyJobProperties.ms_strPropValueOfFailedJob);
        }
    }
}
