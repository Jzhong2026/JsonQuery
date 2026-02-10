using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace JmesPathWpfDemo.Jmes
{
	public sealed class ConcatFunction : JmesPathFunction
	{
		public ConcatFunction() : base("concat", 1, true)
		{
		}

		public override JToken Execute(params JmesPathFunctionArgument[] args)
		{
			var values = args.Select(arg =>
			{
				var token = arg.Token;
				if (token == null) return string.Empty;
				return token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
			});

			var result = string.Concat(values);
			return JToken.FromObject(result);
		}
	}

	public sealed class ConcatWsFunction : JmesPathFunction
	{
		public ConcatWsFunction() : base("concat_ws", 2, true)
		{
		}

		public override JToken Execute(params JmesPathFunctionArgument[] args)
		{
			if (args == null || args.Length < 2)
			{
				return JToken.FromObject(string.Empty);
			}

			var separator = args[0].Token?.Type == JTokenType.String
				? args[0].Token.Value<string>()
				: args[0].Token?.ToString() ?? string.Empty;

			var values = args.Skip(1)
				.Select(arg =>
				{
					var token = arg.Token;
					if (token == null) return null;
					return token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
				})
				.Where(v => !string.IsNullOrEmpty(v));

			var result = string.Join(separator, values);
			return JToken.FromObject(result);
		}
	}

    public sealed class NewLineFunction : JmesPathFunction
    {
        public NewLineFunction() : base("newline", 0)
        {
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            return JToken.FromObject(System.Environment.NewLine);
        }
    }

    public sealed class IffFunction : JmesPathFunction
    {
        public IffFunction() : base("iff", 3)
        {
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var condition = args[0].Token;
            return IsTruthy(condition) ? args[1].Token : args[2].Token;
        }

        private bool IsTruthy(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return false;

            if (token.Type == JTokenType.Boolean)
                return token.Value<bool>();

            if (token.Type == JTokenType.String)
                return !string.IsNullOrEmpty(token.Value<string>());

            if (token.Type == JTokenType.Array)
                return token.HasValues;

            if (token.Type == JTokenType.Object)
                return token.HasValues;

            return true;
        }
    }

    public sealed class EqualFunction : JmesPathFunction
    {
        public EqualFunction() : base("eq", 2)
        {
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var left = args[0].Token;
            var right = args[1].Token;

            return JToken.FromObject(JToken.DeepEquals(left, right));
        }
    }

    public sealed class MergeArraysFunction : JmesPathFunction
    {
        public MergeArraysFunction() : base("merge_arrays", 1, true)
        {
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var result = new JArray();
            
            foreach (var arg in args)
            {
                if (arg.Token is JArray arr)
                {
                    foreach (var item in arr)
                    {
                        result.Add(item);
                    }
                }
                else if (arg.Token != null && arg.Token.Type != JTokenType.Null)
                {
                    result.Add(arg.Token);
                }
            }

            return result;
        }
    }

    public sealed class FlattenFunction : JmesPathFunction
    {
        public FlattenFunction() : base("flatten", 1)
        {
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var result = new JArray();
            
            if (args[0].Token is JArray arr)
            {
                FlattenRecursive(arr, result);
            }

            return result;
        }

        private void FlattenRecursive(JArray source, JArray target)
        {
            foreach (var item in source)
            {
                if (item is JArray nestedArray)
                {
                    FlattenRecursive(nestedArray, target);
                }
                else
                {
                    target.Add(item);
                }
            }
        }
    }
}
