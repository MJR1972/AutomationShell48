using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AutomationShell48.UI.Features.HowTo.Services
{
    /// <summary>
    /// Lightweight markdown-to-FlowDocument renderer.
    /// Supports headers, bold text, bullet lines, fenced code blocks, and line breaks.
    /// </summary>
    public class MarkdownParserService
    {
        /// <summary>
        /// Renders markdown to a FlowDocument suitable for FlowDocumentScrollViewer.
        /// H2/H3 headers are rendered as collapsible sections.
        /// </summary>
        public FlowDocument BuildDocument(string markdown, string searchQuery, ICommand copyCodeCommand)
        {
            var blocks = ParseBlocks(markdown ?? string.Empty);
            var document = CreateBaseDocument();
            StackPanel activeSectionPanel = null;
            var hasSection = false;

            foreach (var block in blocks)
            {
                if (block.Type == MarkdownBlockType.Header && (block.HeaderLevel == 2 || block.HeaderLevel == 3))
                {
                    hasSection = true;
                    var expander = CreateSectionExpander(block.Text, block.HeaderLevel);
                    activeSectionPanel = new StackPanel { Margin = new Thickness(4, 4, 4, 4) };
                    expander.Content = activeSectionPanel;
                    document.Blocks.Add(new BlockUIContainer(expander));
                    continue;
                }

                if (block.Type == MarkdownBlockType.BlankLine)
                {
                    AddToTarget(document, activeSectionPanel, new Paragraph { Margin = new Thickness(0, 2, 0, 2) });
                    continue;
                }

                if (block.Type == MarkdownBlockType.Code)
                {
                    var codeBlock = CreateCodeBlock(block.Text, copyCodeCommand);
                    AddToTarget(document, activeSectionPanel, new BlockUIContainer(codeBlock));
                    continue;
                }

                Paragraph paragraph;
                if (block.Type == MarkdownBlockType.Header)
                {
                    paragraph = CreateHeaderParagraph(block.Text, block.HeaderLevel);
                }
                else if (block.Type == MarkdownBlockType.Bullet)
                {
                    paragraph = CreateBulletParagraph(block.Text);
                }
                else
                {
                    paragraph = CreateBodyParagraph(block.Text);
                }

                AppendFormattedInlines(paragraph, block.Text, searchQuery, block.Type == MarkdownBlockType.Bullet);
                AddToTarget(document, activeSectionPanel, paragraph);
            }

            if (!hasSection)
            {
                // Maintain consistent UX if a document does not have H2/H3 sections.
                document.Blocks.Add(new Paragraph(new Run("No section headers found. Add ## headings for collapsible sections."))
                {
                    Margin = new Thickness(0, 10, 0, 0)
                });
            }

            return document;
        }

        private static FlowDocument CreateBaseDocument()
        {
            return new FlowDocument
            {
                PagePadding = new Thickness(0),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 14,
                ColumnWidth = 1200
            };
        }

        private static Expander CreateSectionExpander(string title, int level)
        {
            var expander = new Expander
            {
                IsExpanded = true,
                Margin = new Thickness(0, 8, 0, 4),
                Header = new TextBlock
                {
                    Text = title,
                    FontSize = level == 2 ? 18 : 16,
                    FontWeight = FontWeights.SemiBold
                }
            };

            // Keep section header text theme-aware.
            if (expander.Header is TextBlock tb)
            {
                tb.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
            }

            return expander;
        }

        private static Paragraph CreateHeaderParagraph(string text, int level)
        {
            return new Paragraph
            {
                Margin = new Thickness(0, level == 1 ? 10 : 6, 0, 4),
                FontWeight = FontWeights.SemiBold,
                FontSize = level == 1 ? 24 : (level == 2 ? 18 : 16),
                Foreground = ResolveBrush("TextPrimaryBrush")
            };
        }

        private static Paragraph CreateBodyParagraph(string text)
        {
            return new Paragraph
            {
                Margin = new Thickness(0, 2, 0, 6),
                Foreground = ResolveBrush("TextPrimaryBrush")
            };
        }

        private static Paragraph CreateBulletParagraph(string text)
        {
            return new Paragraph
            {
                Margin = new Thickness(16, 2, 0, 2),
                Foreground = ResolveBrush("TextPrimaryBrush")
            };
        }

        private static FrameworkElement CreateCodeBlock(string code, ICommand copyCodeCommand)
        {
            var grid = new Grid { Margin = new Thickness(0, 6, 0, 10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var topBar = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            var copyButton = new Button
            {
                Content = "Copy",
                MinWidth = 64,
                Height = 28,
                Command = copyCodeCommand,
                CommandParameter = code
            };
            copyButton.SetResourceReference(Control.StyleProperty, "SecondaryButtonStyle");
            DockPanel.SetDock(copyButton, Dock.Right);
            topBar.Children.Add(copyButton);

            var codeBorder = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12)
            };
            codeBorder.SetResourceReference(Border.BackgroundProperty, "CodeBlockBackgroundBrush");
            codeBorder.SetResourceReference(Border.BorderBrushProperty, "CodeBlockBorderBrush");

            var codeBox = new TextBox
            {
                Text = code ?? string.Empty,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            codeBox.SetResourceReference(TextBox.ForegroundProperty, "TextPrimaryBrush");
            codeBorder.Child = codeBox;

            Grid.SetRow(topBar, 0);
            Grid.SetRow(codeBorder, 1);
            grid.Children.Add(topBar);
            grid.Children.Add(codeBorder);
            return grid;
        }

        private static void AddToTarget(FlowDocument document, StackPanel activeSectionPanel, Block block)
        {
            if (activeSectionPanel == null)
            {
                document.Blocks.Add(block);
                return;
            }

            if (block is Paragraph p)
            {
                var host = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Margin = p.Margin
                };
                host.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
                foreach (var inline in p.Inlines.ToList())
                {
                    p.Inlines.Remove(inline);
                    host.Inlines.Add(inline);
                }

                activeSectionPanel.Children.Add(host);
                return;
            }

            if (block is BlockUIContainer bui && bui.Child != null)
            {
                // A UIElement can only have one logical parent at a time.
                // Detach it from BlockUIContainer before attaching to StackPanel.
                var child = bui.Child;
                bui.Child = null;
                activeSectionPanel.Children.Add(child);
            }
        }

        private static void AppendFormattedInlines(Paragraph paragraph, string text, string searchQuery, bool isBullet)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var content = isBullet ? "- " + text : text;
            var segments = content.Split(new[] { "**" }, StringSplitOptions.None);
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (segment.Length == 0)
                {
                    continue;
                }

                var isBold = i % 2 == 1;
                AppendHighlightableRuns(paragraph.Inlines, segment, searchQuery, isBold);
            }
        }

        private static void AppendHighlightableRuns(InlineCollection inlines, string text, string searchQuery, bool isBold)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                inlines.Add(CreateInline(text, isBold, false));
                return;
            }

            var index = 0;
            var source = text;
            var query = searchQuery;
            while (index < source.Length)
            {
                var match = source.IndexOf(query, index, StringComparison.OrdinalIgnoreCase);
                if (match < 0)
                {
                    inlines.Add(CreateInline(source.Substring(index), isBold, false));
                    break;
                }

                if (match > index)
                {
                    inlines.Add(CreateInline(source.Substring(index, match - index), isBold, false));
                }

                inlines.Add(CreateInline(source.Substring(match, query.Length), isBold, true));
                index = match + query.Length;
            }
        }

        private static Inline CreateInline(string text, bool isBold, bool isHighlighted)
        {
            Inline inline = new Run(text);
            if (isBold)
            {
                inline = new Bold(inline);
            }

            if (isHighlighted)
            {
                inline.SetResourceReference(TextElement.BackgroundProperty, "SearchHighlightBrush");
            }

            return inline;
        }

        private static Brush ResolveBrush(string key)
        {
            return Application.Current?.TryFindResource(key) as Brush ?? Brushes.Black;
        }

        private static List<MarkdownBlock> ParseBlocks(string markdown)
        {
            var blocks = new List<MarkdownBlock>();
            var lines = (markdown ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            var inCode = false;
            var codeBuilder = new StringBuilder();

            foreach (var rawLine in lines)
            {
                var line = rawLine ?? string.Empty;

                if (line.StartsWith("```", StringComparison.Ordinal))
                {
                    if (!inCode)
                    {
                        inCode = true;
                        codeBuilder.Clear();
                    }
                    else
                    {
                        inCode = false;
                        blocks.Add(new MarkdownBlock(MarkdownBlockType.Code, codeBuilder.ToString().TrimEnd('\n')));
                        codeBuilder.Clear();
                    }
                    continue;
                }

                if (inCode)
                {
                    codeBuilder.AppendLine(line);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    blocks.Add(new MarkdownBlock(MarkdownBlockType.BlankLine, string.Empty));
                    continue;
                }

                if (line.StartsWith("### ", StringComparison.Ordinal))
                {
                    blocks.Add(new MarkdownBlock(MarkdownBlockType.Header, line.Substring(4).Trim(), 3));
                    continue;
                }

                if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    blocks.Add(new MarkdownBlock(MarkdownBlockType.Header, line.Substring(3).Trim(), 2));
                    continue;
                }

                if (line.StartsWith("# ", StringComparison.Ordinal))
                {
                    blocks.Add(new MarkdownBlock(MarkdownBlockType.Header, line.Substring(2).Trim(), 1));
                    continue;
                }

                if (line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal))
                {
                    blocks.Add(new MarkdownBlock(MarkdownBlockType.Bullet, line.Substring(2).Trim()));
                    continue;
                }

                blocks.Add(new MarkdownBlock(MarkdownBlockType.Paragraph, line.Trim()));
            }

            if (inCode && codeBuilder.Length > 0)
            {
                blocks.Add(new MarkdownBlock(MarkdownBlockType.Code, codeBuilder.ToString().TrimEnd('\n')));
            }

            return blocks;
        }

        private sealed class MarkdownBlock
        {
            public MarkdownBlock(MarkdownBlockType type, string text, int headerLevel = 0)
            {
                Type = type;
                Text = text;
                HeaderLevel = headerLevel;
            }

            public MarkdownBlockType Type { get; }
            public string Text { get; }
            public int HeaderLevel { get; }
        }

        private enum MarkdownBlockType
        {
            Header,
            Paragraph,
            Bullet,
            Code,
            BlankLine
        }
    }
}
