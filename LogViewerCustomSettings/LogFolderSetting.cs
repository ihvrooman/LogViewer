using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LogViewerCustomSettings
{
    [Serializable]
    public class LogFolderSetting
    {
        #region Properties
        public string Path { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Constructors
        public LogFolderSetting()
        {
            Path = string.Empty;
            IsActive = true;
        }

        public LogFolderSetting(string path, bool isActive = true)
        {
            Path = path;
            IsActive = isActive;
        }
        #endregion
    }
}
