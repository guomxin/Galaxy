using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.RemoteInterfaces;
using System.Collections;

namespace Galaxy.ManagedClientLib
{
    public class CmdOutputEventSinker : MarshalByRefObject
    {
        #region Variables
        public OutputCmdInfoHandler OutputCmdInfoCallBack;
        public CmdFinishHandler CmdFinishCallBack;
        #endregion

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void OnCmdOutput(string strCmdInfo)
        {
            if (OutputCmdInfoCallBack != null)
            {
                Delegate[] invokeList = OutputCmdInfoCallBack.GetInvocationList();
                IEnumerator ie = invokeList.GetEnumerator();
                while (ie.MoveNext())
                {
                    OutputCmdInfoHandler outputCmdInfo = ie.Current as OutputCmdInfoHandler;
                    try
                    {
                        outputCmdInfo.Invoke(strCmdInfo);
                    }
                    catch (Exception)
                    {
                        OutputCmdInfoCallBack -= outputCmdInfo;
                    }
                }
            }
        }

        public void OnCmdFinish()
        {
            if (CmdFinishCallBack != null)
            {
                Delegate[] invokeList = CmdFinishCallBack.GetInvocationList();
                IEnumerator ie = invokeList.GetEnumerator();
                while (ie.MoveNext())
                {
                    CmdFinishHandler cmdFinish = ie.Current as CmdFinishHandler;
                    try
                    {
                        cmdFinish.Invoke();
                    }
                    catch (Exception)
                    {
                        CmdFinishCallBack -= cmdFinish;
                    }
                }
            }
        }
    }
}
