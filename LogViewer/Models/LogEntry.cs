using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using AppStandards.Logging;
using LogViewer.ViewModels;

namespace LogViewer.Models
{
    /// <summary>
    /// Represents a log entry.
    /// </summary>
    public class LogEntry
    {
        #region Properties
        /// <summary>
        /// The log entry's <see cref="LogMessageType"/>.
        /// </summary>
        public LogMessageType Type { get; set; }
        /// <summary>
        /// The log entry's UTC timestamp.
        /// </summary>
        public DateTime UtcTimeStamp { get; set; }
        /// <summary>
        /// The log entry's <see cref="UtcTimeStamp"/> converted to local time.
        /// </summary>
        public string TimeStamp
        {
            get
            {
                return UtcTimeStamp.ToLocalTime().ToString("MM/dd/yyy HH:mm:ss:ffff");
            }
        }
        /// <summary>
        /// The username which was used to write to the log.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The computer that the log entry was recorded from.
        /// </summary>
        public string Computername { get; set; }
        /// <summary>
        /// The log entry's log message.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The log entry's parent Log.
        /// </summary>
        public IParentLog ParentLog { get; set; }
        /// <summary>
        /// Indicates whether or not the log entry should be included based on the filter requirements.
        /// </summary>
        public bool Include { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="type">The <see cref="LogMessageType"/>.</param>
        /// <param name="UtcTimeStamp">The UTC timestamp.</param>
        /// <param name="message">The log message.</param>
        /// <param name="parentLog">The parent Log.</param>
        public LogEntry(LogMessageType type, DateTime UtcTimeStamp, string username, string computername, string message, IParentLog parentLog)
        {
            Type = type;
            this.UtcTimeStamp = UtcTimeStamp;
            Username = username;
            Computername = computername;
            Message = message;
            ParentLog = parentLog;
        }
        #endregion

        #region Public/Internal methods
        /// <summary>
        /// Indicates whether or not two <see cref="LogEntry"/>s are equal.
        /// </summary>
        /// <param name="firstLogEntry">The first <see cref="LogEntry"/>.</param>
        /// <param name="secondLogEntry">The second <see cref="LogEntry"/>.</param>
        /// <returns>A <see cref="bool"/> which indicates whether or not the two <see cref="LogEntry"/>s are equal.</returns>
        public static bool AreEqual(LogEntry firstLogEntry, LogEntry secondLogEntry)
        {
            if (firstLogEntry == null && secondLogEntry == null)
            {
                return true;
            }

            if (firstLogEntry == null || secondLogEntry == null)
            {
                return false;
            }

            return firstLogEntry.UtcTimeStamp == secondLogEntry.UtcTimeStamp && firstLogEntry.Message == secondLogEntry.Message && firstLogEntry.Type == secondLogEntry.Type;
        }

        internal bool UpdateIncludeProperty()
        {
            Include = ParentLog.KeepCurrent && MainWindowViewModel.LogService.IncludeLogEntry(this);
            return Include;
        }
        #endregion
    }
}
