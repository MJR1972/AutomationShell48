namespace AutomationShell48.Core.Services
{
    public interface IFileLogWriter
    {
        void WriteLine(string line);
        string GetCurrentLogFilePath();
    }
}
