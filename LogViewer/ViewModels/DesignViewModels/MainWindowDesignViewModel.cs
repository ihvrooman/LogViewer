using AppStandards;
using AppStandards.Logging;
using LogViewer.Helpers;
using LogViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.ViewModels.DesignViewModels
{
    /// <summary>
    /// A <see cref="MainWindowViewModel"/> with design time data.
    /// </summary>
    internal class MainWindowDesignViewModel : MainWindowViewModel
    {
        #region Fields
        private static MainWindowDesignViewModel _instance;
        #endregion

        #region Properties
        /// <summary>
        /// An instance of the <see cref="MainWindowDesignViewModel"/>.
        /// </summary>
        public static MainWindowDesignViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainWindowDesignViewModel();
                }
                return _instance;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="MainWindowDesignViewModel"/> and fills it with design time data.
        /// </summary>
        public MainWindowDesignViewModel() : base(true)
        {
            GenerateDesignTimeData();
        }
        #endregion

        #region Private methods
        private void GenerateDesignTimeData()
        {
            var parentLogFolder = new LogFolder(@"C:\WY\Test Logs", this);
            var parentLogFile = new LogFile(@"C:\WY\Test Logs\20190523_TestApp", parentLogFolder);
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i <= 9; i++)
                {
                    //LogEntries.Add(new LogEntry((LogMessageType)i, DateTime.UtcNow, Environment.UserName, Environment.MachineName, "New message", parentLogFile));
                }
            }
        }
        #endregion
    }
}
