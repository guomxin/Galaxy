using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

using System.Configuration;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using Galaxy.RemoteInterfaces;
using System.Collections;

namespace ProcessNodeService
{
    public partial class PNService : ServiceBase
    {
        public PNService()
        {
            InitializeComponent();
        }
         
        private delegate int JobAgentRunHandler(int iPortNumber, List<JobAgentDataDir> dataDirs, int iMaxConcurrentJobCount);

        protected override void OnStart(string[] args)
        {
            // TODO: Add log entries

            // Get the configurations
            List<JobAgentDataDir> dataDirs = new List<JobAgentDataDir>();
            int iPortNumber = Int32.Parse(ConfigurationManager.AppSettings["Port"]);
            int iMaxConcurrentJobCount = Int32.Parse(ConfigurationManager.AppSettings["MaxJobCount"]);
            int iMaximumJobIdleTime = Int32.Parse(ConfigurationManager.AppSettings["MaximumJobIdleTime"]);
            int iDataDirCount = Int32.Parse(ConfigurationManager.AppSettings["DataDirCount"]);
            for (int i = 1; i <= iDataDirCount; i++)
            {
                JobAgentDataDir dataDir = new JobAgentDataDir();
                dataDir.DataDirName = ConfigurationManager.AppSettings["DataDir" + i.ToString()];
                dataDir.DataDirShareName = ConfigurationManager.AppSettings["DataDirShareName" + i.ToString()];
                dataDirs.Add(dataDir);
            }

            // Register the channel
            try
            {
                BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
                serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();
                IDictionary props = new Hashtable();
                props["port"] = iPortNumber;
                TcpChannel tcpChannel = new TcpChannel(props, clientProv, serverProv);
                ChannelServices.RegisterChannel(tcpChannel, true);
            }
            catch (RemotingException)
            {
                // Console.WriteLine("The channel tcp:" + iPortNumber.ToString() + " is already existed!");
                return;
            }
            catch (System.Net.Sockets.SocketException)
            {
                // Console.WriteLine("The port number:" + iPortNumber + " is busy!");
                return;
            }
            catch (Exception)
            {
                // Console.WriteLine("Error occurs when registering the channel!");
                return;
            }

            // Register the remote object
            try
            {
                RemotingConfiguration.RegisterWellKnownServiceType(
                    Type.GetType("Galaxy.ProcessNode.JobStatusManager, RemoteImpLib"),
                    "GalaxyJobStatusManager", WellKnownObjectMode.Singleton);
                RemotingConfiguration.RegisterWellKnownServiceType(
                    Type.GetType("Galaxy.ProcessNode.JobAgent, RemoteImpLib"),
                    "GalaxyJobAgent", WellKnownObjectMode.Singleton);
            }
            catch (Exception)
            {
                // Console.WriteLine("Error occurs when registering the remote object!");
                return;
            }

            // Run the job agent
            IJobAgent jobAgent = Activator.GetObject(
                    typeof(IJobAgent),
                    "tcp://localhost:" + iPortNumber.ToString() + "/GalaxyJobAgent") as IJobAgent;
            jobAgent.MaximumJobIdleTime = iMaximumJobIdleTime;
            JobAgentRunHandler jobAgentRun = new JobAgentRunHandler(jobAgent.Run);
            jobAgentRun.BeginInvoke(iPortNumber, dataDirs, iMaxConcurrentJobCount, null, null);
        }
        
       
        protected override void OnStop()
        {
            // Do nothing now
        }
    }
}
