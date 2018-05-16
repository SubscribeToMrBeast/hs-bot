﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Discord;
using System.Reflection;
using System.Threading.Tasks;
using NReco.Converting;

namespace HSBot.Helpers
{
    internal class Utilities
    {
        private static readonly DateTime Unixepoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static Dictionary<string, string> _alerts;

        /// <summary>
        /// Verbose is all, 
        /// Debug includes LogSeverity.Debug,
        /// Normal is LogSeverity.Info, Warning, Error, Critical.
        /// </summary>
        /// <param name="Verbose">Logs more.</param>
        /// <param name="Debug">Logs all.</param>
        public enum LogMode
        {
            Debug,
            Verbose,
            Normal
        };
        public static LogMode GlobalLogMode = LogMode.Normal;

        static Utilities()
        {
            if (!Directory.Exists("SystemLang") || !File.Exists("SystemLang/alerts.json"))
            {
                Directory.CreateDirectory("SystemLang");
                File.Create("SystemLang/alerts.json");
                Log("Utilites", "alerts.json created.", LogSeverity.Verbose);
            }
            else
            {
                try
                {
                    string json = File.ReadAllText("SystemLang/alerts.json");
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    _alerts = data.ToObject<Dictionary<string, string>>();
                    Log("Utilities", "alerts.json read.", LogSeverity.Verbose);
                }
                catch
                {
                    Log("Utilities", "error reading alerts.json -- is your file empty?", LogSeverity.Error);
                }
            }
        }

        public static double GetEpochTime()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime GetDateTime(double unixEpochTimeStamp)
        {
            return Unixepoch.AddSeconds(unixEpochTimeStamp).ToLocalTime();
        }

        public static string GetAlert(string key)
        {
            if (_alerts.ContainsKey(key)) return _alerts[key];
            return "";
        }

        public static string GetFormattedAlert(string key, object parameter)
        {
            if (_alerts.ContainsKey(key))
            {
                return String.Format(_alerts[key], parameter);
            }
            return "";
        }

        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    Log(MethodBase.GetCurrentMethod(), $"File written to {fileName}, Length = {byteArray.Length}", LogSeverity.Error);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log(MethodBase.GetCurrentMethod(), $"File failed to write to {fileName}: \"{ex.ToString()}\"", LogSeverity.Error, ex);
                return false;
            }
        }

        /// <summary>
        /// Automatically logs to the console [and if configured then log channel]
        /// </summary>
        /// <param name="severity">Logseverity is by default Info.</param>
        /// /// <param name="message">Nothing will be sent with no message.</param>
        public static Task Log(string source, string message, LogSeverity severity = LogSeverity.Info,
            Exception exception = null)
        {
            if (message == "") return Task.CompletedTask;
            var cc = Console.ForegroundColor;
            string log = "";
            switch (severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            if (GlobalLogMode == LogMode.Debug || GlobalLogMode == LogMode.Verbose)
            {
                log = $"{DateTime.Now.TimeOfDay,-15} [ {severity,-7} | {source,-75}: {message}";
            }
            else
            {
                log = $"{DateTime.Now.ToLongTimeString(),-15} [{severity}] {source,-30}: {message}";
            }
            Console.WriteLine(log);
            if (exception != null && GlobalLogMode == LogMode.Debug) Console.WriteLine(exception);
            if (severity == LogSeverity.Critical)
            {
                Console.WriteLine("This log is a critical error, the system is haulted.");
                Task.Delay(-1);
            }
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }
        /// <param name="source">MethodBase.GetCurrentMethod()</param>
        public static Task Log(MethodBase source, string message, LogSeverity severity = LogSeverity.Info,
            Exception exception = null)
        {
            string sourceToString = "???";
            switch (GlobalLogMode)
            {
                case LogMode.Normal:
                    sourceToString = String.Format("{0}",
                        GetBetween(source.ReflectedType.Name, "<", ">"));
                    break;
                case LogMode.Debug:
                case LogMode.Verbose:
                    sourceToString = String.Format("{0}.{1}",
                        GetBetween(source.DeclaringType.ToString(), ".", "+"),
                        GetBetween(source.ReflectedType.Name, "<", ">"));
                    break;
            }
            Log(sourceToString, message, severity, exception);
            return Task.CompletedTask;
        }

        public static Task Log(string source, string message, Exception exception, 
            LogSeverity severity = LogSeverity.Error) => Log(source, message, severity, exception);

        public static Task Log(MethodBase source, string message, Exception exception,
            LogSeverity severity = LogSeverity.Error) => Log(source, message, severity, exception);

        public static bool IsNumericType(object o)
        {   
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Finds text in between two strings.
        /// Can not handle a mismatch in order [Start cannot be end.]
        /// </summary>
        /// <param name="strStart">Leave empty to assume start.</param>
        /// <param name="strEnd">Leave empty to assume end.</param>
        /// <returns>The text in between the selection criterion.</returns>
        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            int start, end;
            if (strSource.Contains(strStart) || strSource.Contains(strEnd))
            {
                start = strSource.IndexOf(strStart, 0) + strStart.Length;
                end = strSource.IndexOf(strEnd, start);
                int length = end - start;
                if (length == 0) length = 1;
                if (strEnd == "") length = strSource.Length - start;
                if (strStart == "") length = end;
                return strSource.Substring(start, length);
            }
            else
            {
                return "";
            }
        }
    }


}
