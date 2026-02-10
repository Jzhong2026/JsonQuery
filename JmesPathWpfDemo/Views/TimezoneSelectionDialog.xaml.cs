using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace JmesPathWpfDemo.Views
{
	public partial class TimezoneSelectionDialog : Window
	{
		public string SelectedFromTimezone { get; private set; }
		public string SelectedToTimezone { get; private set; }
		public string SelectedDateFormat { get; private set; }

		public TimezoneSelectionDialog(string defaultFrom = "Central Standard Time", string defaultTo = "Pacific Standard Time", string defaultFormat = "")
		{
			InitializeComponent();

			var timezones = TimeZoneInfo.GetSystemTimeZones()
										.Select(tz => tz.Id)
										.ToList();

			FromTimezoneComboBox.ItemsSource = timezones;
			ToTimezoneComboBox.ItemsSource = timezones;

			// Set defaults if they exist in the list
			if (timezones.Contains(defaultFrom))
				FromTimezoneComboBox.SelectedItem = defaultFrom;
			else
				FromTimezoneComboBox.SelectedIndex = 0;

			if (timezones.Contains(defaultTo))
				ToTimezoneComboBox.SelectedItem = defaultTo;
			else
				ToTimezoneComboBox.SelectedIndex = 0;

			DateFormatTextBox.Text = defaultFormat;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedFromTimezone = FromTimezoneComboBox.SelectedItem as string;
			SelectedToTimezone = ToTimezoneComboBox.SelectedItem as string;
			SelectedDateFormat = DateFormatTextBox.Text;
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
