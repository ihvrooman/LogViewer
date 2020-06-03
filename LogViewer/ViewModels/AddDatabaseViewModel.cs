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
    public class AddDatabaseViewModel : BaseViewModel
    {
        #region Fields
        private string _databaseName;
        private ICommand _addDatabaseCommand;
        private ICommand _cancelCommand;
        #endregion

        #region Properties
        public string DatabaseName { get { return _databaseName; } set { _databaseName = value; RaisePropertyChangedEvent(); } }
        public ICommand AddDatabaseCommand
        {
            get
            {
                if (_addDatabaseCommand == null)
                {
                    _addDatabaseCommand = new RelayCommand<AddDatabaseView>(AddDatabase);
                }
                return _addDatabaseCommand;
            }
        }
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand<AddDatabaseView>(Cancel);
                }
                return _cancelCommand;
            }
        }
        #endregion

        #region Private Methods
        private void AddDatabase(AddDatabaseView addDatabaseWindow)
        {
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                Messages.InfoMessage($"Database name cannot be null or whitespace.", AppInfo.BaseAppInfo);
                return;
            }

            if (!new DirectoryInfo($"\\\\{DatabaseName}\\c$").Exists && Messages.ConfirmAction($"The database \"{DatabaseName}\" could not be found.{Environment.NewLine}Do you still want to add the database?", AppInfo.BaseAppInfo) == MessageBoxResult.No)
            {
                return;
            }

            DatabaseService.AddDatabaseNameToSettingsFileAsync(DatabaseName);
            addDatabaseWindow.DialogResult = true;
        }

        private void Cancel(AddDatabaseView addDatabaseWindow)
        {
            addDatabaseWindow.DialogResult = false;
        }
        #endregion
    }
}
