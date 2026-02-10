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

			// Handle array input - process each element
			if (args[0].Token is JArray arrayInput)
			{
				var resultArray = new JArray();
				var format = args.Length > 1 ? ExtractStringValue(args[1]) : null;
				var fromTimezone = args.Length > 2 ? ExtractStringValue(args[2]) : "UTC";
				var toTimezone = args.Length > 3 ? ExtractStringValue(args[3]) : null;

				foreach (var item in arrayInput)
				{
					var dateString = ExtractStringValueFromToken(item);
					var converted = ConvertSingleDateTime(dateString, format, fromTimezone, toTimezone);
					resultArray.Add(converted);
				}

				return resultArray;
			}

			// Handle single value input
			var singleDateString = ExtractStringValue(args[0]);
			var singleFormat = args.Length > 1 ? ExtractStringValue(args[1]) : null;
			var singleFromTimezone = args.Length > 2 ? ExtractStringValue(args[2]) : "UTC";
			var singleToTimezone = args.Length > 3 ? ExtractStringValue(args[3]) : null;

			return ConvertSingleDateTime(singleDateString, singleFormat, singleFromTimezone, singleToTimezone);
		}

		private JToken ConvertSingleDateTime(string dateString, string format, string fromTimezone, string toTimezone)
		{
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

				// Return simple format without timezone offset
				return new JValue(convertedDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
			}
			catch (Exception)
			{
				return JValue.CreateNull();
			}
		}

		private string ExtractStringValue(JmesPathFunctionArgument arg)
		{
			var token = arg.Token;
			return ExtractStringValueFromToken(token);
		}

		private string ExtractStringValueFromToken(JToken token)
		{
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
