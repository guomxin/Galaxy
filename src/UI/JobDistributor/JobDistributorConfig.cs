using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace JobDistributor
{
    class PNInstance
    {
        #region Variables
        private string m_strMachineName;
        private int m_iPort;
        #endregion

        #region Properties
        public string MachineName
        {
            get { return m_strMachineName; }
            set { m_strMachineName = value; }
        }

        public int Port
        {
            get { return m_iPort; }
            set { m_iPort = value; }
        }
        #endregion
    }

    class JobDistributorConfig
    {
        #region Variables
        private int m_iListeningPort;
        private Dictionary<string, PNInstance> m_pnDict;
        #endregion

        #region Properties
        public int ListeningPort
        {
            get { return m_iListeningPort; }
            set { m_iListeningPort = value; }
        }

        public Dictionary<string, PNInstance> PNDictionary
        {
            get { return m_pnDict; }
            set { m_pnDict = value; }
        }
        #endregion

        #region Public functions

        public bool ParseFromConfigFile(string strConfigFileName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(strConfigFileName);
                XmlElement rootElement = doc.DocumentElement;
                if (rootElement.LocalName.CompareTo("JobDistributorConfig") != 0)
                {
                    return false;
                }
                
                // Get the listening port
                XmlNode listeningPortNode = rootElement.SelectSingleNode("ListeningPort");
                m_iListeningPort = Int32.Parse(listeningPortNode.InnerText);
                
                // Get the process node instances
                XmlNode processNodes = rootElement.SelectSingleNode("ProcessNodes");
                XmlNodeList processNodeList = processNodes.SelectNodes("ProcessNode");
                m_pnDict = new Dictionary<string, PNInstance>();
                foreach (XmlNode processNode in processNodeList)
                {
                    PNInstance pnInstance = new PNInstance();
                    XmlNode nameNode = processNode.SelectSingleNode("Name");
                    string strPNName = nameNode.InnerText.Trim();
                    XmlNode machineNode = processNode.SelectSingleNode("Machine");
                    pnInstance.MachineName = machineNode.InnerText.Trim();
                    XmlNode portNode = processNode.SelectSingleNode("Port");
                    pnInstance.Port = Int32.Parse(portNode.InnerText.Trim());
                    m_pnDict.Add(strPNName, pnInstance);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
