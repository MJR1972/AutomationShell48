using System.Windows;
using System.Windows.Controls;

namespace AutomationShell48.UI.Features.HowTo
{
    public partial class HowToPageView : UserControl
    {
        public HowToPageView()
        {
            InitializeComponent();
        }

        private void MarkdownDraftTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox textBox) || !(DataContext is HowToPageViewModel vm))
            {
                return;
            }

            try
            {
                var lineNumber = textBox.GetLineIndexFromCharacterIndex(textBox.CaretIndex) + 1;
                if (lineNumber < 1)
                {
                    lineNumber = 1;
                }

                vm.InsertAtLineNumberText = lineNumber.ToString();
            }
            catch
            {
                // Ignore transient layout/caret exceptions during control initialization.
            }
        }
    }
}
