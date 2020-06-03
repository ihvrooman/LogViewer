using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml.Serialization;

namespace LogViewerCustomSettings
{
    [Serializable]
    public class LogFolderSettings
    {
        #region Properties
        public List<LogFolderSetting> Settings { get; set; }
        #endregion

        #region Constructor
        public LogFolderSettings()
        {
            Settings = new List<LogFolderSetting>();
        }
        #endregion
    }
}
