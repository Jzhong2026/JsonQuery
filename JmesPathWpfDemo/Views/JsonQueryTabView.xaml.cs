using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.ViewModels;
using System.Windows;
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

        private void CreateTabFromNode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is JsonTreeNode node)
            {
                var vm = DataContext as JsonQueryTabViewModel;
                vm?.CreateTabFromNode(node);
            }
        }

        private void GenerateToDateTime_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is JsonTreeNode node)
            {
                var vm = DataContext as JsonQueryTabViewModel;
                vm?.GenerateToDateTimeQuery(node);
            }
        }

        private void GenerateJoin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is JsonTreeNode node)
            {
                var vm = DataContext as JsonQueryTabViewModel;
                vm?.GenerateJoinQuery(node);
            }
        }

        private void GenerateFunction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && 
                menuItem.Tag is JsonTreeNode node && 
                menuItem.CommandParameter is string functionName)
            {
                var vm = DataContext as JsonQueryTabViewModel;
                vm?.GenerateFunctionQuery(node, functionName);
            }
        }

        private void ConfigureSort_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is JsonTreeNode node)
            {
                var vm = DataContext as JsonQueryTabViewModel;
                vm?.ConfigureArraySort(node);
            }
        }

        private void ClearSort_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is JsonTreeNode node)
            {
                node.SortKey = null;
                node.SortAscending = true;
            }
        }
    }
}
