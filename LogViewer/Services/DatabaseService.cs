using AppStandards.Logging;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.Properties;
using LogViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    public static class DatabaseService
    {
        #region Constants
        private const string sqlCommandText = @"
SELECT 
    [SOADB].[dbo].[Local_SSI_ErrorLogDetail].[OBJECT_NAME],
    [SOADB].[dbo].[Local_SSI_ErrorLogDetail].[Error_Section],
	[SOADB].[dbo].[Local_SSI_ErrorLogDetail].[ERROR_MESSAGE],
	[SOADB].[dbo].[Local_SSI_ErrorLogDetail].[TimeStamp],
	[SOADB].[dbo].[Local_SSI_ErrorSeverityLevel].[Severity_Level_Desc]
FROM[SOADB].[dbo].[Local_SSI_ErrorLogDetail]
WITH (NOLOCK)
INNER JOIN [SOADB].[dbo].[Local_SSI_ErrorSeverityLevel]
ON[SOADB].[dbo].[Local_SSI_ErrorSeverityLevel].Severity_Level_Id = [SOADB].[dbo].[Local_SSI_ErrorLogDetail].Error_Severity_Level
ORDER BY [SOADB].[dbo].[Local_SSI_ErrorLogDetail].[TimeStamp]";
        #endregion

        #region Public methods
        public static List<LogEntry> GetDatabaseLogEntries(string computerName, Database parentDatabase)
        {
            var databaseLogEntries = new List<LogEntry>();
            var sqlConnection = new SqlConnection(GetConnectionString(computerName));
            sqlConnection.Open();
            using (SqlCommand sqlCommand = new SqlCommand(sqlCommandText, sqlConnection))
            {
                using (var sqlDataReader = sqlCommand.ExecuteReader())
                {
                    while (sqlDataReader.Read())
                    {
                        databaseLogEntries.Add(ParseDatabaseLogEntry(sqlDataReader, computerName, parentDatabase));
                    }
                }
            }
            sqlConnection.Close();
            sqlConnection.Dispose();
            return databaseLogEntries;
        }

        /// <summary>
        /// Gets all of the <see cref="Database"/>s from the settings file.
        /// </summary>
        /// <param name="mainWindowViewModel">A reference to the <see cref="MainWindowViewModel"/>.</param>
        /// <returns></returns>
        public static List<Database> GetAllDatabasesFromSettingsFile(MainWindowViewModel mainWindowViewModel)
        {
            AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Loading databases from setting file.", LogMessageType.Verbose);
            var databases = new List<Database>();
            foreach (var databaseName in Settings.Default.DatabaseNames)
            {
                databases.Add(new Database(databaseName, mainWindowViewModel));
            }
            return databases;
        }

        /// <summary>
        /// Adds a database name to the settings file.
        /// </summary>
        /// <param name="databaseName">The name of the database to add.</param>
        public static void AddDatabaseNameToSettingsFileAsync(string databaseName)
        {
            Task.Run(() =>
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Adding database \"{databaseName}\" to the settings file.", LogMessageType.Verbose);
                if (Settings.Default.DatabaseNames.Contains(databaseName))
                {
                    throw new ArgumentException($"The database \"{databaseName}\" has already been added to the settings.");
                }
                Settings.Default.DatabaseNames.Add(databaseName);
                Settings.Default.Save();
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully added database \"{databaseName}\" to the settings file.", LogMessageType.Verbose);
            });
        }

        /// <summary>
        /// Removes a database name from the settings file.
        /// </summary>
        /// <param name="databaseName">The name of the database to remove.</param>
        public static void RemoveDatabaseNameFromSettingsFileAsync(string databaseName)
        {
            Task.Run(() =>
            {
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Removing database \"{databaseName}\" from settings file.", LogMessageType.Verbose);
                var index = GetDatabaseNameIndex(databaseName);
                Settings.Default.DatabaseNames.RemoveAt(index);
                Settings.Default.Save();
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Successfully removed database \"{databaseName}\" from settings file.", LogMessageType.Verbose);
            });
        }
        #endregion

        #region Private methods
        private static string GetConnectionString(string computerName)
        {
            return $"Data Source={computerName};Integrated Security=true;Initial Catalog=SOADB";
        }

        private static LogEntry ParseDatabaseLogEntry(SqlDataReader sqlDataReader, string computerName, Database parentDatabase)
        {
            var logMessageType = ParseDatabaseLogEntryType(sqlDataReader["Severity_Level_Desc"].ToString());
            var timeStamp = Convert.ToDateTime(sqlDataReader["TimeStamp"]).ToLocalTime();
            var logMessage = $"Object: \"{sqlDataReader["OBJECT_NAME"].ToString()}\" | Section: \"{sqlDataReader["Error_Section"].ToString()}\" | Message: \"{sqlDataReader["ERROR_MESSAGE"].ToString()}\"";
            return new LogEntry(logMessageType, timeStamp, "N/A", computerName, logMessage, parentDatabase);
        }

        private static LogMessageType ParseDatabaseLogEntryType(string databaseLogEntryType)
        {
            switch (databaseLogEntryType.ToUpper())
            {
                case ("CRITICAL"):
                    return LogMessageType.Error;
                case ("WARNING"):
                    return LogMessageType.Warning;
                case ("INFORMATIONAL"):
                    return LogMessageType.Information;
                default:
                    return LogMessageType.Error;
            }
        }

        /// <summary>
        /// Gets the index of the specified database name in the collection of database names in the settings file.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <returns></returns>
        private static int GetDatabaseNameIndex(string databaseName)
        {
            return Settings.Default.DatabaseNames.IndexOf(databaseName);
        }
        #endregion
    }
}
