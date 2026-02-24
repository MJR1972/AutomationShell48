using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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
    /// document rendering, section collapse/expand control, copy-code, PDF export,
    /// and CRUD editing for markdown docs.
    /// </summary>
    public class HowToPageViewModel : BaseViewModel
    {
        private static string _lastSelectedCodeLanguage = "powershell";
        private readonly ILogger _logger;
        private readonly MarkdownParserService _markdownParserService;
        private string _searchQuery;
        private HowToDocument _selectedDocument;
        private FlowDocument _renderedDocument;
        private bool _areSectionsExpanded = true;
        private string _documentStatusText;
        private bool _isEditorTabVisible;
        private int _selectedTabIndex;

        private string _editorModeText;
        private string _editorDocumentTitle;
        private string _editorFileName;
        private string _editorMarkdown;
        private string _editorTargetFilePath;

        private int _sectionHeaderLevel = 2;
        private string _sectionHeaderText;
        private string _sectionBodyText;
        private string _sectionCodeText;
        private string _sectionCodeLanguage = _lastSelectedCodeLanguage;
        private string _insertAtLineNumberText;

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
            SectionHeaderOptions = new ObservableCollection<HeaderLevelOption>
            {
                new HeaderLevelOption("Main Section (##)", 2),
                new HeaderLevelOption("Subsection (###)", 3)
            };
            CodeLanguages = new ObservableCollection<string>
            {
                "powershell",
                "csharp",
                "json",
                "xaml",
                "xml",
                "sql",
                "bash",
                "yaml",
                "markdown",
                "text"
            };

            CopyCodeCommand = new RelayCommand<string>(CopyCodeToClipboard, code => !string.IsNullOrWhiteSpace(code));
            ExportToPdfCommand = new RelayCommand(ExportToPdf, () => RenderedDocument != null);
            ToggleExpandCollapseCommand = new RelayCommand(ToggleExpandCollapse, () => RenderedDocument != null);

            NewDraftCommand = new RelayCommand(CreateNewDraft);
            LoadSelectedIntoEditorCommand = new RelayCommand(LoadSelectedDocumentIntoEditor, () => SelectedDocument != null);
            AddSectionToDraftCommand = new RelayCommand(AddSectionToDraft, CanAddSectionToDraft);
            InsertCodeBlockCommand = new RelayCommand(InsertCodeBlockTemplate);
            PreviewDraftCommand = new RelayCommand(PreviewDraft, () => !string.IsNullOrWhiteSpace(EditorMarkdown));
            SaveHowToCommand = new RelayCommand(SaveHowToDocument, CanSaveHowToDocument);
            DeleteHowToCommand = new RelayCommand(DeleteHowToDocument, CanDeleteHowToDocument);
            CancelEditingCommand = new RelayCommand(CancelEditing, CanCancelEditing);

            LoadHowToDocuments();
            ApplyFilter();
            SelectFirstDocument();

            CreateNewDraft();
            IsEditorTabVisible = false;
            SelectedTabIndex = 0;
            CancelEditingCommand.RaiseCanExecuteChanged();
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
        /// Header level choices for the section builder.
        /// </summary>
        public ObservableCollection<HeaderLevelOption> SectionHeaderOptions { get; }

        /// <summary>
        /// Common code fence language labels for the editor dropdown.
        /// </summary>
        public ObservableCollection<string> CodeLanguages { get; }

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
                LoadSelectedIntoEditorCommand.RaiseCanExecuteChanged();
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
        /// Controls whether the editor tab is visible.
        /// </summary>
        public bool IsEditorTabVisible
        {
            get => _isEditorTabVisible;
            set => SetProperty(ref _isEditorTabVisible, value);
        }

        /// <summary>
        /// Active tab index for preview/editor tab control.
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        /// <summary>
        /// Editor state label shown in the editor tab.
        /// </summary>
        public string EditorModeText
        {
            get => _editorModeText;
            private set => SetProperty(ref _editorModeText, value);
        }

        /// <summary>
        /// Friendly title used to seed the markdown H1 on save if missing.
        /// </summary>
        public string EditorDocumentTitle
        {
            get => _editorDocumentTitle;
            set
            {
                if (!SetProperty(ref _editorDocumentTitle, value))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(EditorFileName))
                {
                    EditorFileName = GenerateSafeFileNameBase(value);
                }

                SaveHowToCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Markdown file name stored in the HowTo folder.
        /// </summary>
        public string EditorFileName
        {
            get => _editorFileName;
            set
            {
                if (SetProperty(ref _editorFileName, value))
                {
                    SaveHowToCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Raw markdown draft.
        /// </summary>
        public string EditorMarkdown
        {
            get => _editorMarkdown;
            set
            {
                if (SetProperty(ref _editorMarkdown, value))
                {
                    PreviewDraftCommand.RaiseCanExecuteChanged();
                    SaveHowToCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Section header level for section-builder inserts.
        /// </summary>
        public int SectionHeaderLevel
        {
            get => _sectionHeaderLevel;
            set => SetProperty(ref _sectionHeaderLevel, value);
        }

        /// <summary>
        /// Section heading text.
        /// </summary>
        public string SectionHeaderText
        {
            get => _sectionHeaderText;
            set
            {
                if (SetProperty(ref _sectionHeaderText, value))
                {
                    AddSectionToDraftCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Section body/sub text.
        /// </summary>
        public string SectionBodyText
        {
            get => _sectionBodyText;
            set => SetProperty(ref _sectionBodyText, value);
        }

        /// <summary>
        /// Optional section code text.
        /// </summary>
        public string SectionCodeText
        {
            get => _sectionCodeText;
            set => SetProperty(ref _sectionCodeText, value);
        }

        /// <summary>
        /// Optional fenced code language label.
        /// </summary>
        public string SectionCodeLanguage
        {
            get => _sectionCodeLanguage;
            set
            {
                if (SetProperty(ref _sectionCodeLanguage, value))
                {
                    var language = (value ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(language))
                    {
                        _lastSelectedCodeLanguage = language;
                    }
                }
            }
        }

        /// <summary>
        /// Optional insertion line number (1-based). Leave blank to append to end.
        /// </summary>
        public string InsertAtLineNumberText
        {
            get => _insertAtLineNumberText;
            set => SetProperty(ref _insertAtLineNumberText, value);
        }

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
        /// Clears the editor for a new How To draft.
        /// </summary>
        public RelayCommand NewDraftCommand { get; }

        /// <summary>
        /// Loads the selected preview document into the editor.
        /// </summary>
        public RelayCommand LoadSelectedIntoEditorCommand { get; }

        /// <summary>
        /// Appends a new markdown section to the draft.
        /// </summary>
        public RelayCommand AddSectionToDraftCommand { get; }

        /// <summary>
        /// Appends an empty fenced code block to the draft.
        /// </summary>
        public RelayCommand InsertCodeBlockCommand { get; }

        /// <summary>
        /// Renders the current draft into the preview pane.
        /// </summary>
        public RelayCommand PreviewDraftCommand { get; }

        /// <summary>
        /// Saves the draft to a markdown file in /HowTo.
        /// </summary>
        public RelayCommand SaveHowToCommand { get; }

        /// <summary>
        /// Deletes the current editor document.
        /// </summary>
        public RelayCommand DeleteHowToCommand { get; }

        /// <summary>
        /// Hides the editor without saving changes.
        /// </summary>
        public RelayCommand CancelEditingCommand { get; }

        /// <summary>
        /// Loads all markdown files from the root /HowTo folder.
        /// </summary>
        private void LoadHowToDocuments()
        {
            Documents.Clear();

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

            try
            {
                Clipboard.SetText(code);
                DocumentStatusText = "Code block copied to clipboard.";
                _logger?.Info("HowTo code block copied to clipboard.");
            }
            catch (Exception ex)
            {
                DocumentStatusText = "Copy failed. Try again.";
                _logger?.Error("Failed to copy HowTo code block.", ex);
            }
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

        private void CreateNewDraft()
        {
            IsEditorTabVisible = true;
            SelectedTabIndex = 1;
            CancelEditingCommand.RaiseCanExecuteChanged();
            _editorTargetFilePath = null;
            EditorModeText = "New How To Draft";
            EditorDocumentTitle = string.Empty;
            EditorFileName = string.Empty;
            EditorMarkdown = "# New How To" + Environment.NewLine + Environment.NewLine + "## Overview" + Environment.NewLine + Environment.NewLine;

            ClearSectionBuilderInputs();
            DeleteHowToCommand.RaiseCanExecuteChanged();
            SaveHowToCommand.RaiseCanExecuteChanged();
        }

        private void LoadSelectedDocumentIntoEditor()
        {
            if (SelectedDocument == null)
            {
                return;
            }

            IsEditorTabVisible = true;
            SelectedTabIndex = 1;
            CancelEditingCommand.RaiseCanExecuteChanged();
            _editorTargetFilePath = SelectedDocument.FilePath;
            EditorMarkdown = SelectedDocument.RawMarkdown ?? string.Empty;
            EditorFileName = Path.GetFileNameWithoutExtension(SelectedDocument.FilePath);
            EditorDocumentTitle = ExtractPrimaryHeading(EditorMarkdown);
            if (string.IsNullOrWhiteSpace(EditorDocumentTitle))
            {
                EditorDocumentTitle = SelectedDocument.Title;
            }

            EditorModeText = "Editing: " + SelectedDocument.Title;
            ClearSectionBuilderInputs();
            DeleteHowToCommand.RaiseCanExecuteChanged();
            SaveHowToCommand.RaiseCanExecuteChanged();
        }

        private bool CanAddSectionToDraft()
        {
            return !string.IsNullOrWhiteSpace(SectionHeaderText);
        }

        private void AddSectionToDraft()
        {
            var header = (SectionHeaderText ?? string.Empty).Trim();
            if (header.Length == 0)
            {
                return;
            }

            var builder = new StringBuilder();
            EnsureDraftSpacing(builder);
            builder.Append(new string('#', SectionHeaderLevel <= 2 ? 2 : 3))
                   .Append(' ')
                   .AppendLine(header)
                   .AppendLine();

            var body = NormalizeMarkdownLineEndings(SectionBodyText).Trim();
            if (body.Length > 0)
            {
                builder.AppendLine(body);
                builder.AppendLine();
            }

            var code = NormalizeMarkdownLineEndings(SectionCodeText).TrimEnd();
            if (code.Length > 0)
            {
                builder.Append("```").Append((SectionCodeLanguage ?? string.Empty).Trim()).AppendLine();
                builder.AppendLine(code);
                builder.AppendLine("```");
                builder.AppendLine();
            }

            EditorMarkdown = InsertTextIntoMarkdown(EditorMarkdown, builder.ToString(), InsertAtLineNumberText, out var insertStatus);

            SectionHeaderText = string.Empty;
            SectionBodyText = string.Empty;
            SectionCodeText = string.Empty;
            DocumentStatusText = insertStatus;
            _logger?.Info("HowTo section appended to draft.");
        }

        private void InsertCodeBlockTemplate()
        {
            var builder = new StringBuilder();
            EnsureDraftSpacing(builder);
            builder.Append("```").Append((SectionCodeLanguage ?? string.Empty).Trim()).AppendLine();
            builder.AppendLine("# add code here");
            builder.AppendLine("```");
            builder.AppendLine();

            EditorMarkdown = InsertTextIntoMarkdown(EditorMarkdown, builder.ToString(), InsertAtLineNumberText, out var insertStatus);
            DocumentStatusText = insertStatus.Replace("Section", "Code block");
        }

        private void PreviewDraft()
        {
            RenderedDocument = _markdownParserService.BuildDocument(
                EditorMarkdown ?? string.Empty,
                SearchQuery,
                CopyCodeCommand);

            _areSectionsExpanded = true;
            SelectedTabIndex = 0;
            OnPropertyChanged(nameof(ExpandCollapseButtonText));
            DocumentStatusText = "Draft preview";
        }

        private bool CanSaveHowToDocument()
        {
            return !string.IsNullOrWhiteSpace(EditorMarkdown) &&
                   (!string.IsNullOrWhiteSpace(EditorFileName) || !string.IsNullOrWhiteSpace(EditorDocumentTitle));
        }

        private void SaveHowToDocument()
        {
            var howToDirectory = ResolveHowToDirectory();
            if (string.IsNullOrWhiteSpace(howToDirectory) || !Directory.Exists(howToDirectory))
            {
                DocumentStatusText = "HowTo folder not found.";
                return;
            }

            var fileNameBase = GenerateSafeFileNameBase(string.IsNullOrWhiteSpace(EditorFileName) ? EditorDocumentTitle : EditorFileName);
            if (string.IsNullOrWhiteSpace(fileNameBase))
            {
                MessageBox.Show("Enter a valid file name or title before saving.", "How To", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var targetPath = Path.Combine(howToDirectory, fileNameBase + ".md");
            var sourcePath = _editorTargetFilePath;
            var sourceExists = !string.IsNullOrWhiteSpace(sourcePath) && File.Exists(sourcePath);
            var isRenaming = sourceExists && !PathsEqual(sourcePath, targetPath);

            if (File.Exists(targetPath) && (!sourceExists || !PathsEqual(sourcePath, targetPath)))
            {
                MessageBox.Show("A How To file with that name already exists. Choose a different file name.", "How To", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var markdownToSave = BuildMarkdownForSave();

            try
            {
                File.WriteAllText(targetPath, markdownToSave);

                if (isRenaming && File.Exists(sourcePath))
                {
                    File.Delete(sourcePath);
                }

                ReloadDocumentsPreservingSelection(targetPath);
                var saved = Documents.FirstOrDefault(d => PathsEqual(d.FilePath, targetPath));
                if (saved != null)
                {
                    SelectedTabIndex = 1;
                    IsEditorTabVisible = true;
                    _editorTargetFilePath = saved.FilePath;
                    EditorMarkdown = saved.RawMarkdown;
                    EditorFileName = Path.GetFileNameWithoutExtension(saved.FilePath);
                    EditorDocumentTitle = ExtractPrimaryHeading(saved.RawMarkdown);
                    if (string.IsNullOrWhiteSpace(EditorDocumentTitle))
                    {
                        EditorDocumentTitle = saved.Title;
                    }

                    EditorModeText = "Editing: " + saved.Title;
                }

                DocumentStatusText = "How To saved: " + Path.GetFileName(targetPath);
                _logger?.Info("HowTo saved: " + targetPath);
                DeleteHowToCommand.RaiseCanExecuteChanged();
                CancelEditingCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                DocumentStatusText = "Failed to save How To.";
                _logger?.Error("Failed to save HowTo document.", ex);
                MessageBox.Show("Save failed. Check the log for details.", "How To", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteHowToDocument()
        {
            return !string.IsNullOrWhiteSpace(_editorTargetFilePath) && File.Exists(_editorTargetFilePath);
        }

        private bool CanCancelEditing()
        {
            return IsEditorTabVisible;
        }

        private void CancelEditing()
        {
            IsEditorTabVisible = false;
            SelectedTabIndex = 0;
            DocumentStatusText = "Editor closed without saving.";
            CancelEditingCommand.RaiseCanExecuteChanged();
        }

        private void DeleteHowToDocument()
        {
            if (!CanDeleteHowToDocument())
            {
                return;
            }

            var filePath = _editorTargetFilePath;
            var fileName = Path.GetFileName(filePath);
            var confirm = MessageBox.Show(
                "Are you sure you want to delete " + fileName + " from the HowTo folder?" + Environment.NewLine + Environment.NewLine + "This cannot be undone.",
                "Delete How To",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                File.Delete(filePath);
                _logger?.Info("HowTo deleted: " + filePath);

                if (SelectedDocument != null && PathsEqual(SelectedDocument.FilePath, filePath))
                {
                    SelectedDocument = null;
                }

                ReloadDocumentsPreservingSelection(null);
                CreateNewDraft();
                SelectedTabIndex = 0;
                IsEditorTabVisible = false;
                CancelEditingCommand.RaiseCanExecuteChanged();

                if (SelectedDocument == null)
                {
                    DocumentStatusText = "How To deleted.";
                }
            }
            catch (Exception ex)
            {
                DocumentStatusText = "Failed to delete How To.";
                _logger?.Error("Failed to delete HowTo document.", ex);
                MessageBox.Show("Delete failed. Check the log for details.", "Delete How To", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadDocumentsPreservingSelection(string preferredFilePath)
        {
            var currentPath = SelectedDocument?.FilePath;
            SelectedDocument = null;
            LoadHowToDocuments();
            ApplyFilter();

            var targetPath = preferredFilePath ?? currentPath;
            if (!string.IsNullOrWhiteSpace(targetPath))
            {
                var preferred = FilteredDocuments.FirstOrDefault(d => PathsEqual(d.FilePath, targetPath)) ??
                                Documents.FirstOrDefault(d => PathsEqual(d.FilePath, targetPath));
                if (preferred != null)
                {
                    SelectedDocument = preferred;
                    return;
                }
            }

            SelectFirstDocument();
        }

        private string BuildMarkdownForSave()
        {
            var markdown = NormalizeMarkdownLineEndings(EditorMarkdown).Trim();
            var title = (EditorDocumentTitle ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(title) && !markdown.StartsWith("# ", StringComparison.Ordinal))
            {
                markdown = "# " + title + Environment.NewLine + Environment.NewLine + markdown;
            }

            return markdown + Environment.NewLine;
        }

        private void EnsureDraftSpacing(StringBuilder builder)
        {
            var draft = EditorMarkdown ?? string.Empty;
            if (draft.Length == 0)
            {
                return;
            }

            var normalized = draft.Replace("\r\n", "\n");
            if (!normalized.EndsWith("\n\n", StringComparison.Ordinal))
            {
                if (!normalized.EndsWith("\n", StringComparison.Ordinal))
                {
                    builder.AppendLine();
                }

                builder.AppendLine();
            }
        }

        private void ClearSectionBuilderInputs()
        {
            SectionHeaderLevel = 2;
            SectionHeaderText = string.Empty;
            SectionBodyText = string.Empty;
            SectionCodeText = string.Empty;
            InsertAtLineNumberText = string.Empty;
        }

        private static string InsertTextIntoMarkdown(string currentMarkdown, string insertBlock, string insertAtLineNumberText, out string statusMessage)
        {
            var current = currentMarkdown ?? string.Empty;
            var block = insertBlock ?? string.Empty;
            if (string.IsNullOrWhiteSpace(block))
            {
                statusMessage = "Nothing inserted.";
                return current;
            }

            if (string.IsNullOrWhiteSpace(insertAtLineNumberText))
            {
                statusMessage = "Section inserted at end of draft.";
                return current + block;
            }

            if (!int.TryParse(insertAtLineNumberText.Trim(), out var requestedLine) || requestedLine < 1)
            {
                statusMessage = "Invalid insert line. Section inserted at end of draft.";
                return current + block;
            }

            var normalizedCurrent = current.Replace("\r\n", "\n");
            var endsWithNewLine = normalizedCurrent.EndsWith("\n", StringComparison.Ordinal);
            var lines = normalizedCurrent.Split('\n').ToList();
            if (lines.Count == 1 && lines[0].Length == 0)
            {
                lines.Clear();
            }

            var normalizedBlock = block.Replace("\r\n", "\n");
            var blockLines = normalizedBlock.Split('\n').ToList();
            if (blockLines.Count > 0 && blockLines[blockLines.Count - 1] == string.Empty)
            {
                blockLines.RemoveAt(blockLines.Count - 1);
            }

            var insertIndex = Math.Min(requestedLine - 1, lines.Count);
            lines.InsertRange(insertIndex, blockLines);

            var result = string.Join(Environment.NewLine, lines);
            if (endsWithNewLine || normalizedBlock.EndsWith("\n", StringComparison.Ordinal))
            {
                result += Environment.NewLine;
            }

            statusMessage = "Section inserted at line " + requestedLine + ".";
            return result;
        }

        private static string NormalizeMarkdownLineEndings(string text)
        {
            return (text ?? string.Empty).Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }

        private static string GenerateSafeFileNameBase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var cleaned = value.Trim();
            if (cleaned.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = Path.GetFileNameWithoutExtension(cleaned);
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            cleaned = new string(cleaned.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());

            while (cleaned.Contains("  "))
            {
                cleaned = cleaned.Replace("  ", " ");
            }

            return cleaned.Trim(' ', '.');
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractPrimaryHeading(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Empty;
            }

            foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                if (line.StartsWith("# ", StringComparison.Ordinal))
                {
                    return line.Substring(2).Trim();
                }
            }

            return string.Empty;
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

        public sealed class HeaderLevelOption
        {
            public HeaderLevelOption(string label, int level)
            {
                Label = label;
                Level = level;
            }

            public string Label { get; }
            public int Level { get; }
        }
    }
}
