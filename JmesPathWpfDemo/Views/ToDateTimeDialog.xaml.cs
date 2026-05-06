using System.Windows;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;

namespace JmesPathWpfDemo.Views
{
    public partial class ToDateTimeDialog : Window
    {
        private const string ClientDefaultOption = "(Client Default)";

        public string Format => FormatBox.Text;
      public string FromTimeZone => NormalizeSelection(FromTzComboBox.Text);
        public string ToTimeZone => NormalizeSelection(ToTzComboBox.Text);


        public ToDateTimeDialog()
        {
            InitializeComponent();
         var timezones = System.TimeZoneInfo.GetSystemTimeZones();
            FromTzComboBox.Items.Add(ClientDefaultOption);
            ToTzComboBox.Items.Add(ClientDefaultOption);
            foreach (var tz in timezones)
            {
                FromTzComboBox.Items.Add(tz.Id);
                ToTzComboBox.Items.Add(tz.Id);
            }
         FromTzComboBox.Text = ClientDefaultOption;
            ToTzComboBox.Text = ClientDefaultOption;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ClearFormat_Click(object sender, RoutedEventArgs e)
        {
            FormatBox.Clear();
        }

        private void QuickFormat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string fmt)
                FormatBox.Text = fmt;
        }

        private void FitActiveFrom_Click(object sender, RoutedEventArgs e)
        {
            FormatBox.Text = "yyyy-MM-ddTHH:mm:ssZ";
        }

        private void ClientSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClientTimeZoneSettingsDialog(ClientTimeZoneSettingsService.Current);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                ClientTimeZoneSettingsService.Save(new ClientTimeZoneSettings
                {
                    TimeZoneId = dialog.SelectedTimeZoneId,
                    RespectDaylightSavings = dialog.RespectDaylightSavings
                });
            }
        }

        private static string NormalizeSelection(string value)
        {
            return string.Equals(value, ClientDefaultOption, System.StringComparison.Ordinal)
                ? null
                : value;
        }
    }
}
