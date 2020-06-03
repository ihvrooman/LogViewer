using AppStandards;
using AppStandards.MVVM;
using LogViewer.Helpers;
using LogViewer.Properties;
using LogViewer.Services;
using LogViewer.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LogViewer.ViewModels
{
    public class AddLogFolderViewModel : BaseViewModel
    {
        #region Fields
        private string _logFolderPath;
        private ICommand _addLogFolderCommand;
        private ICommand _cancelCommand;
        #endregion

        #region Properties
        public string LogFolderPath { get { return _logFolderPath; } set { _logFolderPath = value; RaisePropertyChangedEvent(); } }
        public ICommand AddLogFolderCommand
        {
            get
            {
                if (_addLogFolderCommand == null)
                {
                    _addLogFolderCommand = new RelayCommand<AddLogFolderView>(AddLogFolder);
                }
                return _addLogFolderCommand;
            }
        }
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand<AddLogFolderView>(Cancel);
                }
                return _cancelCommand;
            }
        }
        #endregion

        #region Private Methods
        private void AddLogFolder(AddLogFolderView addLogFolderWindow)
        {
            if (string.IsNullOrWhiteSpace(LogFolderPath))
            {
                Messages.InfoMessage($"Log folder path cannot be null or whitespace.", AppInfo.BaseAppInfo);
                return;
            }
            
            try
            {
                new DirectoryInfo(LogFolderPath);
            }
            catch
            {
                Messages.InfoMessage($"Log folder path must be a valid directory path.", AppInfo.BaseAppInfo);
                return;
            }

            if (!new DirectoryInfo(LogFolderPath).Exists && Messages.ConfirmAction($"The log folder with path \"{LogFolderPath}\" could not be found.{Environment.NewLine}Do you still want to add the log folder?", AppInfo.BaseAppInfo) == MessageBoxResult.No)
            {
                return;
            }

            LogFolderService.AddLogFolderPathToSettingsFileAsync(LogFolderPath);
            addLogFolderWindow.DialogResult = true;
        }

        private void Cancel(AddLogFolderView addLogFolderWindow)
        {
            addLogFolderWindow.DialogResult = false;
        }
        #endregion
    }
}
