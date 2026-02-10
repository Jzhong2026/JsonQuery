using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JmesPathWpfDemo.Jmes
{
	public sealed class ToDateTimeFunction : JmesPathFunction
	{
		public ToDateTimeFunction()
			: base("todatetime", 1, true)
		{
		}

		public override JToken Execute(params JmesPathFunctionArgument[] args)
		{
			if (args.Length == 0 || args.Length > 4)
			{
				return JValue.CreateNull();
			}

			var dateString = ExtractStringValue(args[0]);
			var format = args.Length > 1 ? ExtractStringValue(args[1]) : null;
			var fromTimezone = args.Length > 2 ? ExtractStringValue(args[2]) : "UTC";
			var toTimezone = args.Length > 3 ? ExtractStringValue(args[3]) : null;

			if (string.IsNullOrWhiteSpace(dateString))
			{
				return JValue.CreateNull();
			}

			try
			{
				var parsedDateTime = TryParseDateTime(dateString, format);
				if (parsedDateTime == DateTime.MinValue)
				{
					return JValue.CreateNull();
				}

				// Get source timezone (default UTC)
				var sourceTimeZone = ConvertTimeZone(fromTimezone, TimeZoneInfo.Utc);
				var targetTimeZone = ConvertTimeZone(toTimezone, TimeZoneInfo.Local);

				DateTime convertedDateTime;

				if (parsedDateTime.Kind != DateTimeKind.Unspecified)
				{
					convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(parsedDateTime.ToUniversalTime(), targetTimeZone);
				}
				else if (sourceTimeZone.Equals(TimeZoneInfo.Utc))
				{
					convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(parsedDateTime.ToUniversalTime(), targetTimeZone);
				}
				else if (targetTimeZone.Equals(TimeZoneInfo.Utc))
				{
					convertedDateTime = TimeZoneInfo.ConvertTimeToUtc(parsedDateTime, sourceTimeZone);
				}
				else
				{
					var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(parsedDateTime, sourceTimeZone);
					convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);
				}

				var offset = targetTimeZone.GetUtcOffset(convertedDateTime);
				var dto = new DateTimeOffset(convertedDateTime, offset);
				return new JValue(dto.ToString("yyyy-MM-dd HH:mm:ss.ffffzzz"));
			}
			catch (Exception)
			{
				return JValue.CreateNull();
			}
		}

		private string ExtractStringValue(JmesPathFunctionArgument arg)
		{
			var token = arg.Token;
			if (token == null || token.Type == JTokenType.Null)
			{
				return null;
			}

			return token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
		}

		private TimeZoneInfo ConvertTimeZone(string timezoneId, TimeZoneInfo defaultTimeZone)
		{
			if (string.IsNullOrEmpty(timezoneId))
			{
				return defaultTimeZone;
			}

			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
			}
			catch
			{
				return defaultTimeZone;
			}
		}

		private DateTime TryParseDateTime(string dateTimestr, string format)
		{
			DateTime parsedDateTime = DateTime.MinValue;
			if (!string.IsNullOrEmpty(format))
			{
				if (!DateTime.TryParseExact(dateTimestr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
				{
					return parsedDateTime;
				}
			}
			else
			{
				if (!DateTime.TryParse(dateTimestr, out parsedDateTime))
				{
					return parsedDateTime;
				}
			}

			return parsedDateTime;
		}
	}
}
