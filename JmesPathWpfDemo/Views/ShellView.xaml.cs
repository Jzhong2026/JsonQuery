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
	}
}
