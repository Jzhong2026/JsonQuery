using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace JmesPathWpfDemo.Views
{
    public partial class JsonQueryTabView : UserControl
    {
        public JsonQueryTabView()
        {
            InitializeComponent();
        }

        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Prevent TreeViewItem from handling the event
            // This allows our Border MouseDown to work
        }

        private void NodeText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is JsonTreeNode node)
            {
                var vm = DataContext as JsonQueryTabViewModel;
                vm?.OnNodeSelected(node);
                e.Handled = true;
            }
        }
    }
}
