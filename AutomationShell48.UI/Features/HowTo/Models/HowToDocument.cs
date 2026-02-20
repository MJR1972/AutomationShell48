namespace AutomationShell48.UI.Features.HowTo.Models
{
    /// <summary>
    /// Represents one markdown source file displayed in the How To feature.
    /// </summary>
    public class HowToDocument
    {
        /// <summary>
        /// Display title in the left navigation list.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Absolute file path for the markdown source.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Raw markdown text loaded from disk.
        /// </summary>
        public string RawMarkdown { get; set; }
    }
}
