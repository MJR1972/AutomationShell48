using System;
using System.Diagnostics;
using AutomationShell48.Core.Services;

namespace AutomationShell48.Infrastructure.Services
{
    public class EventLogWriter : IEventLogWriter
    {
        private const string SourceName = "AutomationShell48";
        private readonly IFileLogWriter _fileLogWriter;
        private bool _sourceCheckDone;
        private bool _sourceAvailable;

        public EventLogWriter(IFileLogWriter fileLogWriter)
        {
            _fileLogWriter = fileLogWriter;
        }

        public void WriteError(string message, Exception ex = null)
        {
            try
            {
                EnsureSource();
                if (!_sourceAvailable)
                {
                    return;
                }

                var full = ex == null ? message : message + Environment.NewLine + ex;
                EventLog.WriteEntry(SourceName, full, EventLogEntryType.Error);
            }
            catch
            {
            }
        }

        private void EnsureSource()
        {
            if (_sourceCheckDone)
            {
                return;
            }

            _sourceCheckDone = true;

            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, "Application");
                }

                _sourceAvailable = true;
            }
            catch (Exception ex)
            {
                _sourceAvailable = false;
                _fileLogWriter?.WriteLine(DateTime.Now.ToString("o") + " [WARN] EventLog source unavailable. " + ex.Message);
            }
        }
    }
}
