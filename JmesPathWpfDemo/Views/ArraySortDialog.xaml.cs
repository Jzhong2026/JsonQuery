using System.Collections.Generic;
using System.Windows;

namespace JmesPathWpfDemo.Views
{
	public partial class ArraySortDialog : Window
	{
		public string SelectedSortKey { get; private set; }
		public bool SortAscending { get; private set; }

		public ArraySortDialog(List<string> sortKeys, string currentSortKey, bool currentSortAscending)
		{
			InitializeComponent();

			SortKeyComboBox.ItemsSource = sortKeys;
			SortKeyComboBox.SelectedItem = currentSortKey;

			AscendingRadio.IsChecked = currentSortAscending;
			DescendingRadio.IsChecked = !currentSortAscending;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedSortKey = SortKeyComboBox.SelectedItem as string;
			SortAscending = AscendingRadio.IsChecked == true;
			DialogResult = true;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedSortKey = null;
			SortAscending = true;
			DialogResult = true;
			Close();
		}
	}
}
