using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JmesPathWpfDemo.Views
{
    public partial class ArrayFilterDialog : Window
    {
        private readonly Dictionary<string, List<string>> _propertyValues;

        public string SelectedFilterProperty { get; private set; }
        public string SelectedFilterValue { get; private set; }
        public string SelectedReturnProperty { get; private set; }

        public ArrayFilterDialog(Dictionary<string, List<string>> propertyValues)
        {
            InitializeComponent();

            _propertyValues = propertyValues ?? new Dictionary<string, List<string>>();

            var properties = _propertyValues.Keys.OrderBy(k => k).ToList();
            FilterPropertyComboBox.ItemsSource = properties;

            var returnProperties = new List<string> { "(Whole item)" };
            returnProperties.AddRange(properties);
            ReturnPropertyComboBox.ItemsSource = returnProperties;
            ReturnPropertyComboBox.SelectedIndex = 0;

            if (properties.Count > 0)
            {
                FilterPropertyComboBox.SelectedIndex = 0;
            }
        }

        private void FilterPropertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterPropertyComboBox.SelectedItem is not string selectedProperty)
            {
                FilterValueComboBox.ItemsSource = null;
                return;
            }

            if (_propertyValues.TryGetValue(selectedProperty, out var values))
            {
                var sortedValues = values.OrderBy(v => v).ToList();
                FilterValueComboBox.ItemsSource = sortedValues;
                FilterValueComboBox.SelectedIndex = sortedValues.Count > 0 ? 0 : -1;
            }
            else
            {
                FilterValueComboBox.ItemsSource = null;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedFilterProperty = FilterPropertyComboBox.SelectedItem as string;
            SelectedFilterValue = FilterValueComboBox.SelectedItem as string;
            var returnSelection = ReturnPropertyComboBox.SelectedItem as string;
            SelectedReturnProperty = returnSelection == "(Whole item)" ? null : returnSelection;

            if (string.IsNullOrWhiteSpace(SelectedFilterProperty) || SelectedFilterValue == null)
            {
                MessageBox.Show("Please choose both filter property and value.", "Filter Query",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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
