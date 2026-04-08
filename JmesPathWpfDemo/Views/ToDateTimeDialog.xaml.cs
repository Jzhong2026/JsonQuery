using System.Windows;

namespace JmesPathWpfDemo.Views
{
    public partial class ToDateTimeDialog : Window
    {

        public string Format => FormatBox.Text;
        public string FromTimeZone => FromTzComboBox.Text;
        public string ToTimeZone => ToTzComboBox.Text;


        public ToDateTimeDialog()
        {
            InitializeComponent();
            // Populate timezone ComboBoxes
            var timezones = System.TimeZoneInfo.GetSystemTimeZones();
            foreach (var tz in timezones)
            {
                FromTzComboBox.Items.Add(tz.Id);
                ToTzComboBox.Items.Add(tz.Id);
            }
            // Set default value as UTC
            FromTzComboBox.Text = "UTC";
            ToTzComboBox.Text = "UTC";
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
            // Set to a typical ActiveFrom format, adjust as needed
            FormatBox.Text = "yyyy-MM-ddTHH:mm:ssZ";
        }
    }
}
