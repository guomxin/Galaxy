using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.RemoteInterfaces
{
    [Serializable]
    public class GalaxyJobProperties
    {
        #region Variables
        public static string ms_strJobStatusPropName = "GalaxyJobStatus";
        public static string ms_strPropValueOfSucJob = "Successfull";
        public static string ms_strPropValueOfFailedJob = "Failed";
       
        private Dictionary<string, string> m_properties;
        #endregion

        #region Properties
        public Dictionary<string, string> Properties
        {
            get { return m_properties; }
            set { m_properties = value; }
        }
        #endregion

        public GalaxyJobProperties()
        {
            m_properties = new Dictionary<string, string>();
        }
    }

    public interface IJobStatusManager
    {
        /// <summary>
        /// Set the property of the job
        /// </summary>
        /// <param name="iProcessId">The id of the job process</param>
        /// <param name="strPropertyName">The name of the property</param>
        /// <param name="strPropertyValue">The value of the property</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - something wrong
        /// </returns>
        int SetJobProperty(int iProcessId, string strPropertyName, string strPropertyValue);

        /// <summary>
        /// Get all the properties of a job
        /// </summary>
        /// <param name="iProcessId">The id of the process</param>
        /// <param name="jobProperties">[OUT] The properties of the job</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - the properties of the job isn't found
        /// </returns>
        int GetJobProperties(int iProcessId, out GalaxyJobProperties jobProperties);

        /// <summary>
        /// Remove job from the job statuses list
        /// </summary>
        /// <param name="iProcessId">The id of the process</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - the id of the job doesn't exist
        /// </returns>
        int RemoveJob(int iProcessId);

        void ClearJobs();

        int GetJobCount();
    }
}
