using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JmesPathWpfDemo.Models;

namespace JmesPathWpfDemo.Views
{
    public partial class JoinQueryDialog : Window
    {
        public string SelectedSeparator { get; private set; }
        public string SelectedProperty { get; private set; }
        public SavedQuery SelectedSavedQuery { get; private set; }
        public bool UsePipeline { get; private set; }

        public JoinQueryDialog(List<string> properties = null, List<SavedQuery> savedQueries = null)
        {
            InitializeComponent();

            if (properties != null && properties.Count > 0)
            {
                PropertyComboBox.ItemsSource = properties;
                PropertyComboBox.SelectedIndex = 0;
            }
            else
            {
                PropertyComboBox.IsEnabled = false;
                PropertyComboBox.Text = "(Use array items directly)";
            }

            if (savedQueries != null && savedQueries.Count > 0)
            {
                SavedQueryComboBox.ItemsSource = savedQueries;
                SavedQueryComboBox.SelectedIndex = 0;
            }
            else
            {
                UsePipelineCheckBox.IsEnabled = false;
                SavedQueryComboBox.IsEnabled = false;
            }

            UpdatePreview();
        }

        private void AddSeparator_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string sep)
            {
                SeparatorTextBox.SelectedText = sep;
                SeparatorTextBox.Focus();
                // Move caret to end of inserted text
                SeparatorTextBox.CaretIndex = SeparatorTextBox.SelectionStart + SeparatorTextBox.SelectionLength;
                // Clear selection
                SeparatorTextBox.SelectionLength = 0;
            }
        }

        private void UsePipelineCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (PreviewTextBlock == null) return;

            var preview = "Preview:\n";
            if (UsePipelineCheckBox?.IsChecked == true && SavedQueryComboBox?.SelectedItem is SavedQuery sq)
            {
                preview += $"({sq.Expression}) | join('{SeparatorTextBox?.Text ?? ", "}', @)";
            }
            else
            {
                preview += $"join('{SeparatorTextBox?.Text ?? ", "}', @)";
            }

            PreviewTextBlock.Text = preview;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedSeparator = SeparatorTextBox.Text;
            SelectedProperty = PropertyComboBox.IsEnabled ? PropertyComboBox.SelectedItem as string : null;
            SelectedSavedQuery = UsePipelineCheckBox.IsChecked == true 
                ? SavedQueryComboBox.SelectedItem as SavedQuery 
                : null;
            UsePipeline = UsePipelineCheckBox.IsChecked ?? false;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
