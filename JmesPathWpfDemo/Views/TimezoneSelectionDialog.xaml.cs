using System;
using System.Collections.Generic;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using System.Linq;
using System.Windows;

namespace JmesPathWpfDemo.Views
{
   public partial class TimezoneSelectionDialog : Window
	{
		private const string ClientDefaultOption = "(Client Default)";

		public string SelectedFromTimezone { get; private set; }
		public string SelectedToTimezone { get; private set; }
		public string SelectedDateFormat { get; private set; }

      public TimezoneSelectionDialog(string defaultFrom = null, string defaultTo = null, string defaultFormat = "")
		{
			InitializeComponent();

			var timezones = TimeZoneInfo.GetSystemTimeZones()
										.Select(tz => tz.Id)
										.ToList();
			timezones.Insert(0, ClientDefaultOption);

			FromTimezoneComboBox.ItemsSource = timezones;
			ToTimezoneComboBox.ItemsSource = timezones;

          var fromSelection = defaultFrom ?? ClientDefaultOption;
			if (timezones.Contains(fromSelection))
				FromTimezoneComboBox.SelectedItem = fromSelection;
			else
               FromTimezoneComboBox.SelectedItem = ClientDefaultOption;

         var toSelection = defaultTo ?? ClientDefaultOption;
			if (timezones.Contains(toSelection))
				ToTimezoneComboBox.SelectedItem = toSelection;
			else
               ToTimezoneComboBox.SelectedItem = ClientDefaultOption;

			DateFormatTextBox.Text = defaultFormat;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
          SelectedFromTimezone = NormalizeSelection(FromTimezoneComboBox.Text);
			SelectedToTimezone = NormalizeSelection(ToTimezoneComboBox.Text);
			SelectedDateFormat = DateFormatTextBox.Text;
			DialogResult = true;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void ClientSettingsButton_Click(object sender, RoutedEventArgs e)
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
			return string.Equals(value, ClientDefaultOption, StringComparison.Ordinal)
				? null
				: value;
		}
	}
}
