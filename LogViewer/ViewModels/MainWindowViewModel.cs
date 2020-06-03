using AppStandards;
using AppStandards.MVVM;
using LogViewer.Models;
using LogViewer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogViewer.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using AppStandards.Logging;
using System.Windows.Input;
using System.Windows.Media.Effects;
using LogViewer.Views;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LogViewer.Properties;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Windows.Data;
using DynamicData;
using System.Reactive.Linq;

namespace LogViewer.ViewModels
{
    public class MainWindowViewModel : BaseViewModel, INotification
    {
        #region Fields
        #region Read only observable collections
        private ReadOnlyObservableCollection<Notification> _notifications;
        private ReadOnlyObservableCollection<LogFolder> _logFolders;
        private ReadOnlyObservableCollection<LogFile> _logFiles;
        private ReadOnlyObservableCollection<Database> _databases;
        private ReadOnlyObservableCollection<LogEntry> _logEntries;
        private ReadOnlyObservableCollection<string> _availableHours;
        private ReadOnlyObservableCollection<string> _availableMinutesAndSeconds;
        private ReadOnlyObservableCollection<string> _availableMilliseconds;
        private ReadOnlyObservableCollection<string> _availableUsernames;
        private ReadOnlyObservableCollection<string> _availableComputernames;
        #endregion
        #region Source lists
        private SourceList<Notification> _notificationsSource = new SourceList<Notification>();
        private SourceList<LogFile> _logFilesSource = new SourceList<LogFile>();
        private SourceList<LogEntry> _logEntriesSource = new SourceList<LogEntry>();
        private SourceList<string> _availableHoursSource = new SourceList<string>();
        private SourceList<string> _availableMinutesAndSecondsSource = new SourceList<string>();
        private SourceList<string> _availableMillisecondsSource = new SourceList<string>();
        private SourceList<string> _availableUsernamesSource = new SourceList<string>();
        private SourceList<string> _availableComputernamesSource = new SourceList<string>();
        #endregion
        #region Filtering
        private bool _includeErrors;
        private bool _includeWarnings;
        private bool _includeInformation;
        private bool _includeVerbose;
        private bool _includeDebug;
        private bool _includeDebugErrors;
        private bool _includeDebugWarnings;
        private bool _includeDebugInformation;
        private bool _includeDebugVerbose;
        private bool _includeUnknown;
        private DateTime _minDay;
        private string _minHour;
        private string _minMinute;
        private string _minSecond;
        private string _minMillisecond;
        private DateTime _maxDay;
        private string _maxHour;
        private string _maxMinute;
        private string _maxSecond;
        private string _maxMillisecond;
        private string _filterUsername;
        private string _filterComputername;
        private string _searchTerm;
        private string _exclusionTerm;
        #endregion
        #region Commands
        private ICommand _settingsCommand;
        private ICommand _aboutCommand;
        private ICommand _clearAllNotificationsCommand;
        private ICommand _resetFiltersCommand;
        private ICommand _clearSearchboxCommand;
        private ICommand _clearExcludeboxCommand;
        private ICommand _selectAllLogFilesCommand;
        private ICommand _deselectAllLogFilesCommand;
        private ICommand _selectAllDatabasesCommand;
        private ICommand _deselectAllDatabasesCommand;
        #endregion
        #region Formatting/animation
        private Effect _windowEffect;
        private bool _animateNotificationPanel;
        private bool _notificationPanelIsExpanded;
        private Brush _searchboxForeground;
        private Brush _excludeboxForeground;
        private volatile int _progressValue;
        private volatile int _minProgress = 0;
        private volatile int _maxProgress;
        #endregion
        private LogEntry _selectedLogEntry;
        private volatile bool _updatingUILogEntries;
        private volatile bool _cancelOperations;
        private volatile bool _initialized;
        private volatile int _numberOfLogFilesSelected;
        private volatile int _numberOfDatabasesSelected;
        private volatile bool _reloadUILogEntries;
        private volatile static bool _disposing;
        private volatile static bool _requestCancelDispose;
        private readonly object _userAndComputerNamesLock = new object();
        #endregion

        #region Properties
        #region Read only observable collections
        public ReadOnlyObservableCollection<LogFolder> LogFolders { get { return _logFolders; } set { _logFolders = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<Database> Databases { get { return _databases; } set { _databases = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<LogFile> LogFiles { get { return _logFiles; } set { _logFiles = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<LogEntry> LogEntries { get { return _logEntries; } set { _logEntries = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<Notification> Notifications { get { return _notifications; } set { _notifications = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<string> AvailableHours { get { return _availableHours; } set { _availableHours = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<string> AvailableMinutesAndSeconds { get { return _availableMinutesAndSeconds; } set { _availableMinutesAndSeconds = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<string> AvailableMilliseconds { get { return _availableMilliseconds; } set { _availableMilliseconds = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<string> AvailableUsernames { get { return _availableUsernames; } set { _availableUsernames = value; RaisePropertyChangedEvent(); } }
        public ReadOnlyObservableCollection<string> AvailableComputernames { get { return _availableComputernames; } set { _availableComputernames = value; RaisePropertyChangedEvent(); } }
        #endregion
        #region Source lists
        public SourceList<LogFolder> LogFoldersSource { get; private set; } = new SourceList<LogFolder>();
        public SourceList<Database> DatabasesSource { get; private set; } = new SourceList<Database>();
        #endregion
        #region Filtering
        public bool IncludeErrors { get { return _includeErrors; } set { _includeErrors = value; RaisePropertyChangedEvent(); } }
        public bool IncludeWarnings { get { return _includeWarnings; } set { _includeWarnings = value; RaisePropertyChangedEvent(); } }
        public bool IncludeInformation { get { return _includeInformation; } set { _includeInformation = value; RaisePropertyChangedEvent(); } }
        public bool IncludeVerbose { get { return _includeVerbose; } set { _includeVerbose = value; RaisePropertyChangedEvent(); } }
        public bool IncludeDebug { get { return _includeDebug; } set { _includeDebug = value; RaisePropertyChangedEvent(); } }
        public bool IncludeDebugErrors { get { return _includeDebugErrors; } set { _includeDebugErrors = value; RaisePropertyChangedEvent(); } }
        public bool IncludeDebugWarnings { get { return _includeDebugWarnings; } set { _includeDebugWarnings = value; RaisePropertyChangedEvent(); } }
        public bool IncludeDebugInformation { get { return _includeDebugInformation; } set { _includeDebugInformation = value; RaisePropertyChangedEvent(); } }
        public bool IncludeDebugVerbose { get { return _includeDebugVerbose; } set { _includeDebugVerbose = value; RaisePropertyChangedEvent(); } }
        public bool IncludeUnknown { get { return _includeUnknown; } set { _includeUnknown = value; RaisePropertyChangedEvent(); } }
        public DateTime MinDay { get { return _minDay; } set { _minDay = value; RaisePropertyChangedEvent(); } }
        public string MinHour { get { return _minHour; } set { _minHour = value; RaisePropertyChangedEvent(); } }
        public string MinMinute { get { return _minMinute; } set { _minMinute = value; RaisePropertyChangedEvent(); } }
        public string MinSecond { get { return _minSecond; } set { _minSecond = value; RaisePropertyChangedEvent(); } }
        public string MinMillisecond { get { return _minMillisecond; } set { _minMillisecond = value; RaisePropertyChangedEvent(); } }
        public DateTime MinDate
        {
            get
            {
                return DateTime.ParseExact($"{MinDay.ToString("MM/dd/yyyy")} {MinHour}:{MinMinute}:{MinSecond}:{MinMillisecond}", "MM/dd/yyyy HH:mm:ss:ffff", CultureInfo.CurrentCulture);
            }
        }
        public DateTime MaxDay { get { return _maxDay; } set { _maxDay = value; RaisePropertyChangedEvent(); } }
        public string MaxHour { get { return _maxHour; } set { _maxHour = value; RaisePropertyChangedEvent(); } }
        public string MaxMinute { get { return _maxMinute; } set { _maxMinute = value; RaisePropertyChangedEvent(); } }
        public string MaxSecond { get { return _maxSecond; } set { _maxSecond = value; RaisePropertyChangedEvent(); } }
        public string MaxMillisecond { get { return _maxMillisecond; } set { _maxMillisecond = value; RaisePropertyChangedEvent(); } }
        public DateTime MaxDate
        {
            get
            {
                return DateTime.ParseExact($"{MaxDay.ToString("MM/dd/yyyy")} {MaxHour}:{MaxMinute}:{MaxSecond}:{MaxMillisecond}", "MM/dd/yyyy HH:mm:ss:ffff", CultureInfo.CurrentCulture);
            }
        }
        public string FilterUsername { get { return _filterUsername; } set { _filterUsername = value; RaisePropertyChangedEvent(); } }
        public string FilterComputername { get { return _filterComputername; } set { _filterComputername = value; RaisePropertyChangedEvent(); } }
        public string SearchTerm
        {
            get
            {
                return _searchTerm;
            }
            set
            {
                if (value == SearchPrompt)
                {
                    SearchboxForeground = Brushes.Gray;
                }
                else
                {
                    SearchboxForeground = Brushes.Black;
                }
                _searchTerm = value;
                RaisePropertyChangedEvent();

            }
        }
        public string ExclusionTerm
        {
            get
            {
                return _exclusionTerm;
            }
            set
            {
                if (value == ExcludePrompt)
                {
                    ExcludeboxForeground = Brushes.Gray;
                }
                else
                {
                    ExcludeboxForeground = Brushes.Black;
                }
                _exclusionTerm = value;
                RaisePropertyChangedEvent();

            }
        }
        #endregion
        #region Formatting/animations
        public Effect WindowEffect { get { return _windowEffect; } set { _windowEffect = value; RaisePropertyChangedEvent(); } }
        public bool AnimateNotificationPanel
        {
            get
            {
                return _animateNotificationPanel;
            }
            set
            {
                if (!value || !NotificationPanelIsExpanded)
                {
                    _animateNotificationPanel = value;
                    RaisePropertyChangedEvent();
                }
            }
        }
        public bool NotificationPanelIsExpanded { get { return _notificationPanelIsExpanded; } set { _notificationPanelIsExpanded = value; RaisePropertyChangedEvent(); if (_notificationPanelIsExpanded) { AnimateNotificationPanel = false; } } }
        public Brush SearchboxForeground { get { return _searchboxForeground; } set { _searchboxForeground = value; RaisePropertyChangedEvent(); } }
        public Brush ExcludeboxForeground { get { return _excludeboxForeground; } set { _excludeboxForeground = value; RaisePropertyChangedEvent(); } }
        public string SearchPrompt { get; } = "Search";
        public string ExcludePrompt { get; } = "Exclude";
        public int ProgressValue { get { return _progressValue; } set { _progressValue = value; RaisePropertyChangedEvent(); } }
        public int MinProgress { get { return _minProgress; } set { _minProgress = value; RaisePropertyChangedEvent(); } }
        public int MaxProgress { get { return _maxProgress; } set { _maxProgress = value; RaisePropertyChangedEvent(); } }
        #endregion
        #region Commands
        public ICommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                {
                    _settingsCommand = new RelayCommand(ShowSettingsDialog);
                }
                return _settingsCommand;
            }
        }
        public ICommand AboutCommand
        {
            get
            {
                if (_aboutCommand == null)
                {
                    _aboutCommand = new RelayCommand(ShowAboutDialog);
                }
                return _aboutCommand;
            }
        }
        public ICommand ClearAllNotificationsCommand
        {
            get
            {
                if (_clearAllNotificationsCommand == null)
                {
                    _clearAllNotificationsCommand = new RelayCommand(ClearAllNotifications, CanClearAllNotifications);
                }
                return _clearAllNotificationsCommand;
            }
        }
        public ICommand ResetFiltersCommand
        {
            get
            {
                if (_resetFiltersCommand == null)
                {
                    _resetFiltersCommand = new RelayCommand(ResetFilters);
                }
                return _resetFiltersCommand;
            }
        }
        public ICommand ClearSearchboxCommand
        {
            get
            {
                if (_clearSearchboxCommand == null)
                {
                    _clearSearchboxCommand = new RelayCommand(ClearSearchbox);
                }
                return _clearSearchboxCommand;
            }
        }
        public ICommand ClearExcludeboxCommand
        {
            get
            {
                if (_clearExcludeboxCommand == null)
                {
                    _clearExcludeboxCommand = new RelayCommand(ClearExcludebox);
                }
                return _clearExcludeboxCommand;
            }
        }
        public ICommand SelectAllLogFilesCommand
        {
            get
            {
                if (_selectAllLogFilesCommand == null)
                {
                    _selectAllLogFilesCommand = new RelayCommand(SelectAllLogFiles, CanSelectOrDeselectAllLogFiles);
                }
                return _selectAllLogFilesCommand;
            }
        }
        public ICommand DeselectAllLogFilesCommand
        {
            get
            {
                if (_deselectAllLogFilesCommand == null)
                {
                    _deselectAllLogFilesCommand = new RelayCommand(DeselectAllLogFiles, CanSelectOrDeselectAllLogFiles);
                }
                return _deselectAllLogFilesCommand;
            }
        }
        public ICommand SelectAllDatabasesCommand
        {
            get
            {
                if (_selectAllDatabasesCommand == null)
                {
                    _selectAllDatabasesCommand = new RelayCommand(SelectAllDatabases, CanSelectOrDeselectAllDatabases);
                }
                return _selectAllDatabasesCommand;
            }
        }
        public ICommand DeselectAllDatabasesCommand
        {
            get
            {
                if (_deselectAllDatabasesCommand == null)
                {
                    _deselectAllDatabasesCommand = new RelayCommand(DeselectAllDatabases, CanSelectOrDeselectAllDatabases);
                }
                return _deselectAllDatabasesCommand;
            }
        }
        #endregion
        public LogEntry SelectedLogEntry { get { return _selectedLogEntry; } set { _selectedLogEntry = value; RaisePropertyChangedEvent(); } }
        public Notification Notification { get; set; }
        public static LogService LogService { get; private set; }
        public bool UpdatingUILogEntries { get { return _updatingUILogEntries; } set { _updatingUILogEntries = value; RaisePropertyChangedEvent(); } }
        public int NumberOfLogFilesSelected { get { return _numberOfLogFilesSelected; } set { _numberOfLogFilesSelected = value; RaisePropertyChangedEvent(); } }
        public int NumberOfDatabasesSelected { get { return _numberOfDatabasesSelected; } set { _numberOfDatabasesSelected = value; RaisePropertyChangedEvent(); } }
        public bool ReloadUILogEntries { get { return _reloadUILogEntries; } set { _reloadUILogEntries = value; } }
        /// <summary>
        /// Indicates whether or not there is a request to cancel the dispose operation.
        /// </summary>
        public static bool RequestCancelDispose { get { return _requestCancelDispose; } set { _requestCancelDispose = value; } }
        #endregion

        #region Constructor
        public MainWindowViewModel(bool inDesignMode = false)
        {
            InitializeBindings();
            Initialize();
        }
        #endregion

        #region Public/Internal Methods
        internal void RequestReloadOfUILogEntries(string reason)
        {
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"{reason} Requesting reload of UI log entries.", LogMessageType.Verbose);
            ReloadUILogEntries = true;
        }

        internal void PrepareSearchboxForUse()
        {
            if (SearchTerm == SearchPrompt)
            {
                SearchTerm = string.Empty;
            }
        }

        internal void ClearSearchbox()
        {
            SearchTerm = SearchPrompt;
        }

        internal void PrepareExcludeboxForUse()
        {
            if (ExclusionTerm == ExcludePrompt)
            {
                ExclusionTerm = string.Empty;
            }
        }

        internal void ClearExcludebox()
        {
            ExclusionTerm = ExcludePrompt;
        }

        internal void ShowNotifications()
        {
            var notificationMessage = string.Empty;
            if (Settings.Default.ShowWelcomeNotification)
            {
                notificationMessage += $"Welcome! This is the notification panel where notifications will be shown. You can clear all of the notifications by pressing the \"Clear All\" button below.{Environment.NewLine + Environment.NewLine}To get started, add log folders and databases by clicking the \"Settings\" menu item. There are some log folders and databases that have been already been added for you. You can remove them or deactivate them if you would like to. Then set your filters and select the log files and databases that you would like to view.";
            }
            if (Settings.Default.LastVersionNumber != AppInfo.BaseAppInfo.VersionNumber)
            {
                if (!string.IsNullOrWhiteSpace(notificationMessage))
                {
                    notificationMessage += Environment.NewLine + Environment.NewLine;
                }
                notificationMessage += $"What's new in version {AppInfo.BaseAppInfo.VersionNumber}?{Environment.NewLine}";
                notificationMessage += $"Performance improvements. Log entries now load significantly faster.";
            }

            if (!string.IsNullOrWhiteSpace(notificationMessage))
            {
                Notification = new Notification(this, notificationMessage);
            }
        }

        public void ClearNotification()
        {
            Notification = null;
            if (Settings.Default.ShowWelcomeNotification)
            {
                Settings.Default.ShowWelcomeNotification = false;
                Settings.Default.Save();
            }
            if (Settings.Default.LastVersionNumber != AppInfo.BaseAppInfo.VersionNumber)
            {
                Settings.Default.LastVersionNumber = AppInfo.BaseAppInfo.VersionNumber;
                Settings.Default.Save();
            }
        }

        internal void Dispose()
        {
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Disposing of {GetType().Name}.", LogMessageType.Verbose);
            _disposing = true;
            _cancelOperations = true;
            foreach (var logFolder in LogFolders)
            {
                logFolder.Dispose();
            }
            foreach (var database in Databases)
            {
                database.KeepCurrent = false;
            }
            NumberOfLogFilesSelected = 0;
            NumberOfDatabasesSelected = 0;
            LogFoldersSource?.Clear();
            _logFilesSource?.Clear();
            DatabasesSource?.Clear();
            _logEntriesSource?.Clear();
            _notificationsSource?.Clear();
            CleanupAppDataFolder();
            _initialized = false;
        }

        /// <summary>
        /// Processes the request to activate the application instance.
        /// </summary>
        internal static void ProcessActivationRequest()
        {
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync("Received request to activate application.", LogMessageType.Verbose);
            if (_disposing)
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Requesting cancellation of {typeof(MainWindowViewModel).Name} dispose operation.", LogMessageType.Verbose);
                RequestCancelDispose = true;
            }
        }

        internal void Initialize(bool inDesignMode = false)
        {
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Initializing {GetType().Name}.", LogMessageType.Verbose);
            _disposing = false;
            _cancelOperations = false;
            RequestCancelDispose = false;
            InitializeAvailableTimes();
            InitializeAvailableUsernamesAndComputernames();
            if (!inDesignMode)
            {
                CreateAppDataFolder();
                Task.Delay(5000).Wait();
                if (LogService == null)
                {
                    LogService = new LogService(this);
                }
                Task.Run(() =>
                {
                    Task.Delay(200).Wait();
                    ResetFilters();
                });
                LogFoldersSource.Edit(innerList =>
                {
                    innerList.Clear();
                    innerList.Add(LogFolderService.GetAllLogFoldersFromSettingsFile(this));
                });
                DatabasesSource.Edit(innerList =>
                {
                    innerList.Clear();
                    innerList.Add(DatabaseService.GetAllDatabasesFromSettingsFile(this));
                });
                UpdateLogFilesAsync();
                UpdateNotificationsAsync();
                UpdateLogEntriesAsync();
            }
            _initialized = true;
        }
        #endregion

        #region Private Methods
        private void InitializeBindings()
        {
            LogFoldersSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _logFolders)
                    .DisposeMany()
                    .Subscribe();
            _logFilesSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _logFiles)
                    .DisposeMany()
                    .Subscribe();
            DatabasesSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _databases)
                    .DisposeMany()
                    .Subscribe();
            _logEntriesSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _logEntries)
                    .DisposeMany()
                    .Subscribe();
            _notificationsSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _notifications)
                    .DisposeMany()
                    .Subscribe();
            _availableUsernamesSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _availableUsernames)
                    .DisposeMany()
                    .Subscribe();
            _availableComputernamesSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _availableComputernames)
                    .DisposeMany()
                    .Subscribe();
            _availableHoursSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _availableHours)
                    .DisposeMany()
                    .Subscribe();
            _availableMinutesAndSecondsSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _availableMinutesAndSeconds)
                    .DisposeMany()
                    .Subscribe();
            _availableMillisecondsSource.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _availableMilliseconds)
                    .DisposeMany()
                    .Subscribe();
        }

        private void InitializeAvailableTimes()
        {
            #region Initialize available hours
            _availableHoursSource.Edit(innerList =>
            {
                innerList.Clear();
                for (int i = 0; i < 10; i++)
                {
                    innerList.Add($"0{i.ToString()}");
                }
                for (int i = 10; i <= 23; i++)
                {
                    innerList.Add(i.ToString());
                }
            });
            #endregion

            #region Initialize available minutes and seconds
            _availableMinutesAndSecondsSource.Edit(innerList =>
            {
                innerList.Clear();
                for (int i = 0; i < 10; i++)
                {
                    innerList.Add($"0{i.ToString()}");
                }
                for (int i = 10; i <= 59; i++)
                {
                    innerList.Add(i.ToString());
                }
            });
            #endregion

            #region Initialize available milliseconds
            _availableMillisecondsSource.Edit(innerList =>
            {
                innerList.Clear();
                for (int i = 0; i < 10; i++)
                {
                    innerList.Add($"000{i.ToString()}");
                }
                for (int i = 10; i < 100; i++)
                {
                    innerList.Add($"00{i.ToString()}");
                }
                for (int i = 100; i < 1000; i++)
                {
                    innerList.Add($"0{i.ToString()}");
                }
                for (int i = 1000; i <= 9999; i++)
                {
                    innerList.Add(i.ToString());
                }
            });
            #endregion
        }

        private void InitializeAvailableUsernamesAndComputernames()
        {
            _availableUsernamesSource.Edit(innerList =>
            {
                innerList.Clear();
                innerList.Add(string.Empty);
            });
            _availableComputernamesSource.Edit(innerList =>
            {
                innerList.Clear();
                innerList.Add(string.Empty);
            });
        }

        private void UpdateLogFilesAsync()
        {
            Task.Run(() =>
            {
                while (!_cancelOperations)
                {
                    //Get new log files
                    var newLogFiles = new List<LogFile>();
                    foreach (var logFolder in LogFolders)
                    {
                        if (logFolder.IsActive)
                        {
                            var numLogFilesToAdd = logFolder.LogFiles.Count;
                            for (int i = 0; i < numLogFilesToAdd; i++)
                            {
                                if (_cancelOperations)
                                {
                                    return;
                                }

                                var logFile = logFolder.LogFiles[i];
                                if (!string.IsNullOrEmpty(logFile.Source2) && !string.IsNullOrEmpty(logFile.CreationDate))
                                {
                                    newLogFiles.Add(logFile);
                                }
                            }
                        }
                    }

                    var removalCount = LogFiles.Count > newLogFiles.Count ? LogFiles.Count - newLogFiles.Count : 0;
                    var breakPoint = newLogFiles.Count >= LogFiles.Count ? LogFiles.Count : newLogFiles.Count;
                    var replaceLogFilesTask = Task.Run(() =>
                    {
                        //This task enumerates through the current log files collection and compares the log files to those in the new log files collection
                        _logFilesSource.Edit(innerList =>
                        {
                            var consecutiveReplaceCount = 0;
                            for (int i = 0; i < breakPoint; i++)
                            {
                                if (_cancelOperations)
                                {
                                    return;
                                }

                                var newLogFile = newLogFiles[i];
                                var existingLogFile = innerList[i];

                                //If we've replaced more than 10 consecutive log files, assume that all the rest will be unequal (which skips the comparison and increases performance)
                                if (consecutiveReplaceCount > 10 || !LogFile.AreEqual(existingLogFile, newLogFile))
                                {
                                    //If the log files are not equal, replace existing log file with new one
                                    innerList[i] = newLogFile;
                                    RelayCommand.ReEvaluateCanExecute();
                                    consecutiveReplaceCount++;
                                }
                                else
                                {
                                    consecutiveReplaceCount = 0;
                                }
                            }
                        });
                    });

                    var addAndRemoveLogFilesTask = Task.Run(() =>
                    {
                        /* This task adds all of the log files that are in the new log files collection which are not 
                         * in the current log files collection to the current log files collection.
                         * It then removes log files from the current log files collection which are in the current log files collection but are not in the new log files collection.
                         */

                        _logFilesSource.Edit(innerList =>
                        {
                            var numberToAdd = newLogFiles.Count - innerList.Count;
                            if (!ReloadUILogEntries && numberToAdd > 0)
                            {
                                if (_cancelOperations)
                                {
                                    return;
                                }
                                innerList.AddRange(newLogFiles.GetRange(innerList.Count, numberToAdd));
                                RelayCommand.ReEvaluateCanExecute();
                            }

                            //Remove extra log files
                            var numberToRemove = innerList.Count - newLogFiles.Count;
                            if (!ReloadUILogEntries && numberToRemove > 0)
                            {
                                if (_cancelOperations)
                                {
                                    return;
                                }
                                innerList.RemoveRange(newLogFiles.Count, numberToRemove);
                                RelayCommand.ReEvaluateCanExecute();
                            }
                        });
                    });

                    //Wait for the replace and add/remove log files tasks to finish
                    Task.WhenAll(replaceLogFilesTask, addAndRemoveLogFilesTask).Wait();
                    Task.Delay(50).Wait();
                }
            });
        }

        private void UpdateLogEntriesAsync()
        {
            Task.Run(() =>
            {
                while (!_cancelOperations)
                {
                    UpdatingUILogEntries = true;

                    if (ReloadUILogEntries)
                    {
                        ReloadUILogEntries = false;
                    }

                    try
                    {
                        //Get new log entries
                        var stopWatch = new Stopwatch();
                        var addCount = 0;
                        var removeCount = 0;
                        var replaceCount = 0;
                        var newLogEntries = new List<LogEntry>();
                        var username = string.Empty;
                        var computername = string.Empty;
                        var allLogFilesUpdated = false;
                        var allDatabasesUpdated = false;
                        var initialNumberOfLogFiles = LogFiles.Count;
                        var initialNumberOfDatabases = Databases.Count;
                        var prepareLogFilesAndDatabases = true;
                        var totalNumberOfNewLogEntries = 0;
                        var showProgressValueOverrunWarning = true;
                        ProgressValue = 0;
                        MaxProgress = initialNumberOfLogFiles;
                        stopWatch.Start();
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Updating UI LogEntries collection.", LogMessageType.Verbose);
                        //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"MaxProgress value: {MaxProgress}", LogMessageType.Debug);

                        while ((!allLogFilesUpdated || !allDatabasesUpdated) && !_cancelOperations && !ReloadUILogEntries)
                        {
                            //Loads the log entries (most recent to oldest) from each log file and database into the newLogEntries collection
                            allLogFilesUpdated = true;
                            allDatabasesUpdated = true;

                            //Loop through log files
                            for (int i = 0; i < initialNumberOfLogFiles; i++)
                            {
                                LogFile logFile = null;
                                var currentNumberOfLogFiles = LogFiles.Count;
                                try
                                {
                                    logFile = LogFiles[i];
                                }
                                catch (Exception ex)
                                {
                                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not get log file from log files collection in UpdateLogEntries() method. Initial count on start of UI collection update: \"{initialNumberOfLogFiles}\" Current count: \"{currentNumberOfLogFiles}\". Error message: {ex.Message}", LogMessageType.Warning);
                                    RequestReloadOfUILogEntries($"Update of UI LogEntries collection failed due to log file access issue.");
                                }

                                if (_cancelOperations)
                                {
                                    return;
                                }

                                var tryCount = 0;
                                while (tryCount < 2 && logFile != null && logFile.DisposingOfLogEntries)
                                {
                                    if (++tryCount < 2)
                                    {
                                        //Give the log file a fraction of a second (to possibly cancel if user unselected then reselected log file) and then recheck DisposingOfLogEntries property
                                        Task.Delay(500).Wait();
                                    }
                                    else
                                    {
                                        //If log file is actively disposing of it's log entries, then reload the UI log entries collection
                                        RequestReloadOfUILogEntries($"Discovered that log file with network path \"{logFile.NetworkFile.FullName}\" is actively disposing of its log entries.");
                                    }
                                }

                                if (ReloadUILogEntries)
                                {
                                    break;
                                }

                                if (prepareLogFilesAndDatabases)
                                {
                                    //If this is the first time enumerating through the log files, prepare the log file for loading of log entries into UI

                                    //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Preparing log file with network path \"{logFile.NetworkFile.FullName}\" for load of log entries into UI.", LogMessageType.Verbose);
                                    logFile.PrepareToLoadLogEntriesIntoUI();
                                    totalNumberOfNewLogEntries += logFile.SnapshotNumberOfLogEntries;
                                    //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Log file with network path \"{logFile.NetworkFile.FullName}\" prepared for load of log entries into UI. Snapshot number of log entries: \"{logFile.SnapshotNumberOfLogEntries}\"", LogMessageType.Verbose);
                                }

                                if (!logFile.AllLogEntriesLoadedIntoUI)
                                {
                                    //If the log file has not had all of its log entries loaded into the UI, load the next log entry
                                    LogEntry logEntry = null;

                                    try
                                    {
                                        logEntry = logFile.LogEntries[logFile.NumberOfLogEntriesLoadedIntoUI];
                                    }
                                    catch (Exception ex)
                                    {
                                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not get log entry from log file with network path \"{logFile.NetworkFile.FullName}\". Error message: {ex.Message}", LogMessageType.Warning);
                                        RequestReloadOfUILogEntries($"Update of UI LogEntries collection failed due to log entry access issue.");
                                    }

                                    if (logEntry != null)
                                    {
                                        //Get the username and computername and add them to the user and computer name filters
                                        AddUsernameOrComputernameAsync(logEntry.Username, logEntry.Computername);

                                        //Add the log entry to the new log entries collection and increment the number of log entries that have been loaded into the UI collection
                                        newLogEntries.Add(logEntry);
                                        logFile.NumberOfLogEntriesLoadedIntoUI++;
                                    }

                                    if (!logFile.AllLogEntriesLoadedIntoUI)
                                    {
                                        //If the log file still has log entries that need to be loaded into the UI, indicate that all the log files are NOT updated
                                        allLogFilesUpdated = false;
                                    }
                                    ProgressValue++;
                                }
                            }

                            //Loop through databases
                            for (int i = 0; i < initialNumberOfDatabases; i++)
                            {
                                Database database = null;
                                var currentNumberOfDatabases = Databases.Count;
                                try
                                {
                                    database = Databases[i];
                                }
                                catch (Exception ex)
                                {
                                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not get database from databases collection in UpdateLogEntries() method. Initial count on start of UI collection update: \"{initialNumberOfDatabases}\" Current count: \"{currentNumberOfDatabases}\". Error message: {ex.Message}", LogMessageType.Warning);
                                    RequestReloadOfUILogEntries($"Update of UI LogEntries collection failed due to database access issue.");
                                }

                                if (_cancelOperations)
                                {
                                    return;
                                }

                                var tryCount = 0;
                                while (tryCount < 2 && database != null && database.DisposingOfLogEntries)
                                {
                                    if (++tryCount < 2)
                                    {
                                        //Give the database a fraction of a second (to possibly cancel if user unselected then reselected log file) and then recheck DisposingOfLogEntries property
                                        Task.Delay(500).Wait();
                                    }
                                    else
                                    {
                                        //If database is actively disposing of it's log entries, then reload the UI log entries collection
                                        RequestReloadOfUILogEntries($"Discovered that database \"{database.Name}\" is actively disposing of its log entries.");
                                    }
                                }

                                if (ReloadUILogEntries)
                                {
                                    break;
                                }

                                if (prepareLogFilesAndDatabases)
                                {
                                    //If this is the first time enumerating through the databases, prepare the database for loading of log entries into UI

                                    //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Preparing database \"{database.Name}\" for load of log entries into UI.", LogMessageType.Verbose);
                                    database.PrepareToLoadLogEntriesIntoUI();
                                    totalNumberOfNewLogEntries += database.SnapshotNumberOfLogEntries;
                                    //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Database \"{database.Name}\" prepared for load of log entries into UI. Snapshot number of log entries: \"{database.SnapshotNumberOfLogEntries}\"", LogMessageType.Verbose);
                                }

                                if (!database.AllLogEntriesLoadedIntoUI)
                                {
                                    //If the database has not had all of its log entries loaded into the UI, load the next log entry
                                    LogEntry logEntry = null;

                                    try
                                    {
                                        logEntry = database.LogEntries[database.NumberOfLogEntriesLoadedIntoUI];
                                    }
                                    catch (Exception ex)
                                    {
                                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not get log entry from database \"{database.Name}\". Error message: {ex.Message}", LogMessageType.Warning);
                                        RequestReloadOfUILogEntries($"Update of UI LogEntries collection failed due to log entry access issue.");
                                    }

                                    if (logEntry != null)
                                    {
                                        //Get the username and computername and add them to the user and computer name filters
                                        AddUsernameOrComputernameAsync(logEntry.Username, logEntry.Computername);

                                        //Add the log entry to the new log entries collection and increment the number of log entries that have been loaded into the UI collection
                                        newLogEntries.Add(logEntry);
                                        database.NumberOfLogEntriesLoadedIntoUI++;
                                    }

                                    if (!database.AllLogEntriesLoadedIntoUI)
                                    {
                                        //If the log file still has log entries that need to be loaded into the UI, indicate that all the log files are NOT updated
                                        allDatabasesUpdated = false;
                                    }
                                    ProgressValue++;
                                }
                            }

                            if (prepareLogFilesAndDatabases)
                            {
                                MaxProgress = totalNumberOfNewLogEntries > 0 ? totalNumberOfNewLogEntries - ProgressValue : 0;
                                ProgressValue = 0;
                                //AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Log files and databases prepared. New MaxProgress value: {MaxProgress}", LogMessageType.Debug);
                            }
                            prepareLogFilesAndDatabases = false;

                            if (showProgressValueOverrunWarning && ProgressValue > MaxProgress)
                            {
                                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Progress value is greater than max progress. Progress value: {ProgressValue} Max progress: {MaxProgress}", LogMessageType.Warning);
                                showProgressValueOverrunWarning = false;
                            }
                        }
                        ProgressValue = MaxProgress;

                        var removalCount = _logEntriesSource.Count > newLogEntries.Count ? _logEntriesSource.Count - newLogEntries.Count : 0;
                        MaxProgress = newLogEntries.Count + removalCount;
                        ProgressValue = 0;
                        var breakPoint = newLogEntries.Count >= _logEntriesSource.Count ? _logEntriesSource.Count : newLogEntries.Count;

                        var replaceLogEntriesTask = Task.Run(() =>
                        {
                            //This task enumerates through the current log entries collection and compares the log entries to those in the new log entries collection
                            _logEntriesSource.Edit(innerList =>
                            {
                                var consecutiveReplaceCount = 0;
                                for (int i = 0; i < breakPoint; i++)
                                {
                                    if (_cancelOperations)
                                    {
                                        return;
                                    }

                                    if (ReloadUILogEntries)
                                    {
                                        break;
                                    }

                                    var newLogEntry = newLogEntries[i];
                                    var existingLogEntry = innerList[i];

                                    //If we've replaced more than 10 consecutive log entries, assume that all the rest will be unequal (which skips the comparison and increases performance)
                                    if (consecutiveReplaceCount > 10 || existingLogEntry.ParentLog == null || !LogEntry.AreEqual(existingLogEntry, newLogEntry))
                                    {
                                        //If the log entries are not equal, replace existing log entry with new one
                                        innerList[i] = newLogEntry;
                                        ProgressValue++;
                                        replaceCount++;
                                        consecutiveReplaceCount++;
                                    }
                                    else
                                    {
                                        consecutiveReplaceCount = 0;
                                    }
                                }
                            });
                        });

                        var addOrRemoveLogEntriesTask = Task.Run(() =>
                        {
                            /* This task adds all of the log entries that are in the new log entries collection 
                             * which are not in the current log entries collection to the current log entries collection.
                             * It then removes the log entries from the current log entries collection which are in the 
                             * current log entries collection but are not in the new log entries collection.
                             * 
                             * Note: Since the new log entries collection is EITHER the same size as, longer than, or 
                             * shorter than the UI log entries collection, we will only be adding OR removing log entries,
                             * not both.
                             */

                            _logEntriesSource.Edit(innerList =>
                            {
                                var numberToAdd = newLogEntries.Count - innerList.Count;
                                if (!ReloadUILogEntries && numberToAdd > 0)
                                {
                                    if (_cancelOperations)
                                    {
                                        return;
                                    }
                                    innerList.AddRange(newLogEntries.GetRange(innerList.Count, numberToAdd));
                                    ProgressValue += numberToAdd;
                                    addCount += numberToAdd;
                                }

                                //Remove extra log entries
                                var numberToRemove = innerList.Count - newLogEntries.Count;
                                if (!ReloadUILogEntries && numberToRemove > 0)
                                {
                                    if (_cancelOperations)
                                    {
                                        return;
                                    }

                                    try
                                    {
                                        innerList.RemoveRange(newLogEntries.Count, numberToRemove);
                                    }
                                    catch (Exception ex)
                                    {
                                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not remove log entry from UI log entries collection. Error message: {ex.Message}", LogMessageType.Warning);
                                        RequestReloadOfUILogEntries($"Update of UI LogEntries collection failed due to log entry removal issue.");
                                    }
                                    ProgressValue += numberToRemove;
                                    removeCount += numberToRemove;
                                }
                            });
                        });

                        //Small delay to let progress bar update
                        Task.Delay(50).Wait();

                        //Wait for the replace and add/remove log entries tasks to finish
                        Task.WhenAll(replaceLogEntriesTask, addOrRemoveLogEntriesTask).Wait();
                        ProgressValue = MaxProgress;

                        stopWatch.Stop();
                        var messagePartTwo = $" {addCount} log entries added, {removeCount} removed, and {replaceCount} replaced in {stopWatch.Elapsed.Minutes} m {stopWatch.Elapsed.Seconds} s {stopWatch.Elapsed.Milliseconds} ms.";
                        if (!ReloadUILogEntries)
                        {
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Finished updating UI LogEntries collection.{messagePartTwo}", LogMessageType.Verbose);
                        }
                        else
                        {
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Reload of UI LogEntries collection requested due to log file selections or change in log file initialization states.{messagePartTwo}", LogMessageType.Verbose);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"An error occured while trying to update the UI collection of log entries. Error message: {ex.Message} | Stack trace: {Environment.StackTrace}", LogMessageType.Warning);
                    }
                    UpdatingUILogEntries = false;

                    var delayTime = ReloadUILogEntries ? 0 : 1000;
                    Task.Delay(delayTime).Wait();
                }
            });
        }

        private void UpdateNotificationsAsync()
        {
            Task.Run(() =>
            {
                while (!_cancelOperations)
                {
                    var newNotifications = new List<Notification>();
                    if (Notification != null)
                    {
                        newNotifications.Add(Notification);
                    }

                    //Loop through log folders
                    var initialLogFolderCount = LogFolders.Count;
                    for (int i = 0; i < initialLogFolderCount; i++)
                    {
                        var logFolder = LogFolders[i];
                        if (_cancelOperations)
                        {
                            return;
                        }

                        if (logFolder.HasError && logFolder.Notification != null)
                        {
                            newNotifications.Add(logFolder.Notification);
                        }

                        foreach (var logFile in logFolder.LogFiles)
                        {
                            if (_cancelOperations)
                            {
                                return;
                            }

                            if (logFile.HasError && logFile.Notification != null)
                            {
                                newNotifications.Add(logFile.Notification);
                            }
                        }
                    }

                    //Loop through databases
                    var initialDatabaseCount = Databases.Count;
                    for (int i = 0; i < initialDatabaseCount; i++)
                    {
                        var database = Databases[i];
                        if (_cancelOperations)
                        {
                            return;
                        }

                        if (database.HasError && database.Notification != null)
                        {
                            newNotifications.Add(database.Notification);
                        }
                    }

                    var removalCount = _notificationsSource.Count > newNotifications.Count ? _logEntriesSource.Count - newNotifications.Count : 0;
                    var breakPoint = newNotifications.Count >= _notificationsSource.Count ? _notificationsSource.Count : newNotifications.Count;

                    var replaceNotificationsTask = Task.Run(() =>
                    {
                        //This task enumerates through the current notifications collection and compares the notifications to those in the new notifications collection
                        _notificationsSource.Edit(innerList =>
                        {
                            var consecutiveReplaceCount = 0;
                            for (int i = 0; i < breakPoint; i++)
                            {
                                if (_cancelOperations)
                                {
                                    return;
                                }

                                if (ReloadUILogEntries)
                                {
                                    break;
                                }

                                var newNotification = newNotifications[i];
                                var existingNotification = innerList[i];

                                //If we've replaced more than 10 consecutive notifications, assume that all the rest will be unequal (which skips the comparison and increases performance)
                                if (consecutiveReplaceCount > 10 || !string.Equals(existingNotification.Message, newNotification.Message, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    //If the notifications are not equal, replace existing notification with new one
                                    innerList[i] = newNotification;
                                    AnimateNotificationPanel = true;
                                    consecutiveReplaceCount++;
                                }
                                else
                                {
                                    consecutiveReplaceCount = 0;
                                }
                            }
                        });
                    });

                    var addOrRemoveNotificationsTask = Task.Run(() =>
                    {
                        /* This task adds all of the notifications that are in the new notifications collection 
                         * which are not in the current notifications collection to the current notifications collection.
                         * It then removes the notifications from the current notifications collection which are in the 
                         * current notifications collection but are not in the new notifications collection.
                         * 
                         * Note: Since the new notifications collection is EITHER the same size as, longer than, or 
                         * shorter than the UI notifications collection, we will only be adding OR removing notifications,
                         * not both.
                         */

                        _notificationsSource.Edit(innerList =>
                        {
                            var numberToAdd = newNotifications.Count - innerList.Count;
                            if (!ReloadUILogEntries && numberToAdd > 0)
                            {
                                if (_cancelOperations)
                                {
                                    return;
                                }
                                innerList.AddRange(newNotifications.GetRange(innerList.Count, numberToAdd));
                                AnimateNotificationPanel = true;
                            }

                            //Remove extra notifications
                            var numberToRemove = innerList.Count - newNotifications.Count;
                            if (!ReloadUILogEntries && numberToRemove > 0)
                            {
                                if (_cancelOperations)
                                {
                                    return;
                                }

                                innerList.RemoveRange(newNotifications.Count, numberToRemove);
                            }
                        });
                    });

                    //Wait for the replace and add/remove log entries tasks to finish
                    Task.WhenAll(replaceNotificationsTask, addOrRemoveNotificationsTask).Wait();

                    Task.Delay(50).Wait();
                }
            });
        }

        private void ShowSettingsDialog()
        {
            WindowEffect = AppInfo.WindowBlurEffect;
            new SettingsView(this).ShowDialog();
            WindowEffect = null;
        }

        private void ShowAboutDialog()
        {
            WindowEffect = AppInfo.WindowBlurEffect;
            MessageBox.Show(AppInfo.AboutInformation, "About", MessageBoxButton.OK);
            WindowEffect = null;
        }

        private bool CanClearAllNotifications()
        {
            return Notifications?.Count > 0;
        }

        private void ClearAllNotifications()
        {
            foreach (var notification in Notifications)
            {
                notification.Clear();
            }
            NotificationPanelIsExpanded = false;
        }

        private void ResetFilters()
        {
            try
            {
                MinDay = DateTime.Now.AddDays(-7);
                MaxDay = DateTime.Now;
                MinHour = AvailableHours[0];
                MaxHour = AvailableHours[AvailableHours.Count - 1];
                MinSecond = AvailableMinutesAndSeconds[0];
                MinMinute = AvailableMinutesAndSeconds[0];
                MaxSecond = AvailableMinutesAndSeconds[AvailableMinutesAndSeconds.Count - 1];
                MaxMinute = AvailableMinutesAndSeconds[AvailableMinutesAndSeconds.Count - 1];
                MinMillisecond = AvailableMilliseconds[0];
                MaxMillisecond = AvailableMilliseconds[AvailableMilliseconds.Count - 1];

                IncludeErrors = true;
                IncludeWarnings = true;
                IncludeInformation = true;
                IncludeVerbose = false;
                IncludeDebug = false;
                IncludeDebugErrors = false;
                IncludeDebugWarnings = false;
                IncludeDebugInformation = false;
                IncludeDebugVerbose = false;
                IncludeUnknown = true;

                FilterUsername = AvailableUsernames[0];
                FilterComputername = AvailableComputernames[0];

                ClearSearchbox();
                ClearExcludebox();
            }
            catch (Exception ex)
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Failed to reset filters. Error message: {ex.Message}", LogMessageType.Error);
            }
        }

        private void CreateAppDataFolder()
        {
            //create app data folder
            var localLogFileCopiesFolder = new DirectoryInfo(AppInfo.LocalLogFileFolderPath);
            localLogFileCopiesFolder?.Refresh();
            if (!localLogFileCopiesFolder.Exists)
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Creating local log file folder with path \"{localLogFileCopiesFolder.FullName}\".");
                localLogFileCopiesFolder.Create();
            }
        }

        private void CleanupAppDataFolder()
        {
            var localLogFilesFolder = new DirectoryInfo(AppInfo.LocalLogFileFolderPath);
            var stopWatch = new Stopwatch();
            localLogFilesFolder?.Refresh();
            if (localLogFilesFolder.Exists && !RequestCancelDispose)
            {
                try
                {
                    stopWatch.Start();
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Cleaning up local log file copies from AppData folder with path \"{localLogFilesFolder.FullName}\".", LogMessageType.Information);
                    var localLogFiles = localLogFilesFolder.GetFiles().ToList();
                    var isThereAnotherInstanceRunning = IsThereAnotherInstanceRunning();
                    var timeoutMinutes = 10;
                    var operationHasTimedOut = stopWatch.Elapsed.Minutes >= timeoutMinutes;
                    var failedDeleteList = new List<string>();
                    while (localLogFiles.Count > 0 && !operationHasTimedOut && !isThereAnotherInstanceRunning && !RequestCancelDispose)
                    {
                        foreach (var localLogFile in localLogFiles)
                        {
                            isThereAnotherInstanceRunning = IsThereAnotherInstanceRunning();
                            operationHasTimedOut = stopWatch.Elapsed.Minutes >= timeoutMinutes;
                            if (isThereAnotherInstanceRunning || operationHasTimedOut || RequestCancelDispose)
                            {
                                break;
                            }

                            try
                            {
                                if (!failedDeleteList.Contains(localLogFile.Name))
                                {
                                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Attempting to delete local log file \"{localLogFile.Name}\" from AppData folder with path \"{localLogFilesFolder.FullName}\".", LogMessageType.Verbose);
                                    localLogFile.Delete();
                                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Deleted local log file \"{localLogFile.Name}\" from AppData folder with path \"{localLogFilesFolder.FullName}\".", LogMessageType.Verbose);
                                }
                            }
                            catch (Exception ex)
                            {
                                failedDeleteList.Add(localLogFile.Name);
                                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not delete local log file \"{localLogFile.Name}\" from AppData folder with path \"{localLogFilesFolder.FullName}\". Error message: {ex.Message}", LogMessageType.Warning);
                            }
                        }
                        localLogFiles = localLogFilesFolder.GetFiles().ToList();
                        localLogFiles.RemoveAll(llf => failedDeleteList.Contains(llf.Name));
                    }

                    stopWatch.Stop();
                    var timeString = $"{stopWatch.Elapsed.Minutes} m {stopWatch.Elapsed.Seconds} s {stopWatch.Elapsed.Milliseconds} ms";
                    localLogFiles = localLogFilesFolder.GetFiles().ToList();
                    if (localLogFiles.Count > 0)
                    {
                        var operationTimeoutReason = $" Operation timed out after {timeString}.";
                        var instanceStartedReason = $" Another instance of the current application has been detected.";
                        var requestCancelDisposeReason = $" A request was made to cancel the {GetType().Name} dispose operation.";
                        var reason = string.Empty;
                        if (operationHasTimedOut)
                        {
                            reason += operationTimeoutReason;
                        }
                        if (isThereAnotherInstanceRunning)
                        {
                            reason += instanceStartedReason;
                        }
                        if (RequestCancelDispose)
                        {
                            reason += requestCancelDisposeReason;
                        }

                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not completely clean up local log file copies from AppData folder with path \"{localLogFilesFolder.FullName}\".{reason}", LogMessageType.Information);
                    }
                    else
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Clean up of local log file copies from AppData folder with path \"{localLogFilesFolder.FullName}\" completed after {timeString}.", LogMessageType.Information);
                    }
                }
                catch (Exception ex)
                {
                    stopWatch.Stop();
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not clean up local log file copies from AppData folder with path \"{localLogFilesFolder.FullName}\". Error message: {ex.Message}", LogMessageType.Error);
                }
            }
        }

        private bool IsThereAnotherInstanceRunning()
        {
            var instanceCount = 0;
            var currentProcessName = Process.GetCurrentProcess().ProcessName;
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == currentProcessName)
                {
                    instanceCount++;
                    if (instanceCount > 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CanSelectOrDeselectAllLogFiles()
        {
            return LogFiles.Count > 0;
        }

        private void SelectAllLogFiles()
        {
            foreach (var logFile in LogFiles)
            {
                if (logFile.CanBeSelected)
                {
                    logFile.KeepCurrent = true;
                }
            }
            RequestReloadOfUILogEntries("User selected all log files.");
        }

        private void DeselectAllLogFiles()
        {
            foreach (var logFile in LogFiles)
            {
                if (logFile.CanBeSelected)
                {
                    logFile.KeepCurrent = false;
                }
            }
            RequestReloadOfUILogEntries("User deselected all log files.");
        }

        private bool CanSelectOrDeselectAllDatabases()
        {
            return Databases.Count > 0;
        }

        private void SelectAllDatabases()
        {
            foreach (var database in Databases)
            {
                if (database.CanBeSelected)
                {
                    database.KeepCurrent = true;
                }
            }
            RequestReloadOfUILogEntries("User selected all databases.");
        }

        private void DeselectAllDatabases()
        {
            foreach (var database in Databases)
            {
                if (database.CanBeSelected)
                {
                    database.KeepCurrent = false;
                }
            }
            RequestReloadOfUILogEntries("User deselected all databases.");
        }

        private void AddUsernameOrComputernameAsync(string username, string computername)
        {
            Task.Run(() =>
            {
                lock (_userAndComputerNamesLock)
                {
                    /* Add the username and computername to the collection of available user or computer names
                     * if it's not already in the collection.
                     * Note:
                     * Since the user or computer name won't be added to the UI collection immediately (because the priority
                     * is background), we need to keep a copy of the collection which is used to determine whether or not the
                     * given user or computer name has already been added to the collection. If we simply check the UI collection, it may be added more than once.
                     */

                    if (!_availableUsernamesSource.Items.Contains(username))
                    {
                        _availableUsernamesSource.Add(username);
                    }
                    if (!_availableComputernamesSource.Items.Contains(computername))
                    {
                        _availableComputernamesSource.Add(computername);
                    }
                }
            });
        }
        #endregion

        //TODO: Make sure there all references to readonlyobservablecollections get changed to references to the sourceLists.
    }
}
