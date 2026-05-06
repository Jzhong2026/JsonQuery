using DevLab.JmesPath.Functions;
using JmesPathWpfDemo.Services;
using Newtonsoft.Json.Linq;
using System;

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
               var fromTimezone = args.Length > 2 ? ExtractStringValue(args[2]) : null;
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
         var singleFromTimezone = args.Length > 2 ? ExtractStringValue(args[2]) : null;
			var singleToTimezone = args.Length > 3 ? ExtractStringValue(args[3]) : null;

			return ConvertSingleDateTime(singleDateString, singleFormat, singleFromTimezone, singleToTimezone);
		}

		private JToken ConvertSingleDateTime(string dateString, string format, string fromTimezone, string toTimezone)
		{
			try
			{
              return DateTimeConversionService.ConvertForJson(dateString, format, fromTimezone, toTimezone);
			}
           catch
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
