using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace JmesPathWpfDemo.Jmes
{
    public sealed class ToDateFunction : JmesPathFunction
    {
        public ToDateFunction()
            : base("todate", 1, true)
        {
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            if (args.Length == 0 || args.Length > 3)
            {
                return JValue.CreateNull();
            }

            // Handle array input - process each element
            if (args[0].Token is JArray arrayInput)
            {
                var resultArray = new JArray();
                var sourceFormat = args.Length > 1 ? ExtractStringValue(args[1]) : null;
                var targetFormat = args.Length > 2 ? ExtractStringValue(args[2]) : null;

                foreach (var item in arrayInput)
                {
                    var dateString = ExtractStringValueFromToken(item);
                    var converted = ConvertSingleDate(dateString, sourceFormat, targetFormat);
                    resultArray.Add(converted);
                }

                return resultArray;
            }

            // Handle single value input
            var singleDateString = ExtractStringValue(args[0]);
            var singleSourceFormat = args.Length > 1 ? ExtractStringValue(args[1]) : null;
            var singleTargetFormat = args.Length > 2 ? ExtractStringValue(args[2]) : null;

            return ConvertSingleDate(singleDateString, singleSourceFormat, singleTargetFormat);
        }

        private JToken ConvertSingleDate(string dateString, string sourceFormat, string targetFormat)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return JValue.CreateNull();
            }

            try
            {
                DateTime parsedDate;

                // Parse the date with source format (or auto-detect if empty)
                if (!string.IsNullOrWhiteSpace(sourceFormat))
                {
                    if (!DateTime.TryParseExact(dateString, sourceFormat, CultureInfo.InvariantCulture, 
                        DateTimeStyles.None, out parsedDate))
                    {
                        return JValue.CreateNull();
                    }
                }
                else
                {
                    // Auto-detect format
                    if (!DateTime.TryParse(dateString, CultureInfo.InvariantCulture, 
                        DateTimeStyles.None, out parsedDate))
                    {
                        return JValue.CreateNull();
                    }
                }

                // Format the date with target format (or use default if empty)
                string result;
                if (!string.IsNullOrWhiteSpace(targetFormat))
                {
                    result = parsedDate.ToString(targetFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    // Default format: ISO 8601 date
                    result = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                return new JValue(result);
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
    }
}
