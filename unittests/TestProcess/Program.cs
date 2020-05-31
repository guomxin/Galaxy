using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;
using System.Threading;

namespace TestProcess
{
    class Program
    {
        static double ms_dblProcessTime = 0;
        static void Main(string[] args)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = @"D:\Users\guomxin\Codes\Research\MSRAMM3\Researchers\guomxin\Galaxy\unittests\debug\ThrowException2.exe";
            myProcess.StartInfo.WorkingDirectory = @"D:\Users\guomxin\Codes\Research\MSRAMM3\Researchers\guomxin\Galaxy\unittests\debug\";
            myProcess.StartInfo.UseShellExecute = false ;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UserName = "Test";
            System.Security.SecureString sstrPassword = new System.Security.SecureString();
            string strPassword = "abcd1234!";
            for (int i = 0; i < strPassword.Length; i++)
            {
                sstrPassword.AppendChar(strPassword[i]);
            }
            myProcess.StartInfo.Password = sstrPassword;
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            TimerCallback checkProcessStatus = new TimerCallback(CheckStatus);
            Timer timer = new Timer(checkProcessStatus, myProcess, 5000, 5000);
            myProcess.Start();

            myProcess.WaitForExit();

            int iExitCode = myProcess.ExitCode;
            Console.WriteLine("Processor Time:" + myProcess.TotalProcessorTime.TotalMilliseconds.ToString());
            Console.WriteLine("Exit code: " + iExitCode.ToString());
        }

        static void CheckStatus(Object stateInfo)
        {
            Console.WriteLine(DateTime.Now.ToLongTimeString());
            Process process = stateInfo as Process;
            if (!process.HasExited)
            {
                double dblProcessTime = process.TotalProcessorTime.TotalMilliseconds;
                if (dblProcessTime == ms_dblProcessTime)
                {
                    Console.WriteLine("Kill the process");
                    process.Kill();
                }
                else
                {
                    ms_dblProcessTime = dblProcessTime;
                }
            }
            else
            {
                Console.WriteLine("Process has exited!");
            }
        }
    }
}
