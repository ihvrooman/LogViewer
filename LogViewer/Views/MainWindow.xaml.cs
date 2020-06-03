using AppStandards;
using AppStandards.Helpers;
using AppStandards.Logging;
using LogViewer.Helpers;
using LogViewer.Properties;
using LogViewer.Services;
using LogViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private volatile bool _initialized;
        private CollectionView _logEntriesView;
        #endregion

        #region Properties
        public MainWindowViewModel ViewModel
        {
            get
            {
                return (MainWindowViewModel)DataContext;
            }
        }
        #endregion

        #region Constructor
        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            LogFolderService.UpgradeLogFolderSettings();
            LogFolderService.RemoveInvalidLogFolderPathsFromSettingsFile();
            LogFolderService.AddOfflineLogFolderPath();
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            ViewModel.ShowNotifications();
        }
        #endregion

        #region Private methods
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            var errorMessage = $"An unhandled exception occurred. Error message: {ex.Message} Exception stack trace: {ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $" | Inner exception message: {ex.InnerException.Message} Inner exception stack trace: {ex.InnerException.StackTrace}";
            }
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync(errorMessage, LogMessageType.Error);

            //Wait for message queue to be flushed
            Task.Delay(31).Wait();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.SetPlacement(Settings.Default.MainWindowPlacement);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveWindowPlacement();
            _initialized = false;
            Hide();
            ViewModel.Dispose();
            Task.Delay(1000).Wait();
            if (MainWindowViewModel.RequestCancelDispose)
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Application activation requested. Cancelling shutdown and re-initializing components.");
                e.Cancel = true;
                Show();
                ViewModel.Initialize();
                ViewModel.ShowNotifications();
                _initialized = true;
            }
            else
            {
                Routines.Shutdown(AppInfo.BaseAppInfo);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _logEntriesView = (CollectionView)CollectionViewSource.GetDefaultView(LogEntriesListView.ItemsSource);
            _logEntriesView.SortDescriptions.Add(new SortDescription("TimeStamp", ListSortDirection.Descending));
            _logEntriesView.Filter = MainWindowViewModel.LogService.IncludeLogEntry;
            _initialized = true;
        }

        private void SaveWindowPlacement()
        {
            try
            {
                if (_initialized)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Saving main window placement.", LogMessageType.Verbose);
                    Settings.Default.MainWindowPlacement = this.GetPlacement();
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Failed to save main window placement. Error message: {ex.Message}", LogMessageType.Warning);
            }
        }

        private void SearchTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.PrepareSearchboxForUse();
        }

        private void SearchTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextbox.Text))
            {
                ViewModel.ClearSearchbox();
            }
        }

        private void ExcludeTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.PrepareExcludeboxForUse();
        }

        private void ExcludeTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ExcludeTextbox.Text))
            {
                ViewModel.ClearExcludebox();
            }
        }

        private void RefreshLogEntriesView()
        {
            if (_initialized)
            {
                if (_logEntriesView == null)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not refresh log entries view becuase it was null when it was expected to have a value.", LogMessageType.Error);
                    return;
                }
                _logEntriesView.Refresh();
            }
        }

        private void DateOrUserInfoFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshLogEntriesView();
        }

        private void LogTypeFilter_Changed(object sender, RoutedEventArgs e)
        {
            RefreshLogEntriesView();
        }

        private void SearchOrExcludeFilter_Changed(object sender, TextChangedEventArgs e)
        {
            RefreshLogEntriesView();
        }

        private void LogFileOrDatabase_KeepCurrentChanged(object sender, RoutedEventArgs e)
        {
            RefreshLogEntriesView();
        }
        #endregion
    }
}
