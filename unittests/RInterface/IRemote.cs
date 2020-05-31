using System;
using System.Collections.Generic;
using System.Text;

namespace TestRemoting
{
    public delegate void RemoteEventHandler(Guid id);

    public interface IRemote
    {
        event RemoteEventHandler OnRemoteEvent;

        void SinkEvent(Guid id, RemoteEventHandler eventHandler);

        void DoSomething(Guid id);
    }

    public interface IRemoteEventSink
    {
        void EventFired(Guid id);
    }
}
