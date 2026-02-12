using System.Collections.Generic;
using System.Windows;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.ViewModels;

namespace JmesPathWpfDemo.Views
{
    public partial class JoinQueryDialog : Window
    {
        public string SelectedSeparator => (DataContext as JoinQueryViewModel)?.SeparatorText;
        public string SelectedProperty => (DataContext as JoinQueryViewModel)?.SelectedProperty;
        public SavedQuery SelectedSavedQuery => (DataContext as JoinQueryViewModel)?.SelectedSavedQuery;
        public bool UsePipeline => (DataContext as JoinQueryViewModel)?.UsePipeline ?? false;

        public JoinQueryDialog(List<string> properties = null, List<SavedQuery> savedQueries = null)
        {
            InitializeComponent();
            DataContext = new JoinQueryViewModel(properties, savedQueries);
        }

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
