using JmesPathWpfDemo.Models;
using System;
using System.Linq;
using System.Windows;

namespace JmesPathWpfDemo.Views
{
    public partial class ClientTimeZoneSettingsDialog : Window
    {
        public string SelectedTimeZoneId => ClientTimezoneComboBox.Text;

        public bool RespectDaylightSavings => RespectDaylightSavingsCheckBox.IsChecked == true;

        public ClientTimeZoneSettingsDialog(ClientTimeZoneSettings settings)
        {
            InitializeComponent();

            var timezones = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => tz.Id)
                .ToList();

            ClientTimezoneComboBox.ItemsSource = timezones;
            ClientTimezoneComboBox.Text = settings?.TimeZoneId ?? ClientTimeZoneSettings.Default.TimeZoneId;
            RespectDaylightSavingsCheckBox.IsChecked = settings?.RespectDaylightSavings ?? ClientTimeZoneSettings.Default.RespectDaylightSavings;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}