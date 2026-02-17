using System;
using System.IO;
using AutomationShell48.Core.Services;

namespace AutomationShell48.Infrastructure.Services
{
    public class FileLogWriter : IFileLogWriter
    {
        private readonly object _sync = new object();
        private readonly string _logDirectory;

        public FileLogWriter()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _logDirectory = Path.Combine(appData, "AutomationShell48", "Logs");
        }

        public string GetCurrentLogFilePath()
        {
            return Path.Combine(_logDirectory, "log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
        }

        public void WriteLine(string line)
        {
            try
            {
                lock (_sync)
                {
                    Directory.CreateDirectory(_logDirectory);
                    File.AppendAllText(GetCurrentLogFilePath(), line + Environment.NewLine);
                }
            }
            catch
            {
            }
        }
    }
}
