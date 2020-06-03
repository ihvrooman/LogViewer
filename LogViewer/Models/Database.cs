using AppStandards.MVVM;
using AppStandards.Logging;
using LogViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogViewer.ViewModels;
using LogViewer.Services;
using AppStandards;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;

namespace LogViewer.Models
{
    public class Database : PropertyChangedHelper, IParentLog, INotification
    {
        #region Fields
        private string _name;
        private volatile bool _keepCurrent;
        private volatile bool _pendingUpdate;
        private volatile bool _hasError;
        private volatile bool _updating;
        private volatile bool _initialized;
        private volatile bool _reloading;
        private volatile bool _disposingOfLogEntries;
        private volatile bool _canBeSelected = false;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private ICommand _reloadDatabaseCommand;
        #endregion

        #region Properties
        /// <summary>
        /// The name of the <see cref="Database"/>.
        /// </summary>
        public string Name { get { return _name; } set { _name = value; RaisePropertyChangedEvent(); } }
        public string Source1 { get; private set; }
        public string Source2 { get; private set; }
        /// <summary>
        /// Indicates whether or not the <see cref="Database"/> should be kept up to date in memory.
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

                    //Update MainWindowViewModel number of selected databases property
                    var update = _keepCurrent ? 1 : -1;
                    _mainWindowViewModel.NumberOfDatabasesSelected += update;

                    //Update, set pending update, or clear log entries
                    if (Initialized && _keepCurrent)
                    {
                        Task.Run(() =>
                        {
                            Task.Delay(2000).Wait();
                            _mainWindowViewModel.RequestReloadOfUILogEntries($"Database \"{Name}\" selected for viewing.");
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
                    RelayCommand.ReEvaluateCanExecute();
                }
            }
        }
        /// <summary>
        /// The log entries read from the error table in the database on the <see cref="Database"/>.
        /// </summary>
        public List<LogEntry> LogEntries = new List<LogEntry>();
        /// <summary>
        /// Indicates whether or not the <see cref="Database"/> is actively being updated.
        /// </summary>
        public bool Updating { get { return _updating; } set { _updating = value; SetCanBeSelectedProperty(); } }
        /// <summary>
        /// Indicates whether or not the <see cref="Database"/> has an error.
        /// </summary>
        public bool HasError { get { return _hasError; } set { _hasError = value; RaisePropertyChangedEvent(); SetCanBeSelectedProperty(); if (_hasError) { KeepCurrent = false; } } }
        /// <summary>
        /// Indicates whether or not the <see cref="Database"/> has been initialized.
        /// </summary>
        public bool Initialized { get { return _initialized; } private set { _initialized = value; SetCanBeSelectedProperty(); } }
        /// <summary>
        /// Indicates whether or not the <see cref="Database"/> is reloading.
        /// </summary>
        public bool Reloading { get { return _reloading; } set { _reloading = value; RaisePropertyChangedEvent(); SetCanBeSelectedProperty(); } }
        /// <summary>
        /// The <see cref="Database"/>s <see cref="Models.Notification"/>.
        /// </summary>
        public Notification Notification { get; private set; }
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
        public ICommand ReloadDatabaseCommand
        {
            get
            {
                if (_reloadDatabaseCommand == null)
                {
                    _reloadDatabaseCommand = new RelayCommand(ConfirmReloadDatabase, CanReload);
                }
                return _reloadDatabaseCommand;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="Database"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Database"/>.</param>
        /// <param name="mainWindowViewModel">A reference to the <see cref="MainWindowViewModel"/>.</param>
        public Database(string name, MainWindowViewModel mainWindowViewModel)
        {
            Name = name;
            Source1 = Name;
            Source2 = "Local_SSI_ErrorLogDetail";
            _mainWindowViewModel = mainWindowViewModel;
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Database \"{Name}\" initialized.");
            Initialized = true;
        }
        #endregion

        #region Public/internal methods
        /// <summary>
        /// Indicates whether or not two <see cref="Database"/>s are equal.
        /// </summary>
        /// <param name="firstDatabase">The first <see cref="Database"/>.</param>
        /// <param name="secondDatabase">The second <see cref="Database"/>.</param>
        /// <returns>A <see cref="bool"/> indicating whether or not the two <see cref="Database"/>s are equal.</returns>
        public static bool AreEqual(Database firstDatabase, Database secondDatabase)
        {
            if (firstDatabase == null && secondDatabase == null)
            {
                return true;
            }

            return firstDatabase.Name == secondDatabase.Name;
        }

        public void PrepareToLoadLogEntriesIntoUI()
        {
            SnapshotNumberOfLogEntries = LogEntries.Count;
            NumberOfLogEntriesLoadedIntoUI = 0;
        }

        /// <summary>
        /// Clear's the <see cref="Database"/>'s <see cref="Models.Notification"/>
        /// </summary>
        public void ClearNotification()
        {
            Notification = null;
        }
        #endregion

        #region Private methods
        private bool CanReload()
        {
            return KeepCurrent || HasError;
        }

        private void ConfirmReloadDatabase()
        {
            if (Messages.ConfirmAction($"Are you sure that you want to reload database \"{Name}\"?{Environment.NewLine}This operation may take several minutes.", AppInfo.BaseAppInfo) == MessageBoxResult.Yes)
            {
                var keepCurrentAfterReload = KeepCurrent;
                KeepCurrent = false;
                Reloading = true;
                ReloadAsync(keepCurrentAfterReload);
            }
        }

        /// <summary>
        /// Reloads the <see cref="Database"/> which entails updating the log entries.
        /// </summary>
        private void ReloadAsync(bool keepCurrentAfterReload)
        {
            Task.Run(() =>
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"User requested a reload of database \"{Name}\".");
                Notification?.Clear();
                HasError = false;
                KeepCurrent = false;
                Task.Delay(500).Wait();
                KeepCurrent = keepCurrentAfterReload;
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Reload of database \"{Name}\" complete.", LogMessageType.Verbose);
                Reloading = false;
            });
        }

        /// <summary>
        /// Updates the <see cref="Database"/>'s in-memory collection of <see cref="LogEntry"/>s.
        /// </summary>
        private void UpdateAsync()
        {
            Task.Run(() =>
            {
                try
                {
                    Updating = true;
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Loading/reloading log entries for database \"{Name}\".", LogMessageType.Verbose);
                    var logWaitMessage = true;
                    while (KeepCurrent && DisposingOfLogEntries)
                    {
                        if (logWaitMessage)
                        {
                            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Waiting for database \"{Name}\" to finish disposing log entries before updating.");
                            logWaitMessage = false;
                        }
                        Task.Delay(100).Wait();
                    }

                    if (!KeepCurrent)
                    {
                        return;
                    }

                    var newLogEntries = DatabaseService.GetDatabaseLogEntries(Name, this);
                    if (newLogEntries.Count > 0)
                    {
                        for (int i = newLogEntries.Count - 1; i >= 0; i--)
                        {
                            var newLogEntry = newLogEntries[i];
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

                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Found {LogEntries.Count} log entries in database \"{Name}\".", LogMessageType.Verbose);

                    if (Initialized && KeepCurrent && _pendingUpdate)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Database \"{Name}\" has a pending update. Clearing pending update and calling Update() method.", LogMessageType.Verbose);
                        _pendingUpdate = false;
                        UpdateAsync();
                    }
                }
                catch (Exception ex)
                {
                    HasError = true;
                    Notification = new Notification(this, $"Could not update database \"{Name}\". Error message: {ex.Message}");
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
            CanBeSelected = !HasError && !Reloading && Initialized;
        }

        private void DisposeLogEntries()
        {
            DisposingOfLogEntries = true;
            try
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Disposing of log entries for database \"{Name}\".", LogMessageType.Verbose);
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
                Messages.ErrorMessage($"An unexpected error occurred and {AppInfo.BaseAppInfo.AppName} must close.", AppInfo.BaseAppInfo, logMessage: $"Disposal of log entries for database \"{Name}\" failed. Error message: {ex.Message}", forceQuit: true);
            }
            DisposingOfLogEntries = false;
        }
        #endregion
    }
}
