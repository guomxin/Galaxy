using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.RemoteInterfaces
{
    public delegate void OutputCmdInfoHandler(string strCmdInfo);
    public delegate void CmdFinishHandler();

    public delegate bool RunRemoteCommandHandler(string strCmdLine, out string strProcessDumpInfo);

    public interface IRemoteController
    {
        bool RunRemoteCommand(string strCmdLine, out string strProcessDumpInfo);
        
        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        int AddCmdOutputEventSinker(OutputCmdInfoHandler outputCmdInfo);
        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        int AddCmdFinishEventSinker(CmdFinishHandler cmdFinish);

        void IsLive();
    }
}
