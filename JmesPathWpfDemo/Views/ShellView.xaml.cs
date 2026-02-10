using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace JmesPathWpfDemo.Views
{
	public partial class ShellView : Window
	{
		public ShellView()
		{
			InitializeComponent();
		}

		private void NodeText_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;

			var textBlock = sender as FrameworkElement;
			if (textBlock?.DataContext is not JsonTreeNode node)
				return;

			var viewModel = DataContext as ShellViewModel;
			if (viewModel == null)
				return;

			viewModel.OnNodeSelected(node);
			e.Handled = true;
		}

		private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// This event is now primarily used to prevent default TreeViewItem behavior
			// The actual node selection is handled by NodeText_MouseDown

			var originalSource = e.OriginalSource as FrameworkElement;

			// Only allow expander clicks to propagate
			if (originalSource?.TemplatedParent is ToggleButton)
				return;

			// Don't prevent TextBlock clicks from being handled
			if (originalSource is TextBlock)
				return;
		}

		private void GenerateToDateTime_Click(object sender, RoutedEventArgs e)
		{
			var menuItem = sender as MenuItem;
			var contextMenu = menuItem?.Parent as ContextMenu;
			var treeViewItem = contextMenu?.PlacementTarget as TreeViewItem;

			if (treeViewItem?.DataContext is JsonTreeNode node)
			{
				var viewModel = DataContext as ShellViewModel;
				viewModel?.GenerateToDateTimeQuery(node);
			}
		}

		private void GenerateJoin_Click(object sender, RoutedEventArgs e)
		{
			var menuItem = sender as MenuItem;
			var contextMenu = menuItem?.Parent as ContextMenu;
			var treeViewItem = contextMenu?.PlacementTarget as TreeViewItem;

			if (treeViewItem?.DataContext is JsonTreeNode node)
			{
				var viewModel = DataContext as ShellViewModel;
				viewModel?.GenerateJoinQuery(node);
			}
		}

        private void GenerateCombine_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var treeViewItem = contextMenu?.PlacementTarget as TreeViewItem;

            if (treeViewItem?.DataContext is JsonTreeNode node)
            {
                var viewModel = DataContext as ShellViewModel;
                viewModel?.GenerateCombineQuery(node);
            }
        }

		private void GenerateFunction_Click(object sender, RoutedEventArgs e)
		{
			var menuItem = sender as MenuItem;
			var functionName = menuItem?.CommandParameter as string;
			
			// Handle nested menu items - getting the tag from the menu item itself which is bound to the node
			if (menuItem?.Tag is JsonTreeNode node)
			{
				var viewModel = DataContext as ShellViewModel;
				viewModel?.GenerateFunctionQuery(node, functionName);
			}
		}

		private void ConfigureSort_Click(object sender, RoutedEventArgs e)
		{
			var menuItem = sender as MenuItem;
			var contextMenu = menuItem?.Parent as ContextMenu;
			var treeViewItem = contextMenu?.PlacementTarget as TreeViewItem;

			if (treeViewItem?.DataContext is JsonTreeNode node)
			{
				var viewModel = DataContext as ShellViewModel;
				viewModel?.ConfigureArraySort(node);
			}
		}

		private void ClearSort_Click(object sender, RoutedEventArgs e)
		{
			var menuItem = sender as MenuItem;
			var contextMenu = menuItem?.Parent as ContextMenu;
			var treeViewItem = contextMenu?.PlacementTarget as TreeViewItem;

			if (treeViewItem?.DataContext is JsonTreeNode node)
			{
				node.SortKey = null;
				node.SortAscending = true;

				// Update query if this node is selected
				var viewModel = DataContext as ShellViewModel;
				if (viewModel != null && node.IsSelected)
				{
					viewModel.Query = node.Path;
				}
			}
		}
	}
}
