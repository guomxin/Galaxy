using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.ManagedClientLib;
using Galaxy.RemoteInterfaces;
using System.Diagnostics;
using System.Threading;

namespace TestJobStatusManager
{
    class Program
    {
        static void Main(string[] args)
        {
        }
        /*
        static Dictionary<Guid, JobStatusEnum> ms_jobStatuses;
        static object ms_accessJobStatusesLock;

        static void Main(string[] args)
        {
            ms_accessJobStatusesLock = new object();
            ms_jobStatuses = new Dictionary<Guid, JobStatusEnum>();
            
            if (!JobStatusHelper.Initialize(9999))
            {
                Console.WriteLine("Initialize error!");
                return;
            }

            Debug.Assert(JobStatusHelper.ClearJobs());

            Thread thread1 = new Thread(new ThreadStart(ThreadEntry1));
            Thread thread2 = new Thread(new ThreadStart(ThreadEntry2));
            Thread thread3 = new Thread(new ThreadStart(ThreadEntry3));
            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread1.Join();
            thread2.Join();
            thread3.Join();

            int iJobCount;
            Debug.Assert(JobStatusHelper.GetJobCount(out iJobCount));
            Debug.Assert(iJobCount == ms_jobStatuses.Count);
        }

        #region Helper functions
        static void ReportJobStatus(Guid jobId, JobStatusEnum jobStatus)
        {
            lock (ms_accessJobStatusesLock)
            {
                if (ms_jobStatuses.ContainsKey(jobId))
                {
                    ms_jobStatuses[jobId] = jobStatus;
                }
                else
                {
                    ms_jobStatuses.Add(jobId, jobStatus);
                }
            }
        }

        static JobStatusEnum GetJobStatus(Guid jobId)
        {
            lock (ms_accessJobStatusesLock)
            {
                return ms_jobStatuses[jobId];
            }
        }

        static int GetJobCount()
        {
            int iJobCount = 0;
            lock (ms_accessJobStatusesLock)
            {
                iJobCount = ms_jobStatuses.Count;
            }
            return iJobCount;
        }

        static void RemoveJob(Guid jobId)
        {
            lock (ms_accessJobStatusesLock)
            {
                ms_jobStatuses.Remove(jobId);
            }
        }
        #endregion

        static void ThreadEntry1()
        {
            Console.WriteLine("Start thread1 ...");
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();
            JobStatusEnum jobStatus;
            //
            ReportJobStatus(guid1, JobStatusEnum.Ready);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid1, JobStatusEnum.Ready));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid1, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Ready);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid2, JobStatusEnum.Running);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid2, JobStatusEnum.Running));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid2, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Running);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid3, JobStatusEnum.Succeeded);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid3, JobStatusEnum.Succeeded));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid3, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Succeeded);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid1, JobStatusEnum.Failed);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid1, JobStatusEnum.Failed));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid1, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Failed);
            Thread.Sleep(1000);
            //
            RemoveJob(guid2);
            Debug.Assert(JobStatusHelper.RemoveJob(guid2));
            Debug.Assert(!JobStatusHelper.GetJobStatus(guid2, out jobStatus));
        }

        static void ThreadEntry2()
        {
            Console.WriteLine("Start thread2 ...");
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            JobStatusEnum jobStatus;
            //
            ReportJobStatus(guid1, JobStatusEnum.Ready);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid1, JobStatusEnum.Ready));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid1, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Ready);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid2, JobStatusEnum.Running);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid2, JobStatusEnum.Running));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid2, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Running);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid1, JobStatusEnum.Failed);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid1, JobStatusEnum.Failed));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid1, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Failed);
            Thread.Sleep(1000);
            //
            RemoveJob(guid2);
            Debug.Assert(JobStatusHelper.RemoveJob(guid2));
            Debug.Assert(!JobStatusHelper.GetJobStatus(guid2, out jobStatus));
        }

        static void ThreadEntry3()
        {
            Console.WriteLine("Start thread3 ...");
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();
            Guid guid4 = Guid.NewGuid();
            JobStatusEnum jobStatus;
            //
            ReportJobStatus(guid1, JobStatusEnum.Ready);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid1, JobStatusEnum.Ready));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid1, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Ready);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid2, JobStatusEnum.Running);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid2, JobStatusEnum.Running));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid2, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Running);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid3, JobStatusEnum.Succeeded);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid3, JobStatusEnum.Succeeded));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid3, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Succeeded);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid4, JobStatusEnum.Failed);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid4, JobStatusEnum.Failed));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid4, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Failed);
            Thread.Sleep(1000);
            //
            ReportJobStatus(guid1, JobStatusEnum.Running);
            Debug.Assert(JobStatusHelper.ReportJobStatus(guid1, JobStatusEnum.Running));
            Debug.Assert(JobStatusHelper.GetJobStatus(guid1, out jobStatus));
            Debug.Assert(jobStatus == JobStatusEnum.Running);
            Thread.Sleep(1000);
            //
            RemoveJob(guid4);
            Debug.Assert(JobStatusHelper.RemoveJob(guid4));
            Debug.Assert(!JobStatusHelper.GetJobStatus(guid4, out jobStatus));
        }
        */
    }
}
