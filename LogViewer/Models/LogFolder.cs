using AppStandards;
using AppStandards.Helpers;
using AppStandards.Logging;
using AppStandards.MVVM;
using LogViewer.Helpers;
using LogViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LogViewer.Models
{
    public class LogFolder : PropertyChangedHelper, INotification
    {
        #region Fields
        private volatile bool _isActive;
        private volatile bool _initialized;
        private volatile bool _hasError;
        private string _computerName;
        private volatile int _numberOfLogFilesSelected;
        private volatile int _numberOfLogFilesInitializing;
        private volatile bool _logFilesInitializing;
        private volatile int _numberOfLogFilesReloading;
        private volatile bool _logFilesReloading;
        private ICommand _reloadLogFilesCommand;
        private volatile bool _showReloadButton;
        private readonly object _logFilesInitializingLock = new object();
        private readonly object _logFilesReloadingLock = new object();
        #endregion

        #region Properties
        /// <summary>
        /// The computer name.
        /// </summary>
        public string ComputerName { get { return _computerName; } set { _computerName = value; RaisePropertyChangedEvent(); } }
        /// <summary>
        /// The log folder.
        /// </summary>
        public DirectoryInfo Folder { get; set; }
        /// <summary>
        /// The log files found in the log folder.
        /// </summary>
        public List<LogFile> LogFiles { get; set; } = new List<LogFile>();
        /// <summary>
        /// Indicates whether or not the <see cref="LogFiles"/> should be kept up to date in memory.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                RaisePropertyChangedEvent();
                
                if (!_isActive)
                {
                    UpdateLogFilesKeepCurrentPropertyAsync();
                }
                else if (!_initialized)
                {
                    InitializeAsync();
                }
                else
                {
                    ReloadLogFiles();
                }
            }
        }
        /// <summary>
        /// Indicates whether or not the log folder has an error.
        /// </summary>
        public bool HasError { get { return _hasError; } set { _hasError = value; RaisePropertyChangedEvent(); if (_hasError) { IsActive = false; } } }
        /// <summary>
        /// The log folder's <see cref="Models.Notification"/>.
        /// </summary>
        public Notification Notification { get; private set; }
        public int NumberOfLogFilesSelected
        {
            get { return _numberOfLogFilesSelected; }
            set
            {
                //Update MainWindowViewModel number of selected log files property
                var initialValue = _numberOfLogFilesSelected;
                var newValue = value;
                var diff = newValue - initialValue;

                MainWindowViewModel.LogService.MainWindowViewModel.NumberOfLogFilesSelected += diff;

                _numberOfLogFilesSelected = value;
                RaisePropertyChangedEvent();
            }
        }
        /// <summary>
        /// Indicates how many log files are initializing.
        /// </summary>
        private int NumberOfLogFilesInitializing { get { return _numberOfLogFilesInitializing; } set { _numberOfLogFilesInitializing = value; LogFilesInitializing = NumberOfLogFilesInitializing > 0; } }
        /// <summary>
        /// Indicates whether or not any log files are initializing.
        /// </summary>
        private bool LogFilesInitializing
        {
            get { return _logFilesInitializing; }
            set
            {
                var reEvaluate = value != LogFilesInitializing;
                _logFilesInitializing = value;

                if (reEvaluate)
                {
                    SetShowReloadButtonProperty();
                    RelayCommand.ReEvaluateCanExecute();
                }
            }
        }
        /// <summary>
        /// Indicates how many log files are reloading.
        /// </summary>
        private int NumberOfLogFilesReloading { get { return _numberOfLogFilesReloading; } set { _numberOfLogFilesReloading = value; LogFilesReloading = NumberOfLogFilesReloading > 0; } }
        /// <summary>
        /// Indicates whether or not any log files are reloading.
        /// </summary>
        private bool LogFilesReloading
        {
            get { return _logFilesReloading; }
            set
            {
                var reEvaluate = value != LogFilesReloading;
                _logFilesReloading = value;

                if (reEvaluate)
                {
                    SetShowReloadButtonProperty();
                    RelayCommand.ReEvaluateCanExecute();
                }
            }
        }
        public ICommand ReloadLogFilesCommand
        {
            get
            {
                if (_reloadLogFilesCommand == null)
                {
                    _reloadLogFilesCommand = new RelayCommand(ConfirmReloadLogFiles, CanReloadLogFiles);
                }
                return _reloadLogFilesCommand;
            }
        }
        public MainWindowViewModel MainWindowViewModel { get; set; }
        public bool ShowReloadButton { get { return _showReloadButton; } private set { _showReloadButton = value; RaisePropertyChangedEvent(); } }
        public TimeSpan RemoteComputerOffsetFromLocalTime { get; private set; } = new TimeSpan(0, 0, 0);
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="LogFolder"/>.
        /// </summary>
        /// <param name="path">The log folder's path.</param>
        /// <param name="mainWindowViewModel">The application's <see cref="MainWindowViewModel"/>.</param>
        /// <param name="isActive">Indicates whether or not the <see cref="LogFolder"/> is active.</param>
        public LogFolder(string path, MainWindowViewModel mainWindowViewModel, bool isActive = true)
        {
            try
            {
                MainWindowViewModel = mainWindowViewModel;
                Folder = new DirectoryInfo(path);
                IsActive = isActive;
            }
            catch (Exception ex)
            {
                HasError = true;
                Notification = new Notification(this, $"Cannot initialize log folder with path \"{path}\". Error message: {ex.Message}.");
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(Notification.Message, LogMessageType.Error);
                ComputerName = "Error";
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Initializes the <see cref="LogFolder"/>.
        /// </summary>
        private void InitializeAsync()
        {
            Task.Run(() =>
            {
                _initialized = true;
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Initializing log folder with path \"{Folder.FullName}\".", LogMessageType.Verbose);

                //Extract computer name from log folder path
                var chars = Folder.FullName.ToCharArray();
                var startingIndex = 2;
                for (int i = startingIndex; i < Folder.FullName.Length; i++)
                {
                    if (chars[i] == '\\')
                    {
                        ComputerName = Folder.FullName.Substring(startingIndex, i - startingIndex);
                        break;
                    }
                }

                //Get remote computer offset from local time
                try
                {
                    RemoteComputerOffsetFromLocalTime = ComputerHelper.GetRemoteComputerOffsetFromLocalTime(ComputerName);
                }
                catch (Exception ex)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not get remote computer \"{ComputerName}\"'s offset from local time. Error message: {ex.Message}", LogMessageType.Warning);
                }

                //Determine whether or not log folder exists
                Folder?.Refresh();
                if (!Folder.Exists)
                {
                    HasError = true;
                    Notification = new Notification(this, $"Could not find log folder with path \"{Folder.FullName}\".");
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(Notification.Message, LogMessageType.Error);
                    return;
                }

                LoadLogFilesAsync();
            });
        }

        /// <summary>
        /// Updates the log folder's in-memory collection of <see cref="LogFile"/>s.
        /// </summary>
        private void LoadLogFilesAsync()
        {
            Task.Run(() =>
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Loading log files for log folder with path \"{Folder.FullName}\".", LogMessageType.Verbose);

                Folder?.Refresh();
                var logFolderExists = Folder.Exists;

                try
                {
                    //Get list of log file names that are already loaded
                    var currentLogFileNames = new List<string>();
                    foreach (var logFile in LogFiles)
                    {
                        currentLogFileNames.Add(logFile.NetworkFile.FullName);
                    }

                    foreach (var file in Folder.GetFiles())
                    {
                        if (!currentLogFileNames.Contains(file.FullName))
                        {
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Adding log file with name: \"{file.Name}\" to log folder with path \"{Folder.FullName}\".", LogMessageType.Verbose);
                            LogFiles.Add(new LogFile(file.FullName, this));
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    logFolderExists = false;
                }
                catch (IOException)
                {
                    logFolderExists = false;
                }

                if (!logFolderExists)
                {
                    HasError = true;
                    Notification = new Notification(this, $"Could not find log folder with path \"{Folder.FullName}\".");
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(Notification.Message, LogMessageType.Error);
                }
                else
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Found {LogFiles.Count} log files in log folder with path \"{Folder.FullName}\".", LogMessageType.Verbose);
                }
            });
        }

        private bool CanReloadLogFiles()
        {
            return !LogFilesInitializing && !LogFilesReloading;
        }

        private void ConfirmReloadLogFiles()
        {
            if (Messages.ConfirmAction($"Are you sure that you want to reload all of the log files for the log folder with path \"{Folder.FullName}\"?{Environment.NewLine}This operation may take several minutes.", AppInfo.BaseAppInfo) == MessageBoxResult.Yes)
            {
                ReloadLogFiles();
            }
        }

        private void ReloadLogFiles()
        {
            //Reload existing log files
            foreach (var logFile in LogFiles)
            {
                IncrementNumberOfLogFilesReloading();
                logFile.ReloadAsync();
            }

            //Go look for new log files
            LoadLogFilesAsync();
        }

        private void DisposeLogFiles()
        {
            foreach (var logFile in LogFiles)
            {
                if (MainWindowViewModel.RequestCancelDispose)
                {
                    break;
                }
                logFile.Dispose();
            }
        }

        private void UpdateLogFilesKeepCurrentPropertyAsync()
        {
            Task.Run(() =>
            {
                foreach (var logFile in LogFiles)
                {
                    logFile.KeepCurrent = false;
                }
            });
        }

        private void SetShowReloadButtonProperty()
        {
            //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Setting {GetType().Name}.ShowReloadButton property. LogFilesInitializing: {LogFilesInitializing} | NumberOfLogFilesInitializing: {NumberOfLogFilesInitializing} | LogFilesReloading: {LogFilesReloading} | NumberOfLogFilesReloading: {NumberOfLogFilesReloading} | New ShowReloadButton value: {!LogFilesInitializing && !LogFilesReloading}", LogMessageType.Debug);
            ShowReloadButton = !LogFilesInitializing && !LogFilesReloading;
        }
        #endregion

        #region Public/internal methods
        internal void IncrementNumberOfLogFilesInitializing(int incrementValue = 1)
        {
            lock (_logFilesInitializingLock)
            {
                //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Incrementing {GetType().Name}.NumberOfLogFilesInitializing by {incrementValue}. Current value: {NumberOfLogFilesInitializing}", LogMessageType.Debug);
                NumberOfLogFilesInitializing += incrementValue;
            }
        }

        internal void DecrementNumberOfLogFilesInitializing(int decrementValue = 1)
        {
            lock (_logFilesInitializingLock)
            {
                if (NumberOfLogFilesInitializing >= decrementValue)
                {
                    //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Decrementing {GetType().Name}.NumberOfLogFilesInitializing by {decrementValue}. Current value: {NumberOfLogFilesInitializing}", LogMessageType.Debug);
                    NumberOfLogFilesInitializing -= decrementValue;
                }
            }
        }

        internal void IncrementNumberOfLogFilesReloading(int incrementValue = 1)
        {
            lock (_logFilesReloadingLock)
            {
                //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Incrementing {GetType().Name}.NumberOfLogFilesReloading by {incrementValue}. Current value: {NumberOfLogFilesReloading}", LogMessageType.Debug);
                NumberOfLogFilesReloading += incrementValue;
            }
        }

        internal void DecrementNumberOfLogFilesReloading(int decrementValue = 1)
        {
            lock (_logFilesReloadingLock)
            {
                if (NumberOfLogFilesReloading >= decrementValue)
                {
                    //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Decrementing {GetType().Name}.NumberOfLogFilesReloading by {decrementValue}. Current value: {NumberOfLogFilesReloading}", LogMessageType.Debug);
                    NumberOfLogFilesReloading -= decrementValue;
                }
            }
        }

        /// <summary>
        /// Clears the log folder's <see cref="Models.Notification"/>.
        /// </summary>
        public void ClearNotification()
        {
            Notification = null;
        }

        /// <summary>
        /// Cleans up the log folder.
        /// </summary>
        public void Dispose()
        {
            IsActive = false;
            DisposeLogFiles();
            Task.Delay(300).Wait();
        }
        #endregion
    }
}
