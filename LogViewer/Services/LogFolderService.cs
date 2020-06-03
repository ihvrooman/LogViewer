using AppStandards.Logging;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using LogViewer.ViewModels;
using LogViewerCustomSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    /// <summary>
    /// Provides log folder-related services.
    /// </summary>
    public static class LogFolderService
    {
        /// <summary>
        /// Gets all of the log folders stored in the settings file.
        /// </summary>
        /// <returns>An observable collection of <see cref="LogFolder"/>s.</returns>
        public static List<LogFolder> GetAllLogFoldersFromSettingsFile(MainWindowViewModel mainWindowViewModel)
        {
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Loading log folders from setting file.", LogMessageType.Verbose);
            var logFolders = new List<LogFolder>();
            for (int i = 0; i < Settings.Default.LogFolderPaths.Count; i++)
            {
                var logFolderPath = Settings.Default.LogFolderPaths[i];
                var isActive = Convert.ToBoolean(Settings.Default.LogFolderIsActiveBools[i]);
                logFolders.Add(new LogFolder(logFolderPath, mainWindowViewModel, isActive));
            }
            return logFolders;
        }

        /// <summary>
        /// Adds a log folder path to the settings file.
        /// </summary>
        /// <param name="logFolderPath">The path of the log folder to add.</param>
        public static void AddLogFolderPathToSettingsFileAsync(string logFolderPath)
        {
            Task.Run(() =>
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Adding log folder with path \"{logFolderPath}\" to the settings file.", LogMessageType.Verbose);
                if (Settings.Default.LogFolderPaths.Contains(logFolderPath))
                {
                    throw new ArgumentException($"The log folder with path \"{logFolderPath}\" has already been added to the settings.");
                }
                Settings.Default.LogFolderPaths.Add(logFolderPath);
                Settings.Default.LogFolderIsActiveBools.Add("True");
                Settings.Default.Save();
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully added log folder with path \"{logFolderPath}\" to the settings file.", LogMessageType.Verbose);
            });
        }

        /// <summary>
        /// Removes a log folder path from the settings file.
        /// </summary>
        /// <param name="logFolderPath">The path of the log folder to remove.</param>
        public static void RemoveLogFolderPathFromSettingsFileAsync(string logFolderPath)
        {
            Task.Run(() =>
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Removing log folder with path \"{logFolderPath}\" from settings file.", LogMessageType.Verbose);
                var index = GetLogFolderPathIndex(logFolderPath);
                Settings.Default.LogFolderPaths.RemoveAt(index);
                Settings.Default.LogFolderIsActiveBools.RemoveAt(index);
                Settings.Default.Save();
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully removed log folder with path \"{logFolderPath}\" from settings file.", LogMessageType.Verbose);
            });
        }

        /// <summary>
        /// Removes log folder paths that are invalid from the settings file.
        /// </summary>
        internal static void RemoveInvalidLogFolderPathsFromSettingsFile()
        {
            if (Settings.Default.LogFolderPaths.Count > 0)
            {
                for (int i = Settings.Default.LogFolderPaths.Count - 1; i >= 0; i--)
                {
                    var logFolderPath = Settings.Default.LogFolderPaths[i];
                    try
                    {
                        new DirectoryInfo(logFolderPath);
                    }
                    catch
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Removing invalid log folder path \"{logFolderPath}\" from settings.");
                        Settings.Default.LogFolderPaths.RemoveAt(i);
                    }
                }
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Adds the offline log folder path to the collection of log folder paths in the settings file.
        /// </summary>
        internal static void AddOfflineLogFolderPath()
        {
            if (Settings.Default.AddOfflineLogFolder)
            {
                try
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Attempting to add offline log folder path to settings file.");
                    var offlineLogFolderPath = Settings.Default.OfflineLogFolderPath;
                    offlineLogFolderPath = "\\\\" + Environment.MachineName + "\\" + offlineLogFolderPath.Replace(":", "$");

                    if (!Settings.Default.LogFolderPaths.Contains(offlineLogFolderPath))
                    {
                        AddLogFolderPathToSettingsFileAsync(offlineLogFolderPath);
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully added offline log folder path to settings file.");
                    }
                    else
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Offline log folder path already exists in settings file.");
                    }

                    Settings.Default.AddOfflineLogFolder = false;
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Could not add offline log folder path to settings file. | Offline log folder path: \"{Settings.Default.OfflineLogFolderPath}\" | Error message: {ex.Message}", AppStandards.Logging.LogMessageType.Error);
                }
            }
        }

        /// <summary>
        /// Upgrades the log folder settings.
        /// </summary>
        internal static void UpgradeLogFolderSettings()
        {
            if (Settings.Default.UpgradeLogFolderSettings)
            {
                try
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Attempting to upgrade log folder settings.");
                    Settings.Default.LogFolderIsActiveBools = new StringCollection();
                    for (int i = 0; i < Settings.Default.LogFolderPaths.Count; i++)
                    {
                        Settings.Default.LogFolderIsActiveBools.Add("True");
                    }

                    Settings.Default.UpgradeLogFolderSettings = false;
                    Settings.Default.Save();
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully upgraded log folder settings.");
                }
                catch (Exception ex)
                {
                    AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Failed to upgrade log folder settings. Error message: {ex.Message}");
                }
            }
        }

        public static void UpdateLogFolderIsActiveSettingAsync(string logFolderPath, bool isActive)
        {
            Task.Run(() =>
            {
                var index = Settings.Default.LogFolderPaths.IndexOf(logFolderPath);
                var prevIsActiveValue = Settings.Default.LogFolderIsActiveBools[index];
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Updating IsActive property of log folder with path \"{logFolderPath}\" in settings file. Old value: {prevIsActiveValue} New value: {isActive}", LogMessageType.Verbose);
                Settings.Default.LogFolderIsActiveBools[index] = isActive.ToString();
                Settings.Default.Save();
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully updated IsActive property of log folder with path \"{logFolderPath}\" in settings file.", LogMessageType.Verbose);
            });
        }

        /// <summary>
        /// Gets the index of the specified log folder path in the collection of log folder paths in the settings file.
        /// </summary>
        /// <param name="logFolderPath">The log folder path.</param>
        /// <returns></returns>
        private static int GetLogFolderPathIndex(string logFolderPath)
        {
            return Settings.Default.LogFolderPaths.IndexOf(logFolderPath);
        }
    }
}
