using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AppStandards;
using AppStandards.Logging;
using AppStandards.MVVM;
using LogViewer.Helpers;
using LogViewer.Services;
using LogViewer.ViewModels;

namespace LogViewer.Models
{
    public class LogFile : PropertyChangedHelper, INotification, IParentLog
    {
        #region Fields
        private volatile bool _keepCurrent;
        private FileStream _fileStream;
        private volatile bool _pendingUpdate;
        private volatile bool _hasError;
        private volatile bool _updating;
        private volatile bool _initialized;
        private volatile bool _disposingOfLogEntries;
        private volatile int _uniqueId = LogFileIdManager.GetNextAvailableId();
        private volatile bool _canBeSelected = false;
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="FileInfo"/> which represents the local copy of the log file.
        /// </summary>
        public FileInfo LocalFile { get; set; }
        /// <summary>
        /// The <see cref="FileInfo"/> which represents the network copy of the log file.
        /// </summary>
        public FileInfo NetworkFile { get; set; }
        /// <summary>
        /// Indicates whether or not the log file should be kept up to date in memory.
        /// </summary>
        public bool KeepCurrent
        {
            get
            {
                return _keepCurrent;
            }
            set
            {
                if (value != KeepCurrent)
                {
                    _keepCurrent = value;
                    RaisePropertyChangedEvent();

                    //Update parent log folder number of selected log files property
                    var update = _keepCurrent ? 1 : -1;
                    ParentLogFolder.NumberOfLogFilesSelected += update;

                    //Update, set pending update, or clear log entries
                    if (Initialized && _keepCurrent)
                    {
                        Task.Run(() =>
                        {
                            Task.Delay(2000).Wait();
                            ParentLogFolder.MainWindowViewModel.RequestReloadOfUILogEntries($"Log folder with network path \"{NetworkFile.FullName}\" selected for viewing.");
                        });
                        if (Updating)
                        {
                            _pendingUpdate = true;
                        }
                        else
                        {
                            UpdateAsync();
                        }
                    }
                    else
                    {
                        if (!DisposingOfLogEntries)
                        {
                            Task.Run(() =>
                            {
                                DisposeLogEntries();
                            });
                        }
                    }
                }
            }
        }
        /// <summary>
        /// The log entries read from the log file.
        /// </summary>
        public List<LogEntry> LogEntries = new List<LogEntry>();

        /// <summary>
        /// Indicates whether or not the log file is actively being updated.
        /// </summary>
        public bool Updating { get { return _updating; } set { _updating = value; SetCanBeSelectedProperty(); } }
        /// <summary>
        /// Indicates whether or not the log file has an error.
        /// </summary>
        public bool HasError { get { return _hasError; } set { _hasError = value; RaisePropertyChangedEvent(); SetCanBeSelectedProperty(); if (_hasError) { KeepCurrent = false; } } }
        /// <summary>
        /// The log file's <see cref="Notification"/>.
        /// </summary>
        public Notification Notification { get; private set; }
        /// <summary>
        /// The full path of the <see cref="ParentLogFolder"/>.
        /// </summary>
        public string Source1 { get; private set; }
        /// <summary>
        /// The log file's simple name. Usually contains the name of the application which created the log file.
        /// </summary>
        public string Source2 { get; private set; }
        /// <summary>
        /// The log file's creation date.
        /// </summary>
        public string CreationDate { get; private set; }
        /// <summary>
        /// The log file's parent <see cref="LogFolder"/>.
        /// </summary>
        public LogFolder ParentLogFolder { get; set; }
        public bool Initialized { get { return _initialized; } private set { _initialized = value; SetCanBeSelectedProperty(); } }
        public string ParentLogFolderFullName
        {
            get
            {
                var logFolder = ParentLogFolder.Folder;
                return logFolder != null ? logFolder.FullName : string.Empty;
            }
        }
        public bool DisposingOfLogEntries { get { return _disposingOfLogEntries; } private set { _disposingOfLogEntries = value; } }
        /// <summary>
        /// Holds a snapshot number of log entries used for loading the log entries into the user interface.
        /// </summary>
        public int SnapshotNumberOfLogEntries { get; private set; }
        /// <summary>
        /// The number of log entries that have been loaded into the user interface.
        /// </summary>
        public int NumberOfLogEntriesLoadedIntoUI { get; set; }
        /// <summary>
        /// Indicates whether or not all of the log entries indicated by the <see cref="SnapshotNumberOfLogEntries"/> have been loaded into the user interface.
        /// </summary>
        public bool AllLogEntriesLoadedIntoUI
        {
            get
            {
                return NumberOfLogEntriesLoadedIntoUI >= SnapshotNumberOfLogEntries;
            }
        }
        public bool CanBeSelected { get { return _canBeSelected; } private set { _canBeSelected = value; RaisePropertyChangedEvent(); } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="LogFile"/>.
        /// </summary>
        /// <param name="path">The log file's path.</param>
        /// <param name="parentLogFolder">The log file's parent <see cref="LogFolder"/>.</param>
        public LogFile(string path, LogFolder parentLogFolder)
        {
            ParentLogFolder = parentLogFolder;
            InitializeAsync(path);
        }

        /// <summary>
        /// Creates a new <see cref="LogFile"/>.
        /// </summary>
        /// <param name="path">The log file's path.</param>
        /// <param name="keepCurrent">A <see cref="bool"/> indicating whether or not the log file should be kept up to date in memory.</param>
        /// <param name="parentLogFolder">The log file's parent <see cref="LogFolder"/>.</param>
        public LogFile(string path, bool keepCurrent, LogFolder parentLogFolder)
        {
            ParentLogFolder = parentLogFolder;
            KeepCurrent = keepCurrent;
            InitializeAsync(path);
        }

        #endregion

        #region Public/internal methods
        /// <summary>
        /// Indicates whether or not two <see cref="LogFile"/>s are equal.
        /// </summary>
        /// <param name="firstLogFile">The first <see cref="LogFile"/>.</param>
        /// <param name="secondLogFile">The second <see cref="LogFile"/>.</param>
        /// <returns>A <see cref="bool"/> indicating whether or not the two <see cref="LogFile"/>s are equal.</returns>
        public static bool AreEqual(LogFile firstLogFile, LogFile secondLogFile)
        {
            if (firstLogFile == null && secondLogFile == null)
            {
                return true;
            }

            if (firstLogFile == null || secondLogFile == null || firstLogFile.NetworkFile == null || secondLogFile.NetworkFile == null)
            {
                return false;
            }

            return firstLogFile.NetworkFile.FullName == secondLogFile.NetworkFile.FullName;
        }

        public void PrepareToLoadLogEntriesIntoUI()
        {
            SnapshotNumberOfLogEntries = LogEntries.Count;
            NumberOfLogEntriesLoadedIntoUI = 0;
        }

        /// <summary>
        /// Reloads the <see cref="LogFile"/> which entails re-downloading the file from the network path.
        /// </summary>
        public void ReloadAsync()
        {
            Task.Run(() =>
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"User requested a reload of log file with network path \"{NetworkFile.FullName}\".");
                var keepCurrentAfterReload = KeepCurrent;
                Notification?.Clear();
                HasError = false;
                KeepCurrent = false;
                Task.Delay(100).Wait();
                var showWaitMessage = true;
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                Initialized = false;
                InitializeAsync(NetworkFile.FullName, true);
                Task.Delay(500).Wait();
                while (!Initialized && !HasError)
                {
                    if (showWaitMessage)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Waiting for log entry with network path \"{NetworkFile.FullName}\" to re-initialize before setting the KeepCurrent property as part of Reload operation.", LogMessageType.Verbose);
                        showWaitMessage = false;
                    }
                    Task.Delay(100).Wait();
                }
                stopWatch.Stop();
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Re-initialization of log file with network path \"{NetworkFile.FullName}\" took {stopWatch.Elapsed.Seconds} s {stopWatch.Elapsed.Milliseconds} ms. Setting KeepCurrent property to {keepCurrentAfterReload}", LogMessageType.Verbose);
                KeepCurrent = keepCurrentAfterReload;
                ParentLogFolder.DecrementNumberOfLogFilesReloading();
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Reload of log file with network path \"{NetworkFile.FullName}\" complete.", LogMessageType.Verbose);
            });
        }

        /// <summary>
        /// Clear's the log file's <see cref="Models.Notification"/>
        /// </summary>
        public void ClearNotification()
        {
            Notification = null;
        }

        /// <summary>
        /// Cleans up the <see cref="LogFile"/>.
        /// </summary>
        public void Dispose()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Initializes the <see cref="LogFile"/>.
        /// </summary>
        /// <param name="path">The log file's path.</param>
        private void InitializeAsync(string path, bool hasAlreadyBeenInitialized = false)
        {
            Task.Run(() =>
            {
                Initialized = false;
                ParentLogFolder.IncrementNumberOfLogFilesInitializing();

                var showWaitMessage = true;
                while (Updating)
                {
                    if (showWaitMessage)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Waiting for log entry with network path \"{NetworkFile.FullName}\" to cancel update before initializing.", LogMessageType.Verbose);
                        showWaitMessage = false;
                    }
                    Task.Delay(100).Wait();
                }

                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Initializing log file with network path \"{path}\".", LogMessageType.Verbose);
                NetworkFile = new FileInfo(path);
                var localPath = AppInfo.LocalLogFileFolderPath + $"UniqueId{_uniqueId}_" + ParentLogFolder.ComputerName + "_" + NetworkFile.Name;
                var stopWatch = new Stopwatch();
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Downloading local copy of log file from network path \"{NetworkFile.FullName}\" to local path \"{localPath}\".", LogMessageType.Verbose);
                var copySucceded = false;
                Exception exception = null;
                stopWatch.Start();
                while (!copySucceded && stopWatch.Elapsed.Minutes < 2)
                {
                    try
                    {
                        LocalFile = NetworkFile.CopyTo(localPath, true);
                        copySucceded = true;
                    }
                    catch (Exception ex)
                    {
                        var waitTimeInMilliseconds = 3000;
                        exception = ex;
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Download of local copy of log file with network path \"{NetworkFile.FullName}\" failed. Error message: {exception.Message} Trying again after {waitTimeInMilliseconds} milliseconds.", LogMessageType.Verbose);
                        Task.Delay(waitTimeInMilliseconds).Wait();
                    }
                }
                stopWatch.Stop();

                if (copySucceded)
                {
                    try
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Download of local copy of log file with network path \"{NetworkFile.FullName}\" completed in {stopWatch.Elapsed.Minutes} m {stopWatch.Elapsed.Seconds} s {stopWatch.Elapsed.Milliseconds} ms. Size {LocalFile.Length / 1000} KB.");
                    }
                    catch (Exception ex)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Failed to log completion of log file download. Error message: {ex.Message}", LogMessageType.Warning);
                    }
                }
                else
                {
                    var errorMessageString = exception != null ? $"Error message: {exception.Message}" : string.Empty;
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Download of local copy of log file with network path \"{NetworkFile.FullName}\" failed after {stopWatch.Elapsed.Minutes} m {stopWatch.Elapsed.Seconds} s {stopWatch.Elapsed.Milliseconds} ms. {errorMessageString}", LogMessageType.Error);

                    HasError = true;
                    Notification = new Notification(this, $"Could not download local copy of log file with network path \"{NetworkFile.FullName}\".");
                }

                if (!hasAlreadyBeenInitialized)
                {
                    //Extract the creation date from the file name
                    var extractAppNameFromFileName = true;
                    try
                    {
                        var year = NetworkFile.Name.Substring(2, 2);
                        var month = NetworkFile.Name.Substring(4, 2);
                        var day = NetworkFile.Name.Substring(6, 2);
                        CreationDate = $"{month}/{day}/{year}";
                        Convert.ToDateTime(CreationDate);
                    }
                    catch (Exception ex)
                    {
                        NetworkFile.Refresh();
                        if (NetworkFile.Exists)
                        {
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not extract creation date from log file with path \"{NetworkFile.FullName}\" Showing windows file system creation time instead. Error message: {ex.Message}", LogMessageType.Verbose);
                            CreationDate = NetworkFile.CreationTime.ToString("MM/dd/yy");
                        }
                        else
                        {
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not set creation date for log file with path \"{NetworkFile.FullName}\" becuase the file no longer exists.", LogMessageType.Warning);
                        }
                        extractAppNameFromFileName = false;
                    }

                    //Set parent log folder full path
                    Source1 = ParentLogFolder.Folder.FullName;

                    //Extract the application name from the file name
                    if (extractAppNameFromFileName)
                    {
                        try
                        {
                            var startingPoint = 9;
                            Source2 = NetworkFile.Name.Substring(startingPoint, NetworkFile.Name.Length - startingPoint - 4);
                        }
                        catch (Exception ex)
                        {
                            Source2 = "Unknown";
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not extract application name from log file with path \"{NetworkFile.FullName}\". Error message: {ex.Message}", LogMessageType.Error);
                        }
                    }
                    else
                    {
                        Source2 = NetworkFile.Name;
                    }
                }

                ParentLogFolder.DecrementNumberOfLogFilesInitializing();

                Initialized = !HasError;

            });
        }

        /// <summary>
        /// Initializes the <see cref="FileStream"/> used to access the log file.
        /// </summary>
        /// <param name="path">The log file's path.</param>
        private void InitializeFileStream(string path)
        {
            _fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        /// <summary>
        /// Updates the log file's in-memory collection of <see cref="LogEntry"/>s.
        /// </summary>
        private void UpdateAsync()
        {
            Task.Run(() =>
            {
                try
                {
                    Updating = true;
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Loading/reloading log entries for log file with network path \"{NetworkFile.FullName}\".", LogMessageType.Verbose);
                    var logWaitMessage = true;
                    while (KeepCurrent && DisposingOfLogEntries)
                    {
                        if (logWaitMessage)
                        {
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Waiting for log file with local path: \"{LocalFile.FullName}\" to finish disposing log entries before updating.");
                            logWaitMessage = false;
                        }
                        Task.Delay(100).Wait();
                    }

                    if (!KeepCurrent)
                    {
                        return;
                    }

                    InitializeFileStream(LocalFile.FullName);
                    var newLogEntryStrings = new List<string>();
                    using (var sr = new StreamReader(_fileStream))
                    {
                        newLogEntryStrings = LogService.GetLogEntriesFromLogContents(sr.ReadToEnd());
                    }

                    if (!KeepCurrent)
                    {
                        return;
                    }

                    if (newLogEntryStrings.Count > 0)
                    {
                        for (int i = newLogEntryStrings.Count - 1; i >= 0; i--)
                        {
                            var newLogEntryString = newLogEntryStrings[i];
                            var newLogEntry = LogService.ParseLogEntry(newLogEntryString, this);
                            if (!KeepCurrent)
                            {
                                break;
                            }

                            if (MainWindowViewModel.LogService.CanAddLogEntry())
                            {
                                LogEntries.Add(newLogEntry);
                            }
                            else if (!LogService.LogEntrySpaceAvailableInMemory)
                            {
                                //If hit max log entry count, break
                                break;
                            }
                        }
                    }
                    Updating = false;

                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Found {LogEntries.Count} log entries in log file with network path \"{NetworkFile.FullName}\".", LogMessageType.Verbose);

                    if (Initialized && KeepCurrent && _pendingUpdate)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Log file with network path \"{NetworkFile.FullName}\" has a pending update. Clearing pending update and calling Update() method.", LogMessageType.Verbose);
                        _pendingUpdate = false;
                        UpdateAsync();
                    }
                }
                catch (Exception ex)
                {
                    HasError = true;
                    var filePath = "File is NULL";
                    if (NetworkFile != null)
                    {
                        filePath = NetworkFile.FullName;
                    }
                    else if (LocalFile != null)
                    {
                        filePath = LocalFile.FullName;
                    }
                    Notification = new Notification(this, $"Could not update log file with path \"{filePath}\". Error message: {ex.Message}");
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(Notification.Message, LogMessageType.Error);
                }
                finally
                {
                    Updating = false;
                }
            });
        }

        private void SetCanBeSelectedProperty()
        {
            //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Setting {GetType().Name}.CanBeSelectedProperty. HasError: {HasError} | Initialized: {Initialized} | New CanBeSelected value: {!HasError && Initialized && !Updating}", LogMessageType.Debug);
            CanBeSelected = !HasError && Initialized;
        }

        private void DisposeLogEntries()
        {
            DisposingOfLogEntries = true;
            try
            {
                if (NetworkFile != null)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Disposing of log entries for log file with network path \"{NetworkFile.FullName}\".", LogMessageType.Verbose);
                }
                while (LogEntries.Count > 0)
                {
                    var logEntryIndex = LogEntries.Count - 1;
                    var logEntry = LogEntries[logEntryIndex];
                    logEntry.ParentLog = null;
                    LogEntries.RemoveAt(logEntryIndex);
                    LogService.InMemoryLogEntryCount--;
                }
            }
            catch (Exception ex)
            {
                Messages.ErrorMessage($"An unexpected error occurred and {AppInfo.BaseAppInfo.AppName} must close.", AppInfo.BaseAppInfo, logMessage: $"Disposal of log entries for log file with network path \"{NetworkFile.FullName}\" failed. Error message: {ex.Message}", forceQuit: true);
            }
            DisposingOfLogEntries = false;
        }
        #endregion
    }
}
