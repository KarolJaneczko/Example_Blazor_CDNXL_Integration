using EBCI_Library.Utils;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Reflection;

namespace EBCI_Library.Services {
    public static class LogService {
        public static string LogsPath { get; private set; }

        private static readonly object SyncRoot = new object();
        private static LoggingConfiguration _config;
        private static Logger _logger;

        public static void Info(string message) => _logger?.Info(message);

        public static void Warn(string message) => _logger?.Warn(message);

        public static void Error(string message, Exception ex = null) => _logger?.Error(ex, message);

        public static void Configure(string directory) {
            lock (SyncRoot) {
                if (_config != null) {
                    return;
                }

                _config = new LoggingConfiguration();

                var logsPath = Path.Combine(directory, "Logs");

                FileHelper.CreateDirectoryIfDoesntExist(logsPath);

                logsPath = Path.Combine(logsPath, $"{DateTime.Now:yyyy-MM}");
                LogsPath = logsPath;
                FileHelper.CreateDirectoryIfDoesntExist(logsPath);

                var infoLogPath = Path.Combine(logsPath, $"Info-{DateTime.Now:yyyy-MM-dd}.log");
                var warningLogPath = Path.Combine(logsPath, $"Warning-{DateTime.Now:yyyy-MM-dd}.log");
                var errorLogPath = Path.Combine(logsPath, $"Error-{DateTime.Now:yyyy-MM-dd}.log");

                var infoTarget = new FileTarget("InfoFile") {
                    FileName = infoLogPath,
                    Layout = "[${longdate}] ${message}"
                };

                var warnTarget = new FileTarget("WarnFile") {
                    FileName = warningLogPath,
                    Layout = "[${longdate}] ${message}"
                };

                var errorTarget = new FileTarget("ErrorFile") {
                    FileName = errorLogPath,
                    Layout = "[${longdate}] ${message} ${exception:format=toString,stacktrace}"
                };

                _config.AddRule(LogLevel.Info, LogLevel.Info, infoTarget);
                _config.AddRule(LogLevel.Warn, LogLevel.Warn, warnTarget);
                _config.AddRule(LogLevel.Error, LogLevel.Fatal, errorTarget);

                LogManager.Configuration = _config;
                _logger = LogManager.GetCurrentClassLogger();
            }
        }
    }
}
