using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

namespace TestRemoting
{
    public class TestClass
    {
        public string m_strTest;
    }

    public delegate void NonRemoteCallBack(string strText);
    
    public class EventSinker : MarshalByRefObject, IRemoteEventSink
    {
        public NonRemoteCallBack m_callBack;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void EventFired(Guid id)
        {
            Console.WriteLine("EventFired");
            Console.WriteLine("Thread Id: " + Thread.CurrentThread.ManagedThreadId.ToString());
            Console.WriteLine("Object hash code: " + this.GetHashCode().ToString());
            Thread.Sleep(5000);
            Console.WriteLine(id.ToString());

            if (m_callBack != null)
            {
                m_callBack("Hello");
            }
        }

        public void EventFired2(Guid id)
        {
            Console.WriteLine("EventFired2");
            Console.WriteLine("Thread Id: " + Thread.CurrentThread.ManagedThreadId.ToString());
            Console.WriteLine("Object hash code: " + this.GetHashCode().ToString());
            Thread.Sleep(5000);
            Console.WriteLine(id.ToString());
        }
    }

    public class RemoteObj : MarshalByRefObject, IRemote
    {
        private Dictionary<Guid, RemoteEventHandler> m_events;

        public event RemoteEventHandler OnRemoteEvent;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public RemoteObj()
        {
            m_events = new Dictionary<Guid, RemoteEventHandler>();
        }

        public void SinkEvent(Guid id, RemoteEventHandler eventHandler)
        {
            m_events.Add(id, eventHandler);
        }

        public void DoSomething(Guid id)
        {
            int i = 0;
            for (int j = 0; j < 100000; j++)
            {
                for (int k = 0; k < 20000; k++)
                {
                    i += j;
                    i -= j;
                }
            }

            System.Delegate[] invokeList = OnRemoteEvent.GetInvocationList();
            IEnumerator ie = invokeList.GetEnumerator();
            while (ie.MoveNext())
            {
                RemoteEventHandler handler = ie.Current as RemoteEventHandler;
                try
                {
                    handler.BeginInvoke(id, null, null);
                }
                catch (Exception)
                {
                    OnRemoteEvent -= handler;
                    Console.WriteLine("Calling remote handler error!");
                }
            }
        }
    }
}
