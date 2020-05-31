using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;

namespace RemoteControllerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the configurations
            int iPortNumber = Int32.Parse(ConfigurationManager.AppSettings["Port"]);
            string strLogFileName = ConfigurationManager.AppSettings["LogFile"];

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
                Console.WriteLine("The channel tcp:" + iPortNumber.ToString() + " is already existed!");
                return;
            }
            catch (Exception)
            {
                Console.WriteLine("Register channel tcp:" + iPortNumber.ToString() + " error!");
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
                Console.WriteLine("Register remote object error!");
                return;
            }

            Console.WriteLine("The remote controller service at port:" + iPortNumber.ToString() + " is started successfully.");

            Console.ReadLine();
        }
    }
}
