using System.Collections.Generic;
using System.IO;
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;

namespace AutomationShell48.UI.Tools.LogViewer
{
    public class LogViewerViewModel : BaseViewModel
    {
        private readonly IFileLogWriter _fileLogWriter;
        private string _logText;

        public LogViewerViewModel(IFileLogWriter fileLogWriter)
        {
            _fileLogWriter = fileLogWriter;
            Title = "Log Viewer";
            RefreshCommand = new RelayCommand(Refresh);
            Refresh();
        }

        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        public RelayCommand RefreshCommand { get; }

        private void Refresh()
        {
            var filePath = _fileLogWriter.GetCurrentLogFilePath();
            if (!File.Exists(filePath))
            {
                LogText = "No log file created yet for today.";
                return;
            }

            var queue = new Queue<string>();
            foreach (var line in File.ReadLines(filePath))
            {
                if (queue.Count >= 300)
                {
                    queue.Dequeue();
                }

                queue.Enqueue(line);
            }

            LogText = string.Join("\r\n", queue);
        }
    }
}

