using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using Galaxy.RemoteInterfaces;
using System.Collections;

namespace Galaxy.ProcessNode
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO Guomao: Read the port from the configuration file
            if (args.Length != 8)
            {
                Console.WriteLine("ProcessNode.exe -port xxx -datadir xxx -datadirsharename xxx -jobcount xxx");
                return;
            }
            if (args[0].ToLower().CompareTo("-port") != 0)
            {
                Console.WriteLine("ProcessNode.exe -port xxx -datadir xxx -datadirsharename xxx -jobcount xxx");
                return;
            }
            int iPortNumber = Int32.Parse(args[1]);
            if (args[2].ToLower().CompareTo("-datadir") != 0)
            {
                Console.WriteLine("ProcessNode.exe -port xxx -datadir xxx -datadirsharename xxx -jobcount xxx");
                return;
            }
            string strDataRootDir = args[3];
            if (args[4].ToLower().CompareTo("-datadirsharename") != 0)
            {
                Console.WriteLine("ProcessNode.exe -port xxx -datadir xxx -datadirsharename xxx -jobcount xxx");
                return;
            }
            string strDataRootDirShareName = args[5];
            if (args[6].ToLower().CompareTo("-jobcount") != 0)
            {
                Console.WriteLine("ProcessNode.exe -port xxx -datadir xxx -datadirsharename xxx -jobcount xxx");
                return;
            }
            int iMaxConcurrentJobCount = Int32.Parse(args[7]);

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
                Console.WriteLine("The channel tcp:" + iPortNumber.ToString() + " is already existed!");
                return;
            }
            catch (System.Net.Sockets.SocketException)
            {
                Console.WriteLine("The port number:" + iPortNumber + " is busy!");
                return;
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurs when registering the channel!");
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
                Console.WriteLine("Error occurs when registering the remote object!");
                return;
            }

            // Run the job agent
            List<JobAgentDataDir> dataDirs = new List<JobAgentDataDir>();
            JobAgentDataDir dataDir = new JobAgentDataDir();
            dataDir.DataDirName = strDataRootDir;
            dataDir.DataDirShareName = strDataRootDirShareName;
            dataDirs.Add(dataDir);
            IJobAgent jobAgent = Activator.GetObject(
                    typeof(IJobAgent),
                    "tcp://localhost:" + iPortNumber.ToString() + "/GalaxyJobAgent") as IJobAgent;
            jobAgent.MaximumJobIdleTime = 2;
            if (jobAgent.Run(iPortNumber, dataDirs, iMaxConcurrentJobCount) != 0)
            {
                Console.WriteLine("Process node instance run error!");
            }
        }
    }
}
