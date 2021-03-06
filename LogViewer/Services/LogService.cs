﻿using AppStandards.Logging;
using LogViewer.Helpers;
using LogViewer.Models;
using LogViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    /// <summary>
    /// Provides log-related services.
    /// </summary>
    public class LogService
    {
        #region Fields
        private static volatile int _inMemoryLogEntryCount;
        private volatile bool _notifyMaxLogCountReached = true;
        #endregion

        #region Properties
        public MainWindowViewModel MainWindowViewModel { get; set; }
        internal const int MaxInMemoryLogEntryCount = 1000000;
        internal static int InMemoryLogEntryCount { get { return _inMemoryLogEntryCount; } set { _inMemoryLogEntryCount = value; } }
        internal static bool LogEntrySpaceAvailableInMemory
        {
            get
            {
                return InMemoryLogEntryCount < MaxInMemoryLogEntryCount;
            }
        }
        #endregion

        #region Constructor
        public LogService(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
        }
        #endregion

        #region Public/Internal methods
        /// <summary>
        /// Parses a log entry string into a <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="logEntryString">The log entry string to parse.</param>
        /// <param name="parentLogFile">The parent <see cref="LogFile"/>.</param>
        /// <returns>A <see cref="LogEntry"/> containing the information from the log entry string.</returns>
        public static LogEntry ParseLogEntry(string logEntryString, LogFile parentLogFile)
        {
            if (string.IsNullOrWhiteSpace(logEntryString))
            {
                throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(logEntryString));
            }

            //Try to parse as standard log entry
            try
            {
                return ParseStandardLogEntry(logEntryString, parentLogFile);
            }
            catch (Exception ex)
            {
                //If can't parse as standard, try parsing as proficy log entry
                try
                {
                    return ParseProficyLogEntry(logEntryString, parentLogFile);
                }
                catch (Exception ex_prof)
                {
                    var message = $"Could not parse log entry string as standard or Proficy log entry. ";
                    if (logEntryString != null)
                    {
                        message += $"| Log entry string: \"{logEntryString}\" ";
                    }
                    message += $"| Inner exception message from standard parse: {ex.Message} | Inner exception message from Proficy parse: {ex_prof.Message}";
                    throw new Exception(message);
                }
            }
        }

        public static List<string> GetLogEntriesFromLogContents(string logContents)
        {
            var logEntries = new List<string>();

            //Determine whether or not log type is standard
            var prefix = string.Empty;
            if (logContents.Length > 2)
            {
                prefix = logContents.Substring(0, 3);
            }
            if (prefix == "-E-" || prefix == "-W-" || prefix == "-I-" || prefix == "-V-" || prefix == "-D-" || prefix == "-D:E-" || prefix == "-D:W-" || prefix == "-D:I-" || prefix == "-D:V-" || prefix == "-U-")
            {
                //Log is standard log, begin parsing
                var logEntryPartialStrings = Regex.Split(logContents, "(-[EWIVDU]-)");
                if (logEntryPartialStrings.Length < 2)
                {
                    logEntryPartialStrings = Regex.Split(logContents, "(-[D]:[EWIV]-)");
                }
                var combine = false;
                prefix = string.Empty;
                foreach (var logEntryPartialString in logEntryPartialStrings)
                {
                    if (!string.IsNullOrWhiteSpace(logEntryPartialString))
                    {
                        if (combine)
                        {
                            logEntries.Add(prefix + logEntryPartialString);
                        }
                        else
                        {
                            prefix = logEntryPartialString;
                        }
                        combine = !combine;
                    }
                }

                return logEntries;
            }
            else if (IsProficyDateTime(logContents.Substring(0, 23), new TimeSpan(0,0,0), out DateTime proficyDateTime))
            {
                //Log is a proficy log, begin parsing
                logContents = logContents.Replace("\t", " ");
                var logEntryPartialStrings = Regex.Split(logContents, @"(\d+[-]\d+[-]\d+\s\d+[:]\d+[:]\d+[,.]\d+)");
                var combine = false;
                var timeStamp = string.Empty;
                foreach (var logEntryPartialString in logEntryPartialStrings)
                {
                    if (!string.IsNullOrWhiteSpace(logEntryPartialString))
                    {
                        if (combine)
                        {
                            logEntries.Add(timeStamp + logEntryPartialString);
                        }
                        else
                        {
                            timeStamp = logEntryPartialString;
                        }
                        combine = !combine;
                    }
                }

                return logEntries;
            }
            else if (logContents.Substring(0, 7).Contains("**"))
            {
                //Special case: log contents begin with header (*** Log Started ***)
                for (int i = 0; i < logContents.Length; i++)
                {
                    if (int.TryParse(logContents.Substring(i, 1), out int int1))
                    {
                        //If hit an int, might be start of datetime. Try removing header and re-processing
                        return GetLogEntriesFromLogContents(logContents.Substring(i, logContents.Length - i - 1));
                    }
                }
            }
            throw new Exception($"Could not parse log contents as Standard or Proficy log.");
        }

        internal bool IncludeLogEntry(object item)
        {
            /* Determine whether or not to include the given log entry based on the following
             * filters:
             *     The parent log file's KeepCurrent property
             *     The log message type filter
             *     The log message timestamp filters
             *     The username filter
             *     The computername filter
             *     The search term filter
             *     The exclusion term filter
             */

            var logEntry = (LogEntry)item;
            bool include = true;

            //Check parent log file keep current
            include = logEntry.ParentLog != null && logEntry.ParentLog.KeepCurrent;

            //Check log message type filters
            switch (logEntry.Type)
            {
                case LogMessageType.Error:
                    include = include && MainWindowViewModel.IncludeErrors;
                    break;
                case LogMessageType.Warning:
                    include = include && MainWindowViewModel.IncludeWarnings;
                    break;
                case LogMessageType.Information:
                    include = include && MainWindowViewModel.IncludeInformation;
                    break;
                case LogMessageType.Verbose:
                    include = include && MainWindowViewModel.IncludeVerbose;
                    break;
                case LogMessageType.Debug:
                    include = include && MainWindowViewModel.IncludeDebug;
                    break;
                case LogMessageType.DebugError:
                    include = include && MainWindowViewModel.IncludeDebugErrors;
                    break;
                case LogMessageType.DebugWarning:
                    include = include && MainWindowViewModel.IncludeDebugWarnings;
                    break;
                case LogMessageType.DebugInformation:
                    include = include && MainWindowViewModel.IncludeDebugInformation;
                    break;
                case LogMessageType.DebugVerbose:
                    include = include && MainWindowViewModel.IncludeDebugVerbose;
                    break;
                case LogMessageType.Unknown:
                    include = include && MainWindowViewModel.IncludeUnknown;
                    break;
                default:
                    break;
            }

            //Check time filters
            include = include && IncludeLogEntryBasedOnDateFilter(logEntry);

            //Check username filter
            include = include && ((string.IsNullOrWhiteSpace(MainWindowViewModel.FilterUsername) || string.IsNullOrWhiteSpace(logEntry.Username)) || MainWindowViewModel.FilterUsername == logEntry.Username);

            //Check computername filter
            include = include && ((string.IsNullOrWhiteSpace(MainWindowViewModel.FilterComputername) || string.IsNullOrWhiteSpace(logEntry.Computername)) || MainWindowViewModel.FilterComputername == logEntry.Computername);

            //Check search term filter
            include = include && ((string.IsNullOrWhiteSpace(MainWindowViewModel.SearchTerm) || MainWindowViewModel.SearchTerm == MainWindowViewModel.SearchPrompt) || logEntry.Message.ToLower().Contains(MainWindowViewModel.SearchTerm.ToLower()));

            //Check exclusion term filter
            include = include && ((string.IsNullOrWhiteSpace(MainWindowViewModel.ExclusionTerm) || MainWindowViewModel.ExclusionTerm == MainWindowViewModel.ExcludePrompt) || !logEntry.Message.ToLower().Contains(MainWindowViewModel.ExclusionTerm.ToLower()));

            return include;
        }

        internal bool CanAddLogEntry()
        {
            if (InMemoryLogEntryCount < MaxInMemoryLogEntryCount)
            {
                InMemoryLogEntryCount++;
                _notifyMaxLogCountReached = true;
                return true;
            }

            if (_notifyMaxLogCountReached)
            {
                MainWindowViewModel.Notification = new Notification(MainWindowViewModel, $"The maximum allowable number of log entries has been reached and some of the selected log files could not be fully loaded. To view more log entries, select only the log files that you are interested in viewing.");
                AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"Max log entry count of {MaxInMemoryLogEntryCount} reached.", LogMessageType.Information);
                _notifyMaxLogCountReached = false;
            }
            return false;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Parses a standard log entry string into a <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="logEntryString">The log entry string to parse.</param>
        /// <param name="parentLogFile">The parent <see cref="LogFile"/>.</param>
        /// <returns>A <see cref="LogEntry"/> containing the information from the log entry string.</returns>
        private static LogEntry ParseStandardLogEntry(string logEntryString, LogFile parentLogFile)
        {
            if (string.IsNullOrWhiteSpace(logEntryString))
            {
                throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(logEntryString));
            }

            try
            {
                //Extract the type, timestamp, and message as strings
                var logEntryChars = logEntryString.ToCharArray();
                var typeString = logEntryString.Substring(0, 3); //TODO: LogViewer doesn't currently parse log entries with type indicator that has more than 3 chars (e.g. '-D:E-').
                var timeStampString = string.Empty;
                var username = string.Empty;
                var computerName = string.Empty;
                var message = string.Empty;

                var startingIndex = 4;
                bool skipSpace = true;
                for (int i = startingIndex; i < logEntryChars.Length; i++)
                {
                    if (char.IsWhiteSpace(logEntryChars[i]))
                    {
                        if (skipSpace)
                        {
                            skipSpace = false;
                        }
                        else
                        {
                            timeStampString = logEntryString.Substring(startingIndex, i - startingIndex);
                            timeStampString = timeStampString.Substring(0, timeStampString.Length);
                            message = logEntryString.Substring(i + 1, logEntryString.Length - i - 1);
                            break;
                        }
                    }
                }

                //Determine whether or not log entry contains username and computername
                var usernamePrefix = " | Username: ";
                var computernamePrefix = " | Computer name: ";
                startingIndex = message.IndexOf(usernamePrefix);
                if (startingIndex > -1)
                {
                    var computernameStartingIndex = message.IndexOf(computernamePrefix);
                    if (computernameStartingIndex > -1)
                    {
                        //Extract username
                        username = message.Substring(startingIndex + usernamePrefix.Length, message.Length - startingIndex - (message.Length - computernameStartingIndex) - usernamePrefix.Length);

                        //Extract computername
                        computerName = message.Substring(computernameStartingIndex + computernamePrefix.Length, message.Length - computernameStartingIndex - computernamePrefix.Length);

                        //Remove newline character from computername (the newline char seperates log entries in log file
                        computerName = computerName.Replace(Environment.NewLine, string.Empty);

                        //Extract message
                        message = message.Substring(0, startingIndex);
                    }
                }

                //Determine the type
                LogMessageType type = LogMessageType.Unknown;
                switch (typeString)
                {
                    case "-E-":
                        type = LogMessageType.Error;
                        break;
                    case "-W-":
                        type = LogMessageType.Warning;
                        break;
                    case "-I-":
                        type = LogMessageType.Information;
                        break;
                    case "-V-":
                        type = LogMessageType.Verbose;
                        break;
                    case "-D-":
                        type = LogMessageType.Debug;
                        break;
                    case "-D:E-":
                        type = LogMessageType.DebugError;
                        break;
                    case "-D:W-":
                        type = LogMessageType.DebugWarning;
                        break;
                    case "-D:I-":
                        type = LogMessageType.DebugInformation;
                        break;
                    case "-D:V-":
                        type = LogMessageType.DebugVerbose;
                        break;
                }

                //Parse time stamp
                var timeStamp = DateTime.ParseExact(timeStampString, "MM/dd/yyyy HH:mm:ss:ffff", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

                return new LogEntry(type, timeStamp, username, computerName, message, parentLogFile);
            }
            catch (Exception ex)
            {
                var message = $"Could not parse log entry as standard log entry. ";
                message += $"| Inner exception message: {ex.Message}";
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Parses a Proficy log entry string into a <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="logEntryString">The log entry string to parse.</param>
        /// <param name="parentLogFile">The parent <see cref="LogFile"/>.</param>
        /// <returns>A <see cref="LogEntry"/> containing the information from the log entry string.</returns>
        private static LogEntry ParseProficyLogEntry(string logEntryString, LogFile parentLogFile)
        {
            if (string.IsNullOrWhiteSpace(logEntryString))
            {
                throw new ArgumentException("Parameter cannot be null or whitespace.", nameof(logEntryString));
            }

            try
            {
                //Extract the timestamp and message as strings
                var timeStampString = logEntryString.Substring(0, 23);
                var message = logEntryString.Substring(24, logEntryString.Length - 26);

                #region Extract log message type
                LogMessageType type = LogMessageType.Unknown;
                var tryExtractingLogMessageTypeFromFront = true;

                //try extracting type from end of log message
                var logMessageTypeString = string.Empty;
                if (message.EndsWith("]"))
                {
                    try
                    {
                        var stopPoint = message.Length >= 20 ? message.Length - 20 : 0;
                        for (int i = message.Length - 2; i >= stopPoint; i--)
                        {
                            var lastChar = message.Substring(i, 1);
                            if (lastChar == "[")
                            {
                                break;
                            }
                            else
                            {
                                logMessageTypeString = lastChar + logMessageTypeString;
                            }
                        }
                        type = ParseProficyLogMessageType(logMessageTypeString);
                        if (type != LogMessageType.Unknown)
                        {
                            message = message.Substring(0, message.Length - logMessageTypeString.Length - 2);
                            tryExtractingLogMessageTypeFromFront = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"An error occurred when trying to extract log message type from back end of Proficy log message: \"{message}\" | Error message: {ex.Message}", LogMessageType.Warning);
                    }
                }

                //try extracting type from front of log message
                if (tryExtractingLogMessageTypeFromFront)
                {
                    logMessageTypeString = string.Empty;
                    try
                    {
                        var stopPoint = message.Length >= 150 ? 150 : message.Length;
                        var skip = false;
                        var numSkippedChars = 0;
                        var endSkipChar = " ";

                        for (int i = 0; i < stopPoint; i++)
                        {
                            string nextChar = message.Substring(i, 1);
                            if (nextChar == ":" || nextChar == "[")
                            {
                                if (nextChar == "[")
                                {
                                    //If skipping something with "[(some string)]" at front, stop skipping when hit end bracket
                                    endSkipChar = "]";
                                }

                                if (!string.IsNullOrWhiteSpace(logMessageTypeString))
                                {
                                    break;
                                }
                                else
                                {
                                    skip = true;
                                }
                            }
                            else if (skip && nextChar == endSkipChar)
                            {
                                skip = false;
                                numSkippedChars = i + 1;
                            }
                            else if (!skip)
                            {
                                logMessageTypeString += nextChar;
                            }
                        }
                        type = ParseProficyLogMessageType(logMessageTypeString);
                        if (type != LogMessageType.Unknown)
                        {
                            var skippedChars = message.Substring(0, numSkippedChars);
                            message = skippedChars + message.Substring(logMessageTypeString.Length + numSkippedChars, message.Length - logMessageTypeString.Length - numSkippedChars);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppInfo.BaseAppInfo.Log.QueueLogMessageAsync($"An error occurred when trying to extract log message type from front end of Proficy log message: \"{message}\" | Error message: {ex.Message}", LogMessageType.Warning);
                    }
                }
                #endregion

                //Parse time stamp
                IsProficyDateTime(timeStampString, -parentLogFile.ParentLogFolder.RemoteComputerOffsetFromLocalTime, out DateTime timeStamp);

                return new LogEntry(type, timeStamp, "N/A", parentLogFile.ParentLogFolder.ComputerName, message, parentLogFile);
            }
            catch (Exception ex)
            {
                var message = $"Could not parse log entry as a Proficy log entry. ";
                message += $"| Inner exception message: {ex.Message}";
                throw new Exception(message);
            }
        }

        private static LogMessageType ParseProficyLogMessageType(string proficyLogMessageTypeString)
        {
            //Remove whitespace
            proficyLogMessageTypeString = Regex.Replace(proficyLogMessageTypeString, @"\s", string.Empty);

            switch (proficyLogMessageTypeString.ToLower())
            {
                case "error":
                    return LogMessageType.Error;
                case "err":
                    return LogMessageType.Error;
                case "error:error":
                    return LogMessageType.Error;
                case "warning":
                    return LogMessageType.Warning;
                case "warn":
                    return LogMessageType.Warning;
                case "information":
                    return LogMessageType.Information;
                case "info":
                    return LogMessageType.Information;
                case "verbose":
                    return LogMessageType.Verbose;
                case "debug":
                    return LogMessageType.Debug;
                case "debug:error":
                    return LogMessageType.DebugError;
                case "debug:warning":
                    return LogMessageType.DebugWarning;
                case "debug:information":
                    return LogMessageType.DebugInformation;
                case "debug:info":
                    return LogMessageType.DebugInformation;
                case "debug:verbose":
                    return LogMessageType.DebugVerbose;
                default:
                    return LogMessageType.Unknown;
            }
        }

        private static bool IsProficyDateTime(string proficyDateTimeString, TimeSpan offsetAdjustment, out DateTime proficyDateTime)
        {
            try
            {
                proficyDateTime = DateTime.ParseExact(proficyDateTimeString, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
                proficyDateTime = proficyDateTime.AddMilliseconds(offsetAdjustment.TotalMilliseconds);
                return true;
            }
            catch
            {
                try
                {
                    proficyDateTime = DateTime.ParseExact(proficyDateTimeString, "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
                    proficyDateTime = proficyDateTime.AddMilliseconds(offsetAdjustment.TotalMilliseconds);
                    return true;
                }
                catch
                {
                    throw new Exception($"Could not parse string \"{proficyDateTimeString}\" as Proficy dateTime.");
                }
            }
        }

        private bool IncludeLogEntryBasedOnDateFilter(LogEntry logEntry)
        {
            return logEntry.UtcTimeStamp.ToLocalTime() >= MainWindowViewModel.MinDate && logEntry.UtcTimeStamp.ToLocalTime() <= MainWindowViewModel.MaxDate;
        }
        #endregion
    }
}
