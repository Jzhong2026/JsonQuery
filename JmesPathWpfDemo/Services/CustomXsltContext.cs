using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Xml.Xsl;
using JmesPathWpfDemo.Services; // Added service namespace

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
            if (name == "sort")
            {
                return new SortFunction();
            }
            if (name == "sortby")
            {
                return new SortByFunction();
            }
            if (name == "todatetime")
            {
                return new ToDateTimeXsltFunction();
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

    public class SortFunction : IXsltContextFunction
    {
        public int Minargs => 3;
        public int Maxargs => 3;

        public XPathResultType ReturnType => XPathResultType.NodeSet;

        public XPathResultType[] ArgTypes => new[] { XPathResultType.NodeSet, XPathResultType.String, XPathResultType.String };

        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args == null || args.Length < 3) return null;

            var iterator = args[0] as XPathNodeIterator;
            if (iterator == null) return null;

            string sortKey = args[1]?.ToString();
            string sortOrder = args[2]?.ToString()?.ToLower() ?? "asc";
            bool isAsc = sortOrder != "desc";

            var nodes = new List<XPathNavigator>();
            while (iterator.MoveNext())
            {
                nodes.Add(iterator.Current.Clone());
            }

            nodes.Sort((a, b) =>
            {
                string valA = GetSortValue(a, sortKey);
                string valB = GetSortValue(b, sortKey);
                
                // Try numeric sort if both parse as double
                if (double.TryParse(valA, out double numA) && double.TryParse(valB, out double numB))
                {
                    return isAsc ? numA.CompareTo(numB) : numB.CompareTo(numA);
                }

                int cmp = string.Compare(valA, valB, StringComparison.OrdinalIgnoreCase);
                return isAsc ? cmp : -cmp;
            });

            return new NodeIteratorList(nodes);
        }

        private string GetSortValue(XPathNavigator nav, string key)
        {
            if (string.IsNullOrEmpty(key)) return nav.Value;

            var clone = nav.Clone();
            if (key.StartsWith("@"))
            {
                string attrName = key.Substring(1);
                if (clone.MoveToAttribute(attrName, string.Empty))
                    return clone.Value;
            }
            else
            {
                if (clone.MoveToChild(key, string.Empty))
                    return clone.Value;
            }
            return string.Empty;
        }
    }

    public class SortByFunction : IXsltContextFunction
    {
        public int Minargs => 2;
        public int Maxargs => 3;

        public XPathResultType ReturnType => XPathResultType.NodeSet;

        public XPathResultType[] ArgTypes => new[] { XPathResultType.NodeSet, XPathResultType.String, XPathResultType.String };

        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args == null || args.Length < 2) return null;

            var iterator = args[0] as XPathNodeIterator;
            if (iterator == null) return null;

            string sortKey = args[1]?.ToString();
            string sortOrder = args.Length > 2 ? (args[2]?.ToString()?.ToLower() ?? "asc") : "asc";
            bool isAsc = sortOrder != "desc";

            var nodes = new List<XPathNavigator>();
            while (iterator.MoveNext())
            {
                nodes.Add(iterator.Current.Clone());
            }

            nodes.Sort((a, b) =>
            {
                string valA = GetSortValue(a, sortKey);
                string valB = GetSortValue(b, sortKey);

                if (double.TryParse(valA, out double numA) && double.TryParse(valB, out double numB))
                {
                    return isAsc ? numA.CompareTo(numB) : numB.CompareTo(numA);
                }

                int cmp = string.Compare(valA, valB, StringComparison.OrdinalIgnoreCase);
                return isAsc ? cmp : -cmp;
            });

            return new NodeIteratorList(nodes);
        }

        private string GetSortValue(XPathNavigator nav, string key)
        {
            if (string.IsNullOrEmpty(key)) return nav.Value;

            var clone = nav.Clone();
            if (key.StartsWith("@"))
            {
                string attrName = key.Substring(1);
                if (clone.MoveToAttribute(attrName, string.Empty))
                    return clone.Value;
            }
            else
            {
                if (clone.MoveToChild(key, string.Empty))
                    return clone.Value;
            }
            return string.Empty;
        }
    }

    public class ToDateTimeXsltFunction : IXsltContextFunction
    {
        public int Minargs => 1;
        public int Maxargs => 4;

        public XPathResultType ReturnType => XPathResultType.String;

        public XPathResultType[] ArgTypes => new[] { XPathResultType.Any, XPathResultType.String, XPathResultType.String, XPathResultType.String };

        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args == null || args.Length == 0) return string.Empty;

            string dateStr = string.Empty;
            if (args[0] is XPathNodeIterator iterator)
            {
                if (iterator.MoveNext())
                    dateStr = iterator.Current.Value;
            }
            else if (args[0] is XPathNavigator navigator)
            {
                dateStr = navigator.Value;
            }
            else if (args[0] != null)
            {
                dateStr = args[0].ToString();
            }

            if (string.IsNullOrWhiteSpace(dateStr)) return string.Empty;

         string format = args.Length > 1 && args[1] != null ? args[1].ToString() : null;
            string fromTz = args.Length > 2 && args[2] != null ? args[2].ToString() : null;
            string toTz = args.Length > 3 && args[3] != null ? args[3].ToString() : null;

            try
            {
             return DateTimeConversionService.ConvertForXPath(dateStr, format, fromTz, toTz);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public class NodeIteratorList : XPathNodeIterator
    {
        private List<XPathNavigator> _nodes;
        private int _index;

        public NodeIteratorList(List<XPathNavigator> nodes)
        {
            _nodes = nodes;
            _index = -1;
        }

        public override XPathNodeIterator Clone()
        {
            return new NodeIteratorList(_nodes) { _index = this._index };
        }

        public override bool MoveNext()
        {
            if (_index + 1 < _nodes.Count)
            {
                _index++;
                return true;
            }
            return false;
        }

        public override XPathNavigator Current => _index >= 0 && _index < _nodes.Count ? _nodes[_index] : null;

        public override int CurrentPosition => _index + 1;

        public override int Count => _nodes.Count;
    }
}
