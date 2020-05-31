using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyDeploymentClient
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                System.Console.WriteLine("GalaxyDeploymentClient <config-file>");
                return;
            }

            GalaxyDeploymentClient gdc = new GalaxyDeploymentClient();
            bool bRet = gdc.Init(args[0]);
            if (!bRet)
            {
                System.Console.WriteLine("Fail to initialize the deployment client");
                return;
            }

            bRet = gdc.Process();
            if (!bRet)
            {
                System.Console.WriteLine("Deploy failed. Please refer to the logs for more details");
                return;
            }

            System.Console.WriteLine("Succeeded");
        }
    }
}
