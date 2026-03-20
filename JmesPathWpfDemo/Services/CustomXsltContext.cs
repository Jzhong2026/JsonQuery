using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace JmesPathWpfDemo.Services
{
    public class CustomXsltContext : XsltContext
    {
        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
        {
            if (name == "join")
            {
                return new StringJoinFunction();
            }
            return null;
        }

        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            return null;
        }

        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            return 0;
        }

        public override bool PreserveWhitespace(XPathNavigator node)
        {
            return true;
        }

        public override bool Whitespace => true;
    }

    public class StringJoinFunction : IXsltContextFunction
    {
        public int Minargs => 1;
        public int Maxargs => 2;

        public XPathResultType ReturnType => XPathResultType.String;

        public XPathResultType[] ArgTypes => new[] { XPathResultType.Any, XPathResultType.String };

        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args == null || args.Length == 0 || args.Length > Maxargs)
            {
                return string.Empty;
            }

            var values = new List<string>();
            string sep = ",";

            if (args.Length == 2 && args[1] != null)
            {
                sep = args[1].ToString();
            }

            if (args[0] is XPathNodeIterator iterator)
            {
                while (iterator.MoveNext())
                {
                    values.Add(iterator.Current.Value);
                }
            }
            else if (args[0] is XPathNavigator navigator)
            {
                values.Add(navigator.Value);
            }
            else if (args[0] != null)
            {
                values.Add(args[0].ToString());
            }

            return string.Join(sep, values);
        }
    }
}
