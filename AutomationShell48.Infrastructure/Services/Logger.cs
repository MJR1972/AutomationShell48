using System;
using AutomationShell48.Core.Services;

namespace AutomationShell48.Infrastructure.Services
{
    public class Logger : ILogger
    {
        private readonly IFileLogWriter _fileLogWriter;
        private readonly IEventLogWriter _eventLogWriter;

        public Logger(IFileLogWriter fileLogWriter, IEventLogWriter eventLogWriter)
        {
            _fileLogWriter = fileLogWriter;
            _eventLogWriter = eventLogWriter;
        }

        public void Info(string message)
        {
            Write("INFO", message, null);
        }

        public void Warn(string message)
        {
            Write("WARN", message, null);
        }

        public void Error(string message, Exception ex = null)
        {
            Write("ERROR", message, ex);

            try
            {
                _eventLogWriter?.WriteError(message, ex);
            }
            catch
            {
            }
        }

        private void Write(string level, string message, Exception ex)
        {
            try
            {
                var safeMessage = Sanitize(message);
                var line = DateTime.Now.ToString("o") + " [" + level + "] " + safeMessage;
                if (ex != null)
                {
                    line += Environment.NewLine + ex;
                }

                _fileLogWriter?.WriteLine(line);
            }
            catch
            {
            }
        }

        private static string Sanitize(string message)
        {
            return string.IsNullOrWhiteSpace(message) ? "(empty)" : message.Replace(Environment.NewLine, " ");
        }
    }
}
