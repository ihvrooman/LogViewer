using AppStandards;
using AppStandards.MVVM;
using DynamicData;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using LogViewer.Services;
using LogViewer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace LogViewer.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Fields
        private int _selectedLogFolderIndex;
        private ICommand _addLogFolderCommand;
        private ICommand _removeLogFolderCommand;
        private int _selectedDatabaseIndex;
        private ICommand _addDatabaseCommand;
        private ICommand _removeDatabaseCommand;
        private ICommand _closeCommand;
        private ICommand _updateLogFolderIsActivePropertyCommand;
        private Effect _windowEffect;
        private bool _disposed;
        #endregion

        #region Properties
        public MainWindowViewModel MainWindowViewModel { get; set; }
        #region Log folder properties
        public int SelectedLogFolderIndex { get { return _selectedLogFolderIndex; } set { _selectedLogFolderIndex = value; RaisePropertyChangedEvent(); } }
        public LogFolder SelectedLogFolder
        {
            get
            {
                if (MainWindowViewModel == null)
                {
                    return null;
                }
                if (SelectedLogFolderIndex >= 0 && SelectedLogFolderIndex < MainWindowViewModel.LogFolders.Count)
                {
                    return MainWindowViewModel.LogFolders[SelectedLogFolderIndex];
                }
                return null;
            }
        }
        public ICommand AddLogFolderCommand
        {
            get
            {
                if (_addLogFolderCommand == null)
                {
                    _addLogFolderCommand = new RelayCommand(ShowAddLogFolderDialog);
                }
                return _addLogFolderCommand;
            }
        }
        public ICommand RemoveLogFolderCommand
        {
            get
            {
                if (_removeLogFolderCommand == null)
                {
                    _removeLogFolderCommand = new RelayCommand(RemoveLogFolder, CanRemoveLogFolder);
                }
                return _removeLogFolderCommand;
            }
        }
        #endregion
        #region Database properties
        public int SelectedDatabaseIndex { get { return _selectedDatabaseIndex; } set { _selectedDatabaseIndex = value; RaisePropertyChangedEvent(); } }
        public Database SelectedDatabase
        {
            get
            {
                if (MainWindowViewModel == null)
                {
                    return null;
                }
                if (SelectedDatabaseIndex >= 0 && SelectedDatabaseIndex < MainWindowViewModel.Databases.Count)
                {
                    return MainWindowViewModel.Databases[SelectedDatabaseIndex];
                }
                return null;
            }
        }
        public ICommand AddDatabaseCommand
        {
            get
            {
                if (_addDatabaseCommand == null)
                {
                    _addDatabaseCommand = new RelayCommand(ShowAddDatabaseDialog);
                }
                return _addDatabaseCommand;
            }
        }
        public ICommand RemoveDatabaseCommand
        {
            get
            {
                if (_removeDatabaseCommand == null)
                {
                    _removeDatabaseCommand = new RelayCommand(RemoveDatabase, CanRemoveDatabase);
                }
                return _removeDatabaseCommand;
            }
        }
        #endregion
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand<SettingsView>(Close);
                }
                return _closeCommand;
            }
        }
        public ICommand UpdateLogFolderIsActivePropertyCommand
        {
            get
            {
                if (_updateLogFolderIsActivePropertyCommand == null)
                {
                    _updateLogFolderIsActivePropertyCommand = new RelayCommand<LogFolder>(UpdateLogFolderIsActiveProperty);
                }
                return _updateLogFolderIsActivePropertyCommand;
            }
        }
        public Effect WindowEffect { get { return _windowEffect; } set { _windowEffect = value; RaisePropertyChangedEvent(); } }
        #endregion

        #region Constructor
        public SettingsViewModel(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
        }
        #endregion

        #region Private Methods
        #region Log folder methods
        private void ShowAddLogFolderDialog()
        {
            WindowEffect = AppInfo.WindowBlurEffect;
            var logFolderView = new AddLogFolderView();
            if (logFolderView.ShowDialog() == true)
            {
                MainWindowViewModel.LogFoldersSource.Add(new LogFolder(logFolderView.ViewModel.LogFolderPath, MainWindowViewModel));
            }
            WindowEffect = null;
        }

        private bool CanRemoveLogFolder()
        {
            return SelectedLogFolder != null;
        }

        private void RemoveLogFolder()
        {
            if (SelectedLogFolder != null && Messages.ConfirmAction($"Are you sure that you want to remove log folder with path \"{SelectedLogFolder.Folder.FullName}\"?", AppInfo.BaseAppInfo) == MessageBoxResult.Yes)
            {
                LogFolderService.RemoveLogFolderPathFromSettingsFileAsync(SelectedLogFolder.Folder.FullName);
                MainWindowViewModel.LogFoldersSource.Remove(SelectedLogFolder);
            }
        }
        #endregion

        #region Database methods
        private void ShowAddDatabaseDialog()
        {
            WindowEffect = AppInfo.WindowBlurEffect;
            var databaseView = new AddDatabaseView();
            if (databaseView.ShowDialog() == true)
            {
                MainWindowViewModel.DatabasesSource.Add(new Database(databaseView.ViewModel.DatabaseName, MainWindowViewModel));
            }
            WindowEffect = null;
        }

        private bool CanRemoveDatabase()
        {
            return SelectedDatabase != null;
        }

        private void RemoveDatabase()
        {
            if (SelectedDatabase != null && Messages.ConfirmAction($"Are you sure that you want to remove database \"{SelectedDatabase.Name}\"?", AppInfo.BaseAppInfo) == MessageBoxResult.Yes)
            {
                DatabaseService.RemoveDatabaseNameFromSettingsFileAsync(SelectedDatabase.Name);
                MainWindowViewModel.DatabasesSource.Remove(SelectedDatabase);
            }
        }
        #endregion

        private void Close(SettingsView settingsWindow)
        {
            settingsWindow.Close();
        }

        private void UpdateLogFolderIsActiveProperty(LogFolder logFolder)
        {
            if (!_disposed)
            {
                LogFolderService.UpdateLogFolderIsActiveSettingAsync(logFolder.Folder.FullName, logFolder.IsActive);
            }
        }
        #endregion

        #region Public/Internal methdods
        /// <summary>
        /// Cleans up the <see cref="SettingsViewModel"/>.
        /// </summary>
        internal void Dispose()
        {
            _disposed = true;
            MainWindowViewModel = null;
        }
        #endregion
    }
}
