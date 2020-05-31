using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.ManagedClientLib;
using Galaxy.RemoteInterfaces;
using System.Diagnostics;
using System.Threading;

namespace TestProcessNode
{
    class ProcessNodeInfo
    {
        public string ProcessNodeName;
        public int PortNumber;
        public JobAgentHelper JobAgentHelper;

        public ProcessNodeInfo(string strProcessNodeName, int iPortNumber, JobAgentHelper jobAgentHelper)
        {
            ProcessNodeName = strProcessNodeName;
            PortNumber = iPortNumber;
            JobAgentHelper = jobAgentHelper;
        }
    }

    class Program
    {
        static void Main(string[] args)
        { 
        }
        /*
        static void Main(string[] args)
        {
            JobAgentHelper jobAgentHelper = new JobAgentHelper();

            // Test running jobs
            ProcessNodeInfo processNodeInfo = new ProcessNodeInfo("wsm-ws-39", 8001, jobAgentHelper);
            string strProcessNodeName = processNodeInfo.ProcessNodeName;
            int iPortNumber = processNodeInfo.PortNumber;
            if (!jobAgentHelper.Initialize(strProcessNodeName, iPortNumber))
            {
                Console.WriteLine("Initialize job agent error!");
                return;
            }

            int iWaitingJobCount;
            int iRunningJobCount;
            jobAgentHelper.GetWaitingJobCount(out iWaitingJobCount);
            jobAgentHelper.GetRunningJobCount(out iRunningJobCount);
            Console.WriteLine("Waiting job count: " + iWaitingJobCount.ToString());
            Console.WriteLine("Running job count: " + iRunningJobCount.ToString());
            int iSuccessfulJobCount;
            int iFailedJobCount;
            jobAgentHelper.GetFinishedJobCount(out iSuccessfulJobCount, out iFailedJobCount);
            Console.WriteLine("Successful job count: " + iSuccessfulJobCount.ToString());
            Console.WriteLine("Failed job count: " + iFailedJobCount.ToString());

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            //ParameterizedThreadStart threadStart1 = new ParameterizedThreadStart(TestRunJob1);
            //Thread thread1 = new Thread(threadStart1);
            //thread1.Start(processNodeInfo);

            //ParameterizedThreadStart threadStart2 = new ParameterizedThreadStart(TestRunJob2);
            //Thread thread2 = new Thread(threadStart2);
            //thread2.Start(processNodeInfo);

            ParameterizedThreadStart threadStart3 = new ParameterizedThreadStart(TestRunJob3);
            Thread thread3 = new Thread(threadStart3);
            thread3.Start(processNodeInfo);

            ParameterizedThreadStart threadStart4 = new ParameterizedThreadStart(TestRunJob4);
            Thread thread4 = new Thread(threadStart4);
            thread4.Start(processNodeInfo);

            ParameterizedThreadStart threadStart5 = new ParameterizedThreadStart(TestRunJob5);
            Thread thread5 = new Thread(threadStart5);
            thread5.Start(processNodeInfo);

            //thread1.Join();
            //thread2.Join();
            thread3.Join();
            thread4.Join();
            thread5.Join();

            jobAgentHelper.GetWaitingJobCount(out iWaitingJobCount);
            jobAgentHelper.GetRunningJobCount(out iRunningJobCount);
            Console.WriteLine("Waiting job count: " + iWaitingJobCount.ToString());
            Console.WriteLine("Running job count: " + iRunningJobCount.ToString());
            jobAgentHelper.GetFinishedJobCount(out iSuccessfulJobCount, out iFailedJobCount);
            Console.WriteLine("Successful job count: " + iSuccessfulJobCount.ToString());
            Console.WriteLine("Failed job count: " + iFailedJobCount.ToString());

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        static void TestRunJob5(object objPara)
        {
            // 3 jobs, 1 successful, 2 failed
            JobAgentHelper jobAgentHelper = (objPara as ProcessNodeInfo).JobAgentHelper;

            // Construct jobs
            Guid jobId;

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\ReportSucJob.exe",
                "ReportSucJob.exe",
                "",
                1024 * 1024) == 0);
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\ManagedClientLib.dll",
                "ManagedClientLib.dll",
                "",
                1024 * 1024) == 0);
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\RemoteInterfaces.dll",
                "RemoteInterfaces.dll",
                "",
                1024 * 1024) == 0);
            // Submit the job
            GalaxyJobStartInfo jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.AutoReportJobStatus = true;
            jobStartInfo.ExecutableFileName = "ReportSucJob.exe";
            jobStartInfo.Arguments = "-JobId " + jobId.ToString();
            jobStartInfo.RelativePath = "";
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\ReportFailedJob.exe",
                "ReportFailedJob.exe",
                "",
                1024 * 1024) == 0);
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\ManagedClientLib.dll",
                "ManagedClientLib.dll",
                "",
                1024 * 1024) == 0);
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\RemoteInterfaces.dll",
                "RemoteInterfaces.dll",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "ReportFailedJob.exe";
            jobStartInfo.AutoReportJobStatus = true;
            jobStartInfo.Arguments = "-JobId " + jobId.ToString();
            jobStartInfo.RelativePath = "";
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\DoSomething3.exe",
                "DoSomething2.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "DoSomething3.exe";
            jobStartInfo.AutoReportJobStatus = true;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

        }

        static void TestRunJob3(object objPara)
        {
            // 4 jobs, 2 successful, 2 failed
            JobAgentHelper jobAgentHelper = (objPara as ProcessNodeInfo).JobAgentHelper;

            // Construct jobs
            Guid jobId;

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\ThrowException.exe",
                "ThrowException.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            GalaxyJobStartInfo jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "ThrowException.exe";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\DoSomething.exe",
                "DoSomething.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "DoSomething.exe";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\ThrowException.exe",
                "ThrowException.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "ThrowException.exe";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\DoSomething.exe",
                "DoSomething.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "DoSomething.exe";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);
        }

        static void TestRunJob4(object objPara)
        {
            // 4 jobs, 3 successful, 1 failed
            JobAgentHelper jobAgentHelper = (objPara as ProcessNodeInfo).JobAgentHelper;

            // Construct jobs
            Guid jobId;

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\ThrowException2.exe",
                "ThrowException2.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            GalaxyJobStartInfo jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "ThrowException2.exe";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\DoSomething2.exe",
                "DoSomething2.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "DoSomething2.exe";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\DoSomething2.exe",
                "DoSomething2.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "DoSomething2.exe";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\DoSomething2.exe",
                "DoSomething2.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "DoSomething2.exe";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);
        }

        static void TestRunJob1(object objPara)
        {
            JobAgentHelper jobAgentHelper = (objPara as ProcessNodeInfo).JobAgentHelper;

            // Construct jobs
            Guid jobId;

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\VerifyDomainGraphFile.exe",
                "VerifyDomainGraphFile.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            GalaxyJobStartInfo jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "VerifyDomainGraphFile.exe";
            jobStartInfo.Arguments = @"\\msra-mica-1\StaticRankBase\StaticRankBaseFileServer\MSRAData\DomainGraphFiles\2006-10-25\SRBDomainRank_20061025.web 0 0 NULL 0";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\VerifyDomainGraphFile.exe",
                "VerifyDomainGraphFile.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "VerifyDomainGraphFile.exe";
            jobStartInfo.Arguments = @"\\msra-mica-1\StaticRankBase\StaticRankBaseFileServer\MSRAData\DomainGraphFiles\2006-10-25\SRBDomainRank_20061025.web 1 32 NULL 0";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\VerifyDomainGraphFile.exe",
                "VerifyDomainGraphFile.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "VerifyDomainGraphFile.exe";
            jobStartInfo.Arguments = @"\\msra-mica-1\StaticRankBase\StaticRankBaseFileServer\MSRAData\DomainGraphFiles\2006-10-25\SRBDomainRank_20061025.mweb 0 0 NULL 0";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\VerifyDomainGraphFile.exe",
                "VerifyDomainGraphFile.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "VerifyDomainGraphFile.exe";
            jobStartInfo.Arguments = @"\\msra-mica-1\StaticRankBase\StaticRankBaseFileServer\MSRAData\DomainGraphFiles\2006-10-25\SRBDomainRank_20061025.mweb 1 64 NULL 0";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);
        }

        static void TestRunJob2(object objPara)
        {
            JobAgentHelper jobAgentHelper = (objPara as ProcessNodeInfo).JobAgentHelper;

            // Construct jobs
            Guid jobId;

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\VerifyDomainGraphChunks.exe",
                "VerifyDomainGraphChunks.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            GalaxyJobStartInfo jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "VerifyDomainGraphChunks.exe";
            jobStartInfo.Arguments = @"\\msra-mica-1\StaticRankBase\StaticRankBaseFileServer\MSRAData\DomainGraphFiles\2006-10-25\Chunks\SRBDomainRank_20061025.web 0 0 NULL 0";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\VerifyDomainGraphChunks.exe",
                "VerifyDomainGraphChunks.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "VerifyDomainGraphChunks.exe";
            jobStartInfo.Arguments = @"\\msra-mica-1\StaticRankBase\StaticRankBaseFileServer\MSRAData\DomainGraphFiles\2006-10-25\Chunks\SRBDomainRank_20061025.web 1 32 NULL 0";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\VerifyDomainGraphChunks.exe",
                "VerifyDomainGraphChunks.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "VerifyDomainGraphChunks.exe";
            jobStartInfo.Arguments = @"\\msra-mica-1\StaticRankBase\StaticRankBaseFileServer\MSRAData\DomainGraphFiles\2006-10-25\Chunks\SRBDomainRank_20061025.mweb 0 0 NULL 0";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);

            // Apply a new job id
            Debug.Assert(jobAgentHelper.ApplyForNewJob(out jobId) == 0);
            // Transport executable file
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\Job\VerifyDomainGraphChunks.exe",
                "VerifyDomainGraphChunks.exe",
                "",
                1024 * 1024) == 0);
            // Submit the job
            jobStartInfo = new GalaxyJobStartInfo();
            jobStartInfo.ExecutableFileName = "VerifyDomainGraphChunks.exe";
            jobStartInfo.Arguments = @"\\msra-mica-1\StaticRankBase\StaticRankBaseFileServer\MSRAData\DomainGraphFiles\2006-10-25\Chunks\SRBDomainRank_20061025.mweb 1 64 NULL 0";
            jobStartInfo.RelativePath = "";
            jobStartInfo.AutoReportJobStatus = false;
            jobStartInfo.JobId = jobId;
            Debug.Assert(jobAgentHelper.AppendJobRequest(jobStartInfo) == 0);
        }

        static void TestTransportData(string strProcessNodeName, int iPortNumber, JobAgentHelper jobAgentHelper)
        {
            // Apply a new job id
            Guid jobId;
            jobAgentHelper.ApplyForNewJob(out jobId);
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\TransportData\autopilot.ini",
                "autopilot.ini",
                "",
                1024) == 0);
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\TransportData\bin\cosmos2.exe",
                "cosmos2.exe",
                "bin",
                1024) == 0);
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\TransportData\bin\cosmos2.exe",
                "cosmos2.exe",
                "bin\\test",
                1024) == 0);
            Debug.Assert(jobAgentHelper.TransportFile(
                jobId,
                @"..\..\..\..\bin\TestData\TransportData\bin\cosmos.exe",
                "cosmos2.exe",
                "bin",
                1024) != 0);
        }*/
    }
    
}
