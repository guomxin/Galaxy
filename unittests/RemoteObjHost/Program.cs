using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace TestRemoting
{
    class Program
    {
        static void Main(string[] args)
        {
            // Register the channel
            try
            {
                BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
                serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();
                IDictionary props = new Hashtable();
                props["port"] = 9999;
                TcpChannel tcpChannel = new TcpChannel(props, clientProv, serverProv);
                ChannelServices.RegisterChannel(tcpChannel, true);
            }
            catch (RemotingException)
            {
                Console.WriteLine("The channel tcp:9999 is already existed!");
                return;
            }
            catch (System.Net.Sockets.SocketException)
            {
                Console.WriteLine("The port number:9999 is busy!");
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
                Type type = Type.GetType("TestRemoting.RemoteObj, SharedLibrary");
                RemotingConfiguration.RegisterWellKnownServiceType(
                    Type.GetType("TestRemoting.RemoteObj, SharedLibrary"),
                    "RemoteEventObj", WellKnownObjectMode.Singleton);
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurs when registering the remote object!");
                return;
            }

            Console.WriteLine("Listening port: 9999");
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }

    
}
