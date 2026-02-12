using System.Windows;
using System.Windows.Controls;

namespace JmesPathWpfDemo.Views
{
    public partial class DateFormatDialog : Window
    {
        public string SourceFormat { get; private set; }
        public string TargetFormat { get; private set; }

        public DateFormatDialog(string detectedFormat = "")
        {
            InitializeComponent();
            
            if (!string.IsNullOrEmpty(detectedFormat))
            {
                SourceFormatTextBox.Text = detectedFormat;
            }
            
            // Set default target format
            TargetFormatTextBox.Text = "yyyy-MM-dd";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SourceFormat = SourceFormatTextBox.Text?.Trim();
            TargetFormat = TargetFormatTextBox.Text?.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UseDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            SourceFormatTextBox.Text = "";
            TargetFormatTextBox.Text = "";
        }

        private void ClearSource_Click(object sender, RoutedEventArgs e)
        {
            SourceFormatTextBox.Text = "";
            SourceFormatTextBox.Focus();
        }

        private void ClearTarget_Click(object sender, RoutedEventArgs e)
        {
            TargetFormatTextBox.Text = "";
            TargetFormatTextBox.Focus();
        }

        private void QuickFormat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string format)
            {
                TargetFormatTextBox.Text = format;
                TargetFormatTextBox.Focus();
            }
        }
    }
}
