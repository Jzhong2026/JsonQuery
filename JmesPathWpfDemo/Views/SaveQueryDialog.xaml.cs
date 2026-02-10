using System.Windows;

namespace JmesPathWpfDemo.Views
{
    public partial class SaveQueryDialog : Window
    {
        public string QueryName { get; private set; }
        public string QueryDescription { get; private set; }

        public SaveQueryDialog(string currentQuery)
        {
            InitializeComponent();
            DescriptionTextBox.Text = currentQuery;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            QueryName = NameTextBox.Text;
            QueryDescription = DescriptionTextBox.Text;
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
