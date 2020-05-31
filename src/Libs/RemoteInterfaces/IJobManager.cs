using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.RemoteInterfaces
{
    [Serializable]
    public class GalaxyJobInput
    {
        #region Variables
        // The input is the output of a previous job,
        // null means we need not use another job's data directory
        // to figure out the absolute path of the input
        private string m_strFromJobId;
        // The input file name, the real input file is
        //   m_strInputFileName, (m_strFromJobId == null)
        //   {JobDataDir(m_strFromJobId)} + "\\" + m_strInputFileName, otherwise
        private string m_strInputFileName;
        #endregion

        #region Property
        public string FromJobId
        {
            get { return m_strFromJobId; }
            set { m_strFromJobId = value; }
        }

        public string InputFileName
        {
            get { return m_strInputFileName; }
            set { m_strInputFileName = value; }
        }
        #endregion
    }

    [Serializable]
    public class GalaxyJobOutput
    {
        #region Variables
        private string m_strOutputFileName;
        #endregion

        #region Property
        public string OutputFileName
        {
            get { return m_strOutputFileName; }
            set { m_strOutputFileName = value; }
        }

        #endregion
    }

    [Serializable]
    public class GalaxyJobProperty
    {
        #region Variables
        private string m_strPropertyName;
        private string m_strPropertyValue;
        #endregion

        #region Property
        public string PropertyName
        {
            get { return m_strPropertyName; }
            set { m_strPropertyName = value; }
        }

        public string PropertyValue
        {
            get { return m_strPropertyValue; }
            set { m_strPropertyValue = value; }
        }
        #endregion
    }

    [Serializable]
    public class GalaxyJobInfo
    {
        #region Variables
        // The id of the job, a unique string
        private string m_strJobId;
        private List<GalaxyJobInput> m_jobInputs;
        private List<GalaxyJobProperty> m_jobProperties;
        private List<GalaxyJobOutput> m_jobOutputs;
        #endregion

        #region Constructors
        public GalaxyJobInfo(string strJobId)
        {
            m_strJobId = strJobId;
            m_jobInputs = new List<GalaxyJobInput>();
            m_jobProperties = new List<GalaxyJobProperty>();
            m_jobOutputs = new List<GalaxyJobOutput>();
        }
        #endregion

        #region Properties
        public string JobId
        {
            get { return m_strJobId; }
            set { m_strJobId = value; }
        }

        public List<GalaxyJobInput> JobInputs
        {
            get { return m_jobInputs; }
        }

        public List<GalaxyJobOutput> JobOutputs
        {
            get { return m_jobOutputs; }
        }

        public List<GalaxyJobProperty> JobProperties
        {
            get { return m_jobProperties; }
        }
        #endregion
    }

    [Serializable]
    public class GalaxyJobGraphInfo
    {
        #region Variables
        private List<GalaxyJobInfo> m_jobInfos;
        #endregion

        #region Public methods
        public List<GalaxyJobInfo> JobInfos
        {
            get { return m_jobInfos; }
        }
        #endregion

        #region Constructors
        public GalaxyJobGraphInfo()
        {
            m_jobInfos = new List<GalaxyJobInfo>();
        }
        #endregion

    }

    public interface IJobManager
    {
        /// <summary>
        /// Run the job manager, it should never return
        /// It get the job graphs from the clients and monitor the jobs util they are finished
        /// </summary>
        /// <param name="iPortNumber">The port number</param>
        /// <param name="strDataDir">The root data dir of the job manager</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        int Run(int iPortNumber, string strDataDir);

        /// <summary>
        /// Get a id for the new job graph,
        /// we use this id to transport the executables and the resource files to the job manager
        /// <returns>
        ///     0 - successfully
        ///     -1 - something wrong
        /// </returns>
        /// </summary>
        int ApplyForNewJobGraph(out Guid jobGraphId);

        /// <summary>
        /// Transport job graph's resource files(config files, etc.) to the job manager
        /// </summary>
        /// <param name="rgBuf">The buffer of the bytes</param>
        /// <param name="strFileName">The file name</param>
        /// <param name="strRelativeDir">
        ///     The path relative to the job's data dir, empty means job's data dir
        /// </param>
        /// <param name="fAppend">We append the data to an existing file</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - something wrong
        /// </returns>
        int TransportData(Guid jobGraphId, string strJobId, byte[] rgBuf, string strFileName, string strRelativeDir, bool fAppend);

        /// <summary>
        /// Append a job graph request
        /// </summary>
        /// <param name="job">The information of the job graph</param>
        /// <returns>
        ///     0 - successfully
        ///     -1 - failed
        /// </returns>
        int AppendJobRequest(GalaxyJobGraphInfo jobGraph);
    }
}
