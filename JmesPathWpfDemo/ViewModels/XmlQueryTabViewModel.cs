using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml;

namespace JmesPathWpfDemo.ViewModels
{
    public class XmlQueryTabViewModel : Screen
    {
        private readonly XmlTreeBuilder _treeBuilder = new XmlTreeBuilder();
        private readonly Action<string, string> _onCreateNewTab;
      private readonly Func<List<SavedQuery>> _getSavedQueries;
        private readonly Dictionary<XmlTreeNode, List<XmlTreeNode>> _arrayFilterSnapshots = new();
        private string _tabTitle;
        private string _xmlInput;
        private string _query;
        private string _result;
        private ObservableCollection<XmlTreeNode> _xmlTreeNodes;
        private bool _canClose;

      public XmlQueryTabViewModel(string title, string initialXml, bool canClose, Action<string, string> onCreateNewTab, Func<List<SavedQuery>> getSavedQueries = null)
        {
            _tabTitle = title;
            _xmlInput = string.IsNullOrWhiteSpace(initialXml) ? "<Root />" : initialXml;
            _canClose = canClose;
            _onCreateNewTab = onCreateNewTab;
            _getSavedQueries = getSavedQueries;
            _xmlTreeNodes = new ObservableCollection<XmlTreeNode>();
            _query = string.Empty;
            _result = string.Empty;
            RefreshXmlTree();
        }

        public string TabTitle
        {
            get => _tabTitle;
            set
            {
                _tabTitle = value;
                NotifyOfPropertyChange(() => TabTitle);
            }
        }

        public bool CanClose
        {
            get => _canClose;
            set
            {
                _canClose = value;
                NotifyOfPropertyChange(() => CanClose);
            }
        }

        public string XmlInput
        {
            get => _xmlInput;
            set
            {
                if (_xmlInput != value)
                {
                    _xmlInput = value;
                    NotifyOfPropertyChange(() => XmlInput);
                }
            }
        }

        public string Query
        {
            get => _query;
            set
            {
                if (_query != value)
                {
                    _query = value;
                    NotifyOfPropertyChange(() => Query);
                }
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                if (_result != value)
                {
                    _result = value;
                    NotifyOfPropertyChange(() => Result);
                    NotifyOfPropertyChange(() => HasValidResult);
                }
            }
        }

        public bool HasValidResult => !string.IsNullOrWhiteSpace(_result) && !_result.StartsWith("Error executing query:", StringComparison.Ordinal);

        public ObservableCollection<XmlTreeNode> XmlTreeNodes
        {
            get => _xmlTreeNodes;
            set
            {
                _xmlTreeNodes = value;
                NotifyOfPropertyChange(() => XmlTreeNodes);
            }
        }

        public void RefreshTree()
        {
            RefreshXmlTree();
        }

        public void OnNodeSelected(XmlTreeNode node)
        {
            if (node == null) return;
            Query = BuildXPath(node);
        }

        public void CreateTabFromNode(XmlTreeNode node)
        {
            try
            {
                var xml = GetNodeXml(node);
                if (string.IsNullOrWhiteSpace(xml))
                {
                    return;
                }

               _onCreateNewTab?.Invoke(GetNewTabTitle(node), xml);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating tab from node: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateTabFromResult()
        {
            if (string.IsNullOrWhiteSpace(Result))
            {
                return;
            }

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(Result);
                _onCreateNewTab?.Invoke("Result", doc.OuterXml);
            }
            catch
            {
                MessageBox.Show("Result is not valid XML and cannot be opened as a new tab.", "Invalid XML", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void GenerateArrayFilterQuery(XmlTreeNode node)
        {
            if (node == null) return;

            var arrayNode = ResolveArrayNode(node);
            if (arrayNode == null || !arrayNode.IsArrayNode) return;

            var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var item in arrayNode.Children)
                {
                    if (item.Attributes != null)
                    {
                        foreach (var attr in item.Attributes)
                        {
                            var key = attr.Name;
                            if (!key.StartsWith("@")) key = "@" + key;

                            if (!map.TryGetValue(key, out var values))
                            {
                                values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                map[key] = values;
                            }
                            values.Add(attr.Value ?? string.Empty);
                        }
                    }

                    foreach (var child in item.Children)
                    {
                        if (!child.IsArrayNode && child.Children.Count == 0)
                        {
                            var key = child.Name;
                            if (!map.TryGetValue(key, out var values))
                            {
                                values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                map[key] = values;
                            }
                            values.Add(child.Value ?? string.Empty);
                        }
                    }
                }

                var propertyValues = map.OrderBy(kvp => kvp.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(v => v).ToList(), StringComparer.OrdinalIgnoreCase);

                if (propertyValues.Count == 0)
                {
                    MessageBox.Show("No simple properties found to filter in array items.", "Array Filter", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new Views.ArrayFilterDialog(propertyValues);
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    var filterProp = dialog.SelectedFilterProperty;
                    var filterValue = dialog.SelectedFilterValue;
                    var returnProp = dialog.SelectedReturnProperty;
                    var filterExpression = $"{filterProp}='{filterValue}'";

                    ApplyArrayFilter(arrayNode, filterProp, filterValue);
                    arrayNode.FilterExpression = filterExpression;

                    var query = BuildXPath(arrayNode);
                    if (!string.IsNullOrWhiteSpace(returnProp) && returnProp != "(Whole item)")
                    {
                        query = $"{query}/{returnProp}";
                    }

                    Query = query;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating filter query: {ex.Message}");
            }
        }

        public void ClearArrayFilter(XmlTreeNode node)
        {
            if (node == null) return;

            var arrayNode = ResolveArrayNode(node);
            if (arrayNode == null || !arrayNode.IsArrayNode) return;

            if (_arrayFilterSnapshots.TryGetValue(arrayNode, out var originalChildren))
            {
                arrayNode.Children.Clear();
                for (int i = 0; i < originalChildren.Count; i++)
                {
                    var child = originalChildren[i];
                    child.Name = $"[{i + 1}]";
                    child.Path = $"{arrayNode.Path}[{i + 1}]";
                    RebuildDescendantPaths(child);
                    arrayNode.Children.Add(child);
                }
                _arrayFilterSnapshots.Remove(arrayNode);
            }

            arrayNode.FilterExpression = null;
            Query = BuildXPath(arrayNode);
        }

        public void ConfigureArraySort(XmlTreeNode node)
        {
            if (node == null) return;

            var arrayNode = ResolveArrayNode(node);
            if (arrayNode == null || !arrayNode.IsArrayNode || arrayNode.Children.Count == 0) return;

            var firstItem = arrayNode.Children.FirstOrDefault();
            if (firstItem == null) return;

            var sortKeys = new List<string>();
            foreach (var attr in firstItem.Attributes)
            {
                var key = attr.Name;
                if (!key.StartsWith("@")) key = "@" + key;
                sortKeys.Add(key);
            }
            foreach (var child in firstItem.Children)
            {
                if (!child.IsArrayNode && child.Children.Count == 0)
                    sortKeys.Add(child.Name);
            }

            if (sortKeys.Count == 0)
            {
                MessageBox.Show("No sortable properties found in array items.", "Array Sort", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Views.ArraySortDialog(sortKeys, arrayNode.SortKey, arrayNode.SortAscending);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                arrayNode.SortKey = dialog.SelectedSortKey;
                arrayNode.SortAscending = dialog.SortAscending;

                var sorted = dialog.SortAscending
                    ? arrayNode.Children.OrderBy(s => GetNodeSortValue(s, dialog.SelectedSortKey), new NumericStringComparer()).ToList()
                    : arrayNode.Children.OrderByDescending(s => GetNodeSortValue(s, dialog.SelectedSortKey), new NumericStringComparer()).ToList();

                arrayNode.Children.Clear();
                for (int i = 0; i < sorted.Count; i++)
                {
                    sorted[i].Name = $"[{i + 1}]";
                    sorted[i].Path = $"{arrayNode.Path}[{i + 1}]";
                    RebuildDescendantPaths(sorted[i]);
                    arrayNode.Children.Add(sorted[i]);
                }

                Query = BuildXPath(arrayNode);
            }
        }

        public void ClearArraySort(XmlTreeNode node)
        {
            if (node == null) return;

            var arrayNode = ResolveArrayNode(node);
            if (arrayNode == null || !arrayNode.IsArrayNode) return;

            arrayNode.SortKey = null;
            arrayNode.SortAscending = true;
            Query = BuildXPath(arrayNode);
            RefreshXmlTree();
        }

        public void GenerateJoinQuery(XmlTreeNode node)
        {
            if (node == null) return;

            var arrayNode = ResolveArrayNode(node);
            if (arrayNode == null || !arrayNode.IsArrayNode) return;

            var properties = new HashSet<string>();
            foreach (var item in arrayNode.Children)
            {
                if (item.Attributes != null)
                {
                    foreach (var attr in item.Attributes)
                    {
                        var key = attr.Name;
                        if (!key.StartsWith("@")) key = "@" + key;
                        properties.Add(key);
                    }
                }

                foreach (var child in item.Children)
                {
                    if (!child.IsArrayNode && child.Children.Count == 0)
                    {
                        properties.Add(child.Name);
                    }
                }
            }

            var propsList = properties.OrderBy(x => x).ToList();
            if (propsList.Count == 0) return;

          var dialog = new Views.JoinQueryDialog(propsList, _getSavedQueries?.Invoke() ?? new List<SavedQuery>());
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                var prop = dialog.SelectedProperty;
                var sep = dialog.SelectedSeparator ?? ", ";
                var queryPath = BuildXPath(arrayNode);

                if (dialog.UsePipeline && dialog.SelectedSavedQuery != null)
                {
                    var pipelineExpression = NormalizeSavedQueryForPipeline(arrayNode.Name, dialog.SelectedSavedQuery.Expression);
                    var pipelineArg = BuildXPathStringLiteral(pipelineExpression);
                    var separatorArg = BuildXPathStringLiteral(sep);
                    Query = $"joinquery({queryPath}, {pipelineArg}, {separatorArg})";
                }
                else
                {
                    Query = $"join({queryPath}/{prop}, '{sep}')";
                }
            }
        }

        public void ShowToDateTimeDialog(XmlTreeNode node)
        {
            if (node == null || node.IsArray) return;

            var inputDialog = new Views.ToDateTimeDialog();
            inputDialog.Owner = Application.Current.MainWindow;
            if (inputDialog.ShowDialog() == true)
            {
                var format = inputDialog.Format;
                var fromTz = inputDialog.FromTimeZone;
                var toTz = inputDialog.ToTimeZone;
                var path = BuildXPath(node);
                var formatArg = string.IsNullOrWhiteSpace(format) ? "''" : $"'{format}'";
                var fromTzArg = string.IsNullOrWhiteSpace(fromTz) ? "''" : $"'{fromTz}'";
                var toTzArg = string.IsNullOrWhiteSpace(toTz) ? "''" : $"'{toTz}'";
                Query = $"todatetime({path}, {formatArg}, {fromTzArg}, {toTzArg})";
            }
        }

        public void Execute()
        {
            if (string.IsNullOrWhiteSpace(XmlInput) || string.IsNullOrWhiteSpace(Query))
            {
                Result = "Empty input or query.";
                return;
            }

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(XmlInput);

                var nav = doc.CreateNavigator();
                var expr = nav.Compile(Query);
                expr.SetContext(new CustomXsltContext());
                var result = nav.Evaluate(expr);

                if (result is System.Xml.XPath.XPathNodeIterator iterator)
                {
                    if (iterator.Count == 0)
                    {
                        Result = "No elements found.";
                        return;
                    }

                    var sb = new System.Text.StringBuilder();
                    while (iterator.MoveNext())
                    {
                        var content = string.IsNullOrEmpty(iterator.Current.InnerXml) ? iterator.Current.Value : iterator.Current.InnerXml;
                        sb.Append(content);
                        if (iterator.CurrentPosition != iterator.Count)
                        {
                            sb.AppendLine();
                        }
                    }

                    Result = sb.ToString();
                }
                else if (result is System.Xml.XPath.XPathNavigator navResult)
                {
                    Result = navResult.Value;
                }
                else
                {
                    Result = result?.ToString() ?? "null";
                }
            }
            catch (Exception ex)
            {
                Result = $"Error executing query:\n{ex.Message}";
            }
        }

        private void RefreshXmlTree()
        {
            _arrayFilterSnapshots.Clear();
            var nodes = _treeBuilder.BuildTree(XmlInput);
            if (nodes != null)
            {
                ExpandAll(nodes);
            }
            XmlTreeNodes = nodes;
        }

        private void ExpandAll(IEnumerable<XmlTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.IsExpanded = true;
                if (node.Children != null && node.Children.Count > 0)
                {
                    ExpandAll(node.Children);
                }
            }
        }

        private XmlTreeNode ResolveArrayNode(XmlTreeNode node)
        {
            if (node == null) return null;
            if (node.IsArrayNode) return node;
            if (node.Parent != null && node.Parent.IsArrayNode) return node.Parent;
            return node;
        }

        private string BuildXPath(XmlTreeNode node)
        {
            if (node == null) return string.Empty;

            var steps = new List<XmlTreeNode>();
            var current = node;
            while (current != null)
            {
                steps.Insert(0, current);
                current = current.Parent;
            }

            string expr = string.Empty;
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step.IsArrayNode)
                {
                    expr = string.IsNullOrEmpty(expr) ? $"/{step.Name}" : $"{expr}/{step.Name}";
                    if (step.HasFilterApplied)
                    {
                        expr = $"{expr}[{step.FilterExpression}]";
                    }
                    if (step.HasSortApplied)
                    {
                        var order = step.SortAscending ? "asc" : "desc";
                        expr = $"sortby({expr}, '{step.SortKey}', '{order}')";
                    }
                }
                else if (step.Parent != null && step.Parent.IsArrayNode)
                {
                    var index = step.Parent.Children.IndexOf(step) + 1;
                    expr = $"{expr}[{index}]";
                }
                else
                {
                    expr = string.IsNullOrEmpty(expr) ? $"/{step.Name}" : $"{expr}/{step.Name}";
                }
            }

            return expr;
        }

     private string GetNodeXml(XmlTreeNode node)
        {
         if (node == null)
            {
                return null;
            }

            var document = new XmlDocument();
            var root = document.CreateElement("Root");
            document.AppendChild(root);

            if (node.IsArrayNode)
            {
             foreach (var item in node.Children)
                {
                    var element = CreateXmlElement(document, node.Name, item);
                    root.AppendChild(element);
                }
            }
            else if (node.Parent != null && node.Parent.IsArrayNode)
            {
                var element = CreateXmlElement(document, node.Parent.Name, node);
                root.AppendChild(element);
            }
            else
            {
                var element = CreateXmlElement(document, node.Name, node);
                root.AppendChild(element);
            }

            return document.OuterXml;
        }

        private string GetNewTabTitle(XmlTreeNode node)
        {
            if (node == null)
            {
                return "Node";
            }

            if (node.Parent != null && node.Parent.IsArrayNode)
            {
                return $"{node.Parent.Name} {node.Name}";
            }

            return node.Name;
        }

        private XmlElement CreateXmlElement(XmlDocument document, string elementName, XmlTreeNode node)
        {
            var element = document.CreateElement(elementName);

            foreach (var attribute in node.Attributes)
            {
                var attr = document.CreateAttribute(attribute.Name.TrimStart('@'));
                attr.Value = attribute.Value ?? string.Empty;
                element.Attributes.Append(attr);
            }

            var simpleChildren = node.Children.Where(c => !c.IsArrayNode).ToList();
            var arrayChildren = node.Children.Where(c => c.IsArrayNode).ToList();

            if (!arrayChildren.Any() && !simpleChildren.Any() && !string.IsNullOrEmpty(node.Value))
            {
                element.InnerText = node.Value;
                return element;
            }

            foreach (var child in simpleChildren)
            {
                var childElement = CreateXmlElement(document, child.Name, child);
                element.AppendChild(childElement);
            }

            foreach (var arrayChild in arrayChildren)
            {
                foreach (var item in arrayChild.Children)
                {
                    var childElement = CreateXmlElement(document, arrayChild.Name, item);
                    element.AppendChild(childElement);
                }
            }

            return element;
        }

        private void ApplyArrayFilter(XmlTreeNode arrayNode, string filterProp, string filterValue)
        {
            if (arrayNode == null || !arrayNode.IsArrayNode) return;
            if (!_arrayFilterSnapshots.ContainsKey(arrayNode))
            {
                _arrayFilterSnapshots[arrayNode] = arrayNode.Children.ToList();
            }

            var source = _arrayFilterSnapshots[arrayNode];
            var filtered = source.Where(item => IsItemMatch(item, filterProp, filterValue)).ToList();
            arrayNode.Children.Clear();
            for (int i = 0; i < filtered.Count; i++)
            {
                filtered[i].Name = $"[{i + 1}]";
                filtered[i].Path = $"{arrayNode.Path}[{i + 1}]";
                RebuildDescendantPaths(filtered[i]);
                arrayNode.Children.Add(filtered[i]);
            }
        }

        private void RebuildDescendantPaths(XmlTreeNode node)
        {
            if (node == null) return;
            foreach (var attribute in node.Attributes)
            {
                attribute.Parent = node;
                attribute.Path = $"{node.Path}/@{attribute.Name.TrimStart('@')}";
            }
            foreach (var child in node.Children)
            {
                child.Parent = node;
                if (child.IsArrayNode)
                {
                    child.Path = $"{node.Path}/{child.Name}";
                }
                else if (node.IsArrayNode && child.Name.StartsWith("[") && child.Name.EndsWith("]"))
                {
                    child.Path = $"{node.Path}{child.Name}";
                }
                else
                {
                    child.Path = $"{node.Path}/{child.Name}";
                }
                RebuildDescendantPaths(child);
            }
        }

        private bool IsItemMatch(XmlTreeNode item, string filterProp, string filterValue)
        {
            if (filterProp.StartsWith("@"))
            {
                var attr = item.Attributes?.FirstOrDefault(a => a.Name == filterProp || a.Name == filterProp.TrimStart('@'));
                return attr != null && string.Equals(attr.Value, filterValue, StringComparison.OrdinalIgnoreCase);
            }

            var child = item.Children?.FirstOrDefault(c => c.Name == filterProp);
            return child != null && string.Equals(child.Value, filterValue, StringComparison.OrdinalIgnoreCase);
        }

        private class NumericStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (double.TryParse(x, out double numX) && double.TryParse(y, out double numY))
                {
                    return numX.CompareTo(numY);
                }
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
            }
        }

        private string GetNodeSortValue(XmlTreeNode node, string sortKey)
        {
            if (string.IsNullOrEmpty(sortKey)) return node.Value ?? string.Empty;
            if (sortKey.StartsWith("@"))
            {
                var attr = node.Attributes.FirstOrDefault(a => a.Name == sortKey);
                return attr?.Value ?? string.Empty;
            }

            var child = node.Children.FirstOrDefault(c => c.Name == sortKey);
            return child?.Value ?? string.Empty;
        }

        private string NormalizeSavedQueryForPipeline(string arrayElementName, string queryExpression)
        {
            if (string.IsNullOrWhiteSpace(queryExpression))
            {
                return ".";
            }

            var normalized = queryExpression.Trim();
            var rootPrefix = $"/Root/{arrayElementName}/";
            var rootNode = $"/Root/{arrayElementName}";

            normalized = normalized.Replace(rootPrefix, string.Empty, StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace($"{rootNode}[1]/", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (string.Equals(normalized, rootNode, StringComparison.OrdinalIgnoreCase) || string.Equals(normalized, "/Root", StringComparison.OrdinalIgnoreCase))
            {
                return ".";
            }

            return normalized;
        }

        private string BuildXPathStringLiteral(string value)
        {
            value ??= string.Empty;
            if (!value.Contains("'"))
            {
                return $"'{value}'";
            }

            if (!value.Contains("\""))
            {
                return $"\"{value}\"";
            }

            var parts = value.Split('\'');
            var segments = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    segments.Add($"'{parts[i]}'");
                }

                if (i < parts.Length - 1)
                {
                    segments.Add("\"'\"");
                }
            }

            return $"concat({string.Join(", ", segments)})";
        }
    }
}