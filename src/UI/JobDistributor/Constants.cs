using System;
using System.Collections.Generic;
using System.Text;

namespace JobDistributor
{
    class Constants
    {
        public static string ms_strConfigFileName = "JobDistributor.config.xml";
        public static string ms_strJobFileName = "GalaxyJobs.xml";

        public static int ms_iCheckJobStatusInterval = 2 * 60 * 1000; // 2 mins
    }
}
