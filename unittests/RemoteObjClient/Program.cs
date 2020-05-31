using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections;

namespace TestRemoting
{
    class Program
    {
        delegate void DoSomethingDelegate(Guid id);

        static void Main(string[] args)
        {
            try
            {
                BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
                serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();
                IDictionary props = new Hashtable();
                props["port"] = 8001;
                TcpChannel tcpChannel = new TcpChannel(props, clientProv, serverProv);
                ChannelServices.RegisterChannel(tcpChannel, true);
            }
            catch (RemotingException)
            {
                Console.WriteLine("The channel tcp:8001 is already existed!");
                return;
            }
            catch (System.Net.Sockets.SocketException)
            {
                Console.WriteLine("The port number:8001 is busy!");
                return;
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurs when registering the channel!");
                return;
            }


            IRemote remoteObj = Activator.GetObject(
                    typeof(IRemote),
                    "tcp://localhost:9999/RemoteEventObj") as IRemote;
            TestClass testClass = new TestClass();
            EventSinker eventSinker = new EventSinker();
            //EventSinker eventSinker = Activator.GetObject(
            //        typeof(IRemote),
            //        "tcp://localhost:8001/RemoteEventSinker") as EventSinker;
            NonRemoteCallBack callBack = new NonRemoteCallBack(PrintMessage);
            eventSinker.m_callBack = callBack;
            Console.WriteLine("Event sinker hash code: " + eventSinker.GetHashCode().ToString());
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            remoteObj.OnRemoteEvent -= new RemoteEventHandler(eventSinker.EventFired);
            remoteObj.OnRemoteEvent += new RemoteEventHandler(eventSinker.EventFired);
            remoteObj.OnRemoteEvent -= new RemoteEventHandler(eventSinker.EventFired);
            remoteObj.OnRemoteEvent += new RemoteEventHandler(eventSinker.EventFired);
            //remoteObj.SinkEvent(id1, new RemoteEventHandler(EventFired));
            //remoteObj.SinkEvent(id2, new RemoteEventHandler(EventFired));
            DoSomethingDelegate do1 = new DoSomethingDelegate(remoteObj.DoSomething);
            DoSomethingDelegate do2 = new DoSomethingDelegate(remoteObj.DoSomething);
            do1.BeginInvoke(id1, null, null);
            do2.BeginInvoke(id2, null, null);

            Console.ReadLine();
        }

        static void PrintMessage(string strText)
        {
            Console.WriteLine(strText);
        }
    }

    
}
