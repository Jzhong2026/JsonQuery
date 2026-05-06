using JmesPathWpfDemo.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace JmesPathWpfDemo.Services
{
    public static class DateTimeConversionService
    {
        public static JToken ConvertForJson(string dateString, string format, string fromTimezone, string toTimezone)
        {
            if (string.IsNullOrWhiteSpace(dateString) || string.Equals(dateString.Trim(), "null", StringComparison.OrdinalIgnoreCase))
            {
                return JValue.CreateNull();
            }

            var parsedDateTime = TryParseDateTime(dateString, format);
            if (parsedDateTime == DateTime.MinValue)
            {
                throw new ArgumentException($"Json query todatetime function execution failed. Failed to parse {dateString} to datetime");
            }

            var sourceTimeZone = ConvertTimeZone(fromTimezone);
            var targetTimeZone = ConvertTimeZone(toTimezone);

            if (parsedDateTime.Kind == DateTimeKind.Unspecified)
            {
                if (sourceTimeZone == null)
                {
                    parsedDateTime = ConvertTimeToUtcUsingClientSettings(parsedDateTime);
                }
                else if (string.Equals(fromTimezone, "UTC", StringComparison.OrdinalIgnoreCase))
                {
                    parsedDateTime = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Utc);
                }
                else
                {
                    parsedDateTime = TimeZoneInfo.ConvertTimeToUtc(parsedDateTime, sourceTimeZone);
                }
            }
            else
            {
                parsedDateTime = parsedDateTime.ToUniversalTime();
            }

            if (targetTimeZone == null)
            {
                var convertedDateTime = ConvertUtcToClientTimeZone(parsedDateTime);
                return new JValue(convertedDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            }

            var explicitTargetDateTime = TimeZoneInfo.ConvertTimeFromUtc(parsedDateTime, targetTimeZone);
            var offset = targetTimeZone.GetUtcOffset(explicitTargetDateTime);
            var dto = new DateTimeOffset(explicitTargetDateTime, offset);
            return new JValue(dto.ToString("yyyy-MM-dd HH:mm:ss.ffffzzz", CultureInfo.InvariantCulture));
        }

        public static string ConvertForXPath(string dateString, string format, string fromTimezone, string toTimezone)
        {
            var result = ConvertForJson(dateString, format, fromTimezone, toTimezone);
            return result.Type == JTokenType.Null ? string.Empty : result.ToString();
        }

        public static DateTime ConvertTimeToUtcUsingClientSettings(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }

            var settings = ClientTimeZoneSettingsService.Current;
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
            var source = RespectDaylightSavings(dateTime, timeZone, settings.RespectDaylightSavings);
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), source);
        }

        public static DateTime ConvertUtcToClientTimeZone(DateTime utcDateTime)
        {
            var utc = utcDateTime.Kind == DateTimeKind.Utc ? utcDateTime : utcDateTime.ToUniversalTime();
            var settings = ClientTimeZoneSettingsService.Current;
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
            var target = RespectDaylightSavings(utc, timeZone, settings.RespectDaylightSavings);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, target);
        }

        private static TimeZoneInfo RespectDaylightSavings(DateTime dateTime, TimeZoneInfo timeZone, bool respectDaylightSavings)
        {
            if (respectDaylightSavings)
            {
                return timeZone;
            }

            var baseOffset = timeZone.BaseUtcOffset;
            return CreateFixedOffsetTimeZone(timeZone, baseOffset);
        }

        private static TimeZoneInfo CreateFixedOffsetTimeZone(TimeZoneInfo original, TimeSpan baseOffset)
        {
            var sign = baseOffset < TimeSpan.Zero ? "-" : "+";
            var absoluteOffset = baseOffset.Duration();
            var id = $"{original.Id}__Fixed__{baseOffset.Ticks}";
            var displayName = $"(UTC{sign}{absoluteOffset:hh\\:mm}) {original.StandardName}";
            return TimeZoneInfo.CreateCustomTimeZone(id, baseOffset, displayName, original.StandardName);
        }

        public static TimeZoneInfo ConvertTimeZone(string timezoneId)
        {
            try
            {
                if (string.IsNullOrEmpty(timezoneId))
                {
                    return null;
                }

                return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            }
            catch
            {
                return null;
            }
        }

        public static DateTime TryParseDateTime(string dateTimeStr, string format)
        {
            DateTime parsedDateTime;
            if (!string.IsNullOrEmpty(format))
            {
                if (!DateTime.TryParseExact(dateTimeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                {
                    return parsedDateTime;
                }
            }
            else
            {
                if (!DateTime.TryParse(dateTimeStr, out parsedDateTime))
                {
                    return parsedDateTime;
                }
            }

            return parsedDateTime;
        }
    }
}