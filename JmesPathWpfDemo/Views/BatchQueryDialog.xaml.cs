using System.Windows;
using System.Windows.Controls;

namespace JmesPathWpfDemo.Views
{
    public partial class BatchQueryDialog : Window
    {
        public BatchQueryDialog(JmesPathWpfDemo.Models.JsonTreeNode node, string sortedPath)
        {
            InitializeComponent();
            DataContext = new JmesPathWpfDemo.ViewModels.BatchQueryViewModel(node, sortedPath);
        }

        public string GeneratedQuery => (DataContext as JmesPathWpfDemo.ViewModels.BatchQueryViewModel)?.PreviewQuery;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
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
