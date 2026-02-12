using System.Windows;
using JmesPathWpfDemo.Models;

namespace JmesPathWpfDemo.Views
{
    public partial class MapArrayDialog : Window
    {
        public MapArrayDialog(JsonTreeNode node, string sortedPath)
        {
            InitializeComponent();
            DataContext = new JmesPathWpfDemo.ViewModels.MapArrayViewModel(node, sortedPath);
        }

        public string GeneratedQuery => (DataContext as JmesPathWpfDemo.ViewModels.MapArrayViewModel)?.PreviewQuery;

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
