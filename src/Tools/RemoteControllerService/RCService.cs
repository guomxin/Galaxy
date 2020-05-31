using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonLib.IO.GMLogFile;
using Galaxy.Tools;
using System.Collections;

namespace RemoteControllerService
{
    public partial class RCService : ServiceBase
    {
        public RCService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Get the configurations
            int iPortNumber = Int32.Parse(ConfigurationManager.AppSettings["Port"]);
            string strLogFileName = ConfigurationManager.AppSettings["LogFile"];
            
            // Initialize the log file
            if (!GMThreadSafeLogFile.Open(strLogFileName))
            {
                throw new Exception("Initialize log file error!");
            }

            // Register a channel
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
                GMThreadSafeLogFile.LogError("The channel tcp:" + iPortNumber.ToString() + " is already existed!");
                return;
            }
            catch (Exception)
            {
                GMThreadSafeLogFile.LogError("Register channel tcp:" + iPortNumber.ToString() + " error!");
                return;
            }

            // Register the remote object
            try
            {
                RemotingConfiguration.ApplicationName = "GalaxyRemoteController";
                RemotingConfiguration.RegisterActivatedServiceType(Type.GetType("Galaxy.Tools.RemoteController, RemoteImpLib"));
            }
            catch (Exception)
            {
                GMThreadSafeLogFile.LogError("Register remote object error!");
                return;
            }

            GMThreadSafeLogFile.Log("The remote controller service at port:" + iPortNumber.ToString() + " is started successfully.");
        }

        protected override void OnStop()
        {
            // Do nothing now
        }
    }
}
