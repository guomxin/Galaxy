using System;
using System.Collections.Generic;
using System.Text;

using Galaxy.RemoteInterfaces;

namespace Galaxy.ProcessNode
{
    /// <summary>
    /// Singleton
    /// </summary>
    class JobStatusManager : MarshalByRefObject, IJobStatusManager
    {
        #region Variables
        private Dictionary<int, GalaxyJobProperties> m_jobProps;

        // This is a singleton object, we need a lock to synchronize the access of m_jobProps
        private static object ms_accessJobPropsLock;
        #endregion

        #region Constructors
        public JobStatusManager()
        {
            m_jobProps = new Dictionary<int, GalaxyJobProperties>();
            ms_accessJobPropsLock = new object();
        }
        #endregion

        #region About the lease
        public override object InitializeLifetimeService()
        {
            // The lease will never expire
            return null;
        }
        #endregion

        #region IJobStatusManager interface
        public int SetJobProperty(int iProcessId, string strPropName, string strPropValue)
        {
            lock (ms_accessJobPropsLock)
            {
                GalaxyJobProperties jobProperties = null;
                if (m_jobProps.ContainsKey(iProcessId))
                {
                    jobProperties = m_jobProps[iProcessId];
                }
                else
                {
                    jobProperties = new GalaxyJobProperties();
                    m_jobProps.Add(iProcessId, jobProperties);
                }
                if (jobProperties.Properties.ContainsKey(strPropName))
                {
                    jobProperties.Properties[strPropName] = strPropValue;
                }
                else
                {
                    jobProperties.Properties.Add(strPropName, strPropValue);
                }
            }

            return 0;
        }

        public int GetJobProperties(int iProcessId, out GalaxyJobProperties jobProperties)
        {
            lock (ms_accessJobPropsLock)
            {
                if (m_jobProps.ContainsKey(iProcessId))
                {
                    jobProperties = m_jobProps[iProcessId];
                }
                else
                {
                    jobProperties = null;
                    return -1;
                }
            }

            return 0;
        }

        public int RemoveJob(int iProcessId)
        {
            lock (ms_accessJobPropsLock)
            {
                if (!m_jobProps.ContainsKey(iProcessId)) { return -1; }
                m_jobProps.Remove(iProcessId);
            }

            return 0;
        }

        public int GetJobCount()
        {
            int iJobCount = 0;
            lock (ms_accessJobPropsLock)
            {
                iJobCount = m_jobProps.Count;
            }
            return iJobCount;
        }

        public void ClearJobs()
        {
            lock (ms_accessJobPropsLock)
            {
                m_jobProps.Clear();
            }
        }
        #endregion
    }
}
