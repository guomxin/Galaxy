using System;

namespace Galaxy.RemoteInterfaces
{
    public interface IJobEventSinker
    {
        void OnJobFinished(GalaxyJobFinishInfo jobFinishInfo);

        void OnJobStarted(GalaxyJobStartInfo jobStartInfo);
    }
}