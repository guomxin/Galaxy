using System;
using System.Collections;

using Galaxy.RemoteInterfaces;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Galaxy.ManagedClientLib
{
    public delegate void JobFinishEventHandler(GalaxyJobFinishInfo jobFinishInfo);
    public delegate void JobStartEventHandler(GalaxyJobStartInfo jobStartInfo);


    public class JobEventSinker:MarshalByRefObject, IJobEventSinker
    {
        #region Variables
        public JobFinishEventHandler JobFinishCallBack;
        public JobStartEventHandler JobStartCallBack;
        private bool m_fReady;
        #endregion

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #region Constructors and Initializers
        public JobEventSinker()
        {
            m_fReady = false;
        }

        public bool Initialize(int iPort)
        {
            bool fRet = true;

            try
            {
                BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
                serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();
                IDictionary props = new Hashtable();
                props["port"] = iPort;
                TcpChannel tcpChannel = new TcpChannel(props, clientProv, serverProv);
                ChannelServices.RegisterChannel(tcpChannel, true);
            }
            catch (RemotingException)
            {
                fRet = false;
            }
            catch (System.Net.Sockets.SocketException)
            {
                fRet = false;
            }
            catch (Exception)
            {
                fRet = false;
            }
            m_fReady = fRet;

            return fRet;
        }

        #endregion

        #region IJobEventSinker memebers
        public void OnJobFinished(GalaxyJobFinishInfo jobFinishInfo)
        {
            if (!m_fReady) { return; }

            if (JobFinishCallBack != null)
            {
                Delegate[] invokeList = JobFinishCallBack.GetInvocationList();
                IEnumerator ie = invokeList.GetEnumerator();
                while (ie.MoveNext())
                {
                    JobFinishEventHandler jobFinishListener = ie.Current as JobFinishEventHandler;
                    try
                    {
                        jobFinishListener.BeginInvoke(jobFinishInfo, null, null);
                    }
                    catch (Exception)
                    {
                        JobFinishCallBack -= jobFinishListener;
                    }
                }
            }
        }

        public void OnJobStarted(GalaxyJobStartInfo jobStartInfo)
        {
            if (!m_fReady) { return; }

            if (JobStartCallBack != null)
            {
                Delegate[] invokeList = JobStartCallBack.GetInvocationList();
                IEnumerator ie = invokeList.GetEnumerator();
                while (ie.MoveNext())
                {
                    JobStartEventHandler jobStartListener = ie.Current as JobStartEventHandler;
                    try
                    {
                        jobStartListener.BeginInvoke(jobStartInfo, null, null);
                    }
                    catch (Exception)
                    {
                        JobStartCallBack -= jobStartListener;
                    }
                }
            }
        }
        #endregion
    }
}
