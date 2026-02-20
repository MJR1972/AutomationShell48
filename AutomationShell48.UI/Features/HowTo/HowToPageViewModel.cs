using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;
using AutomationShell48.UI.Features.HowTo.Models;
using AutomationShell48.UI.Features.HowTo.Services;

namespace AutomationShell48.UI.Features.HowTo
{
    /// <summary>
    /// ViewModel for in-app "How To" learning content.
    /// Loads markdown files from the repository /HowTo folder, supports filtering,
    /// document rendering, section collapse/expand control, copy-code, and PDF export.
    /// </summary>
    public class HowToPageViewModel : BaseViewModel
    {
        private readonly ILogger _logger;
        private readonly MarkdownParserService _markdownParserService;
        private string _searchQuery;
        private HowToDocument _selectedDocument;
        private FlowDocument _renderedDocument;
        private bool _areSectionsExpanded = true;
        private string _documentStatusText;

        /// <summary>
        /// Initializes the feature and loads markdown files from the /HowTo folder.
        /// </summary>
        public HowToPageViewModel(ILogger logger)
        {
            _logger = logger;
            _markdownParserService = new MarkdownParserService();
            Title = "How To";

            Documents = new ObservableCollection<HowToDocument>();
            FilteredDocuments = new ObservableCollection<HowToDocument>();

            CopyCodeCommand = new RelayCommand<string>(CopyCodeToClipboard, code => !string.IsNullOrWhiteSpace(code));
            ExportToPdfCommand = new RelayCommand(ExportToPdf, () => RenderedDocument != null);
            ToggleExpandCollapseCommand = new RelayCommand(ToggleExpandCollapse, () => RenderedDocument != null);

            LoadHowToDocuments();
            ApplyFilter();
            SelectFirstDocument();
        }

        /// <summary>
        /// All loaded markdown docs from disk.
        /// </summary>
        public ObservableCollection<HowToDocument> Documents { get; }

        /// <summary>
        /// Filtered docs shown in the left panel list.
        /// </summary>
        public ObservableCollection<HowToDocument> FilteredDocuments { get; }

        /// <summary>
        /// Search string for filtering and content highlighting.
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (!SetProperty(ref _searchQuery, value))
                {
                    return;
                }

                ApplyFilter();
                RenderSelectedDocument();
            }
        }

        /// <summary>
        /// Current selected doc from the left list.
        /// </summary>
        public HowToDocument SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                if (!SetProperty(ref _selectedDocument, value))
                {
                    return;
                }

                RenderSelectedDocument();
            }
        }

        /// <summary>
        /// FlowDocument bound to center viewer.
        /// </summary>
        public FlowDocument RenderedDocument
        {
            get => _renderedDocument;
            private set
            {
                if (SetProperty(ref _renderedDocument, value))
                {
                    ExportToPdfCommand.RaiseCanExecuteChanged();
                    ToggleExpandCollapseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Status text shown above the document viewer.
        /// </summary>
        public string DocumentStatusText
        {
            get => _documentStatusText;
            private set => SetProperty(ref _documentStatusText, value);
        }

        /// <summary>
        /// Dynamic label for the expand/collapse all command button.
        /// </summary>
        public string ExpandCollapseButtonText => _areSectionsExpanded ? "Collapse All" : "Expand All";

        /// <summary>
        /// Copies code block text to clipboard.
        /// </summary>
        public RelayCommand<string> CopyCodeCommand { get; }

        /// <summary>
        /// Sends current document to print dialog (supports Microsoft Print to PDF).
        /// </summary>
        public RelayCommand ExportToPdfCommand { get; }

        /// <summary>
        /// Toggles all markdown section expanders in the rendered view.
        /// </summary>
        public RelayCommand ToggleExpandCollapseCommand { get; }

        /// <summary>
        /// Loads all markdown files from the root /HowTo folder.
        /// </summary>
        private void LoadHowToDocuments()
        {
            var howToDirectory = ResolveHowToDirectory();
            if (string.IsNullOrWhiteSpace(howToDirectory) || !Directory.Exists(howToDirectory))
            {
                DocumentStatusText = "HowTo folder not found.";
                _logger?.Info("HowTo folder could not be resolved.");
                return;
            }

            var files = Directory
                .GetFiles(howToDirectory, "*.md", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                Documents.Add(new HowToDocument
                {
                    FilePath = file,
                    RawMarkdown = File.ReadAllText(file),
                    Title = BuildTitleFromFileName(Path.GetFileNameWithoutExtension(file))
                });
            }

            DocumentStatusText = Documents.Count + " document(s) loaded.";
            _logger?.Info("HowTo documents loaded: " + Documents.Count);
        }

        /// <summary>
        /// Applies case-insensitive filtering against title and markdown body.
        /// </summary>
        private void ApplyFilter()
        {
            var query = (SearchQuery ?? string.Empty).Trim();
            var filtered = Documents.Where(document =>
                string.IsNullOrWhiteSpace(query) ||
                document.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                document.RawMarkdown.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);

            FilteredDocuments.Clear();
            foreach (var document in filtered)
            {
                FilteredDocuments.Add(document);
            }

            if (SelectedDocument != null && !FilteredDocuments.Contains(SelectedDocument))
            {
                SelectedDocument = null;
            }

            if (SelectedDocument == null && FilteredDocuments.Count > 0)
            {
                SelectedDocument = FilteredDocuments[0];
            }
        }

        /// <summary>
        /// Selects the first available document after initial load.
        /// </summary>
        private void SelectFirstDocument()
        {
            if (FilteredDocuments.Count > 0)
            {
                SelectedDocument = FilteredDocuments[0];
            }
        }

        /// <summary>
        /// Rebuilds FlowDocument from selected markdown and current search query.
        /// </summary>
        private void RenderSelectedDocument()
        {
            if (SelectedDocument == null)
            {
                RenderedDocument = new FlowDocument(new Paragraph(new Run("Select a document from the list.")));
                DocumentStatusText = "No document selected.";
                return;
            }

            RenderedDocument = _markdownParserService.BuildDocument(
                SelectedDocument.RawMarkdown,
                SearchQuery,
                CopyCodeCommand);

            // New content should default to expanded.
            _areSectionsExpanded = true;
            OnPropertyChanged(nameof(ExpandCollapseButtonText));
            DocumentStatusText = SelectedDocument.Title;
        }

        /// <summary>
        /// Copies raw code text to clipboard.
        /// </summary>
        private void CopyCodeToClipboard(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            Clipboard.SetText(code);
            _logger?.Info("HowTo code block copied to clipboard.");
        }

        /// <summary>
        /// Toggles all section expanders at once.
        /// </summary>
        private void ToggleExpandCollapse()
        {
            if (RenderedDocument == null)
            {
                return;
            }

            var expand = !_areSectionsExpanded;
            SetSectionExpansion(expand);
            _areSectionsExpanded = expand;
            OnPropertyChanged(nameof(ExpandCollapseButtonText));
        }

        /// <summary>
        /// Applies one expanded state to each section expander in the rendered doc.
        /// </summary>
        private void SetSectionExpansion(bool expand)
        {
            foreach (var block in RenderedDocument.Blocks)
            {
                if (!(block is BlockUIContainer uiBlock))
                {
                    continue;
                }

                if (uiBlock.Child is Expander expander)
                {
                    expander.IsExpanded = expand;
                }
            }
        }

        /// <summary>
        /// Opens print dialog and prints the current FlowDocument.
        /// Use Microsoft Print to PDF in the dialog for PDF export.
        /// </summary>
        private void ExportToPdf()
        {
            if (RenderedDocument == null)
            {
                return;
            }

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
            {
                return;
            }

            var paginator = ((IDocumentPaginatorSource)RenderedDocument).DocumentPaginator;
            printDialog.PrintDocument(paginator, "AutomationShell48 HowTo Export");
            _logger?.Info("HowTo document exported using print dialog.");
        }

        /// <summary>
        /// Resolves repository root /HowTo folder from runtime locations.
        /// </summary>
        private static string ResolveHowToDirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var candidate = new DirectoryInfo(baseDirectory);

            while (candidate != null)
            {
                var howToPath = Path.Combine(candidate.FullName, "HowTo");
                if (Directory.Exists(howToPath))
                {
                    return howToPath;
                }

                candidate = candidate.Parent;
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts markdown filename into a clean display title.
        /// </summary>
        private static string BuildTitleFromFileName(string rawName)
        {
            return (rawName ?? string.Empty)
                .Replace('_', ' ')
                .Replace("HowTo ", "How To ")
                .Trim();
        }
    }
}
