using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JmesPathWpfDemo.Views
{
    public partial class ArrayFilterDialog : Window
    {
        private readonly Dictionary<string, List<string>> _propertyValues;

        public string FilterExpression { get; private set; }
        public string SelectedProperty => PropertyComboBox.SelectedItem as string;
        public string SelectedOperator => OperatorComboBox.SelectedItem as string;
        public string SelectedValueType => ValueTypeComboBox.SelectedItem as string;
        public string SelectedValue => ValueComboBox.Text;

        // Backward-compatible aliases used by other ViewModels after merge
        public string SelectedFilterProperty => SelectedProperty;
        public string SelectedFilterValue => SelectedValue;
        public string SelectedReturnProperty => string.Empty;

        public ArrayFilterDialog(Dictionary<string, List<string>> propertyValues)
            : this(
                propertyValues?.Keys.OrderBy(k => k).ToList() ?? new List<string>(),
                propertyValues)
        {
        }

        public ArrayFilterDialog(List<string> filterableProperties, Dictionary<string, List<string>> propertyValues = null)
        {
            InitializeComponent();

            _propertyValues = propertyValues ?? new Dictionary<string, List<string>>();

            PropertyComboBox.ItemsSource = filterableProperties ?? new List<string>();
            PropertyComboBox.SelectedIndex = PropertyComboBox.Items.Count > 0 ? 0 : -1;

            OperatorComboBox.ItemsSource = new List<string>
            {
                "==",
                "!=",
                ">",
                ">=",
                "<",
                "<=",
                "contains",
                "starts_with",
                "ends_with"
            };
            OperatorComboBox.SelectedItem = "==";

            ValueTypeComboBox.ItemsSource = new List<string>
            {
                "String",
                "Number",
                "Boolean",
                "Null"
            };
            ValueTypeComboBox.SelectedItem = "String";

            RefreshValueOptions();
            UpdatePreview();
        }

        private void InputChanged(object sender, EventArgs e)
        {
            if (ReferenceEquals(sender, PropertyComboBox))
            {
                RefreshValueOptions();
            }

            UpdatePreview();
        }

        private void RefreshValueOptions()
        {
            var selectedProperty = PropertyComboBox.SelectedItem as string;
            var currentValue = ValueComboBox.Text;

            if (!string.IsNullOrWhiteSpace(selectedProperty) && _propertyValues.TryGetValue(selectedProperty, out var values))
            {
                ValueComboBox.ItemsSource = values;
            }
            else
            {
                ValueComboBox.ItemsSource = Array.Empty<string>();
            }

            ValueComboBox.Text = currentValue;
        }

        private void UpdatePreview()
        {
            var expression = BuildFilterExpression();
            PreviewTextBox.Text = string.IsNullOrWhiteSpace(expression)
                ? "[? ... ]"
                : $"[?{expression}]";
        }

        private string BuildFilterExpression()
        {
            var property = PropertyComboBox.SelectedItem as string;
            var op = OperatorComboBox.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(property) || string.IsNullOrWhiteSpace(op))
            {
                return string.Empty;
            }

            var valueLiteral = BuildValueLiteral();
            if (valueLiteral == null)
            {
                return string.Empty;
            }

            if (op == "contains" || op == "starts_with" || op == "ends_with")
            {
                return $"{op}({property}, {valueLiteral})";
            }

            return $"{property} {op} {valueLiteral}";
        }

        private string BuildValueLiteral()
        {
            var valueType = ValueTypeComboBox.SelectedItem as string;
            var rawValue = ValueComboBox.Text ?? string.Empty;

            switch (valueType)
            {
                case "Number":
                    if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        return $"`{rawValue}`";
                    }
                    return "`0`";
                case "Boolean":
                    if (bool.TryParse(rawValue, out var boolValue))
                    {
                        return boolValue ? "true" : "false";
                    }
                    return "false";
                case "Null":
                    return "null";
                case "String":
                default:
                    var escaped = rawValue.Replace("\\", "\\\\").Replace("'", "\\'");
                    return $"'{escaped}'";
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            FilterExpression = BuildFilterExpression();
            if (string.IsNullOrWhiteSpace(FilterExpression))
            {
                MessageBox.Show("Please provide a valid filter condition.", "Array Filter",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
