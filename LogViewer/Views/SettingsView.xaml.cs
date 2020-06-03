using AppStandards;
using AppStandards.Helpers;
using AppStandards.Logging;
using LogViewer.Helpers;
using LogViewer.Properties;
using LogViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LogViewer.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        #region Properties
        private SettingsViewModel _viewModel
        {
            get
            {
                return (SettingsViewModel)DataContext;
            }
        }
        #endregion

        #region Constructor
        public SettingsView(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(mainWindowViewModel);
        }
        #endregion

        #region Private methods
        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPlacement();
            Settings.Default.Save();
            _viewModel.Dispose();
        }

        private void SettingsWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.SetPlacement(Settings.Default.SettingsWindowPlacement);
        }

        private void SaveWindowPlacement()
        {
            try
            {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Saving settings window placement.", LogMessageType.Verbose);
                    Settings.Default.SettingsWindowPlacement = this.GetPlacement();
                    Settings.Default.Save();
            }
            catch (Exception ex)
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Failed to save settings window placement. Error message: {ex.Message}", LogMessageType.Warning);
            }
        }
        #endregion
    }
}
