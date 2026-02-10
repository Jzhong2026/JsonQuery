using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace JmesPathWpfDemo.Views
{
    public partial class JoinQueryDialog : Window
    {
        public string SelectedSeparator { get; private set; }
        public string SelectedProperty { get; private set; }

        public JoinQueryDialog(List<string> properties = null)
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

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedSeparator = SeparatorTextBox.Text;
            SelectedProperty = PropertyComboBox.IsEnabled ? PropertyComboBox.SelectedItem as string : null;
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
