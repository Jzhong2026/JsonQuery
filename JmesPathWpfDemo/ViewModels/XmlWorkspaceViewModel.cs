using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows;
using System.Xml;

namespace JmesPathWpfDemo.ViewModels
{
    public class XmlWorkspaceViewModel : Screen
    {
        private string _currentView = "XmlQuery";
        private string _xmlInput;
        private string _query;
        private string _result;
        private ObservableCollection<XmlTreeNode> _xmlTreeNodes;
        private readonly XmlTreeBuilder _treeBuilder = new XmlTreeBuilder();
        private readonly Dictionary<XmlTreeNode, List<XmlTreeNode>> _arrayFilterSnapshots = new();

        public XmlWorkspaceViewModel()
        {
            _xmlTreeNodes = new ObservableCollection<XmlTreeNode>();

            try
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                var samplePath = Path.Combine(assemblyDirectory, "sample.json");

                if (!File.Exists(samplePath))
                {
                    var projectDir = Path.GetFullPath(Path.Combine(assemblyLocation, @"..\..\.."));
                    var projectSamplePath = Path.Combine(projectDir, "sample.json");
                    if (File.Exists(projectSamplePath))
                    {
                        samplePath = projectSamplePath;
                    }
                }

                if (File.Exists(samplePath))
                {
                    var jsonStr = File.ReadAllText(samplePath);
                    var doc = JsonConvert.DeserializeXmlNode(jsonStr, "Root");
                    using (var stringWriter = new StringWriter())
                    using (var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                    {
                        doc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        _xmlInput = stringWriter.GetStringBuilder().ToString();
                    }
                }
                else
                {
                    _xmlInput = "<Root></Root>";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sample.json and generating XML: {ex.Message}");
                _xmlInput = "<Root/>";
            }

            _query = "/Root";
            _result = "";
            RefreshXmlTree();
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
                }
            }
        }

        public ObservableCollection<XmlTreeNode> XmlTreeNodes
        {
            get => _xmlTreeNodes;
            set
            {
                _xmlTreeNodes = value;
                NotifyOfPropertyChange(() => XmlTreeNodes);
            }
        }

        public string CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                NotifyOfPropertyChange(() => CurrentView);
            }
        }

        public void OnNodeSelected(XmlTreeNode node)
        {
            if (node == null) return;
            Query = BuildXPathWithActiveFilters(node);
        }

        /// <summary>
        /// Resolves the target array node. If the user right-clicked an item inside an array,
        /// navigate up to the virtual array grouping node.
        /// </summary>
        private XmlTreeNode ResolveArrayNode(XmlTreeNode node)
        {
            if (node == null) return null;
            if (node.IsArrayNode) return node;
            if (node.Parent != null && node.Parent.IsArrayNode) return node.Parent;
            return node;
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
                    MessageBox.Show("No simple properties found to filter in array items.", "Array Filter",
                        MessageBoxButton.OK, MessageBoxImage.Information);
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

                    var query = BuildXPathWithActiveFilters(arrayNode);

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
            Query = BuildXPathWithActiveFilters(arrayNode);
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

            var dialog = new Views.JoinQueryDialog(propsList, new List<Models.SavedQuery>());
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                var prop = dialog.SelectedProperty;
                var sep = dialog.SelectedSeparator ?? ", ";
                var queryPath = BuildXPathWithActiveFilters(arrayNode);
                Query = $"join({queryPath}/{prop}, '{sep}')";
            }
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
                MessageBox.Show("No sortable properties found in array items.", "Array Sort",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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

                Query = BuildXPathWithActiveFilters(arrayNode);
            }
        }

        public void ClearArraySort(XmlTreeNode node)
        {
            if (node == null) return;

            var arrayNode = ResolveArrayNode(node);
            if (arrayNode == null || !arrayNode.IsArrayNode) return;

            arrayNode.SortKey = null;
            arrayNode.SortAscending = true;

            Query = BuildXPathWithActiveFilters(arrayNode);
            RefreshXmlTree();
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
                var path = BuildXPathWithActiveFilters(node);
                var formatArg = string.IsNullOrWhiteSpace(format) ? "''" : $"'{format}'";
                var fromTzArg = string.IsNullOrWhiteSpace(fromTz) ? "''" : $"'{fromTz}'";
                var toTzArg = string.IsNullOrWhiteSpace(toTz) ? "''" : $"'{toTz}'";
                var query = $"todatetime({path}, {formatArg}, {fromTzArg}, {toTzArg})";
                Query = query;
            }
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
            if (string.IsNullOrEmpty(sortKey)) return node.Value ?? "";
            if (sortKey.StartsWith("@"))
            {
                var attr = node.Attributes.FirstOrDefault(a => a.Name == sortKey);
                return attr?.Value ?? "";
            }
            else
            {
                var child = node.Children.FirstOrDefault(c => c.Name == sortKey);
                return child?.Value ?? "";
            }
        }

        /// <summary>
        /// Build an XPath expression for the given node, walking up the tree.
        /// Array grouping nodes produce the element name; their children produce [index].
        /// </summary>
        private string BuildXPath(XmlTreeNode node)
        {
            if (node == null) return "";

            var steps = new List<XmlTreeNode>();
            var current = node;
            while (current != null)
            {
                steps.Insert(0, current);
                current = current.Parent;
            }

            string expr = "";
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];

                if (step.IsArrayNode)
                {
                    // Virtual array node → just append the element name
                    expr = string.IsNullOrEmpty(expr) ? $"/{step.Name}" : $"{expr}/{step.Name}";

                    if (step.HasFilterApplied)
                    {
                        expr = $"{expr}[{step.FilterExpression}]";
                    }

                    if (step.HasSortApplied)
                    {
                        string order = step.SortAscending ? "asc" : "desc";
                        expr = $"sortby({expr}, '{step.SortKey}', '{order}')";
                    }
                }
                else if (step.Parent != null && step.Parent.IsArrayNode)
                {
                    // Child of array node → append [index]
                    var index = step.Parent.Children.IndexOf(step) + 1;
                    expr = $"{expr}[{index}]";
                }
                else
                {
                    if (string.IsNullOrEmpty(expr))
                        expr = "/" + step.Name;
                    else
                        expr = expr + "/" + step.Name;
                }
            }

            return expr;
        }

        private string BuildXPathWithActiveFilters(XmlTreeNode node)
        {
            return BuildXPath(node);
        }

        private string ApplyActiveArrayFilters(XmlTreeNode selectedNode, string query)
        {
            if (selectedNode == null || string.IsNullOrWhiteSpace(query))
            {
                return query;
            }

            var filteredArrays = FindArrayAncestorsWithFilter(selectedNode);
            if (selectedNode.IsArrayNode && selectedNode.HasFilterApplied)
            {
                filteredArrays.Add(selectedNode);
            }

            if (filteredArrays.Count == 0)
            {
                return query;
            }

            foreach (var arrayNode in filteredArrays.OrderByDescending(a => a.Path.Length))
            {
                if (string.IsNullOrWhiteSpace(arrayNode.FilterExpression))
                {
                    continue;
                }

                query = ReplaceArrayPrefixWithFilter(query, arrayNode.Path, arrayNode.FilterExpression);
            }

            return query;
        }

        private List<XmlTreeNode> FindArrayAncestorsWithFilter(XmlTreeNode node)
        {
            var result = new List<XmlTreeNode>();
            var current = node?.Parent;

            while (current != null)
            {
                if (current.IsArrayNode && current.HasFilterApplied)
                {
                    result.Add(current);
                }

                current = current.Parent;
            }

            return result;
        }

        private string ReplaceArrayPrefixWithFilter(string query, string arrayPath, string filterExpression)
        {
            if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(arrayPath) || string.IsNullOrWhiteSpace(filterExpression))
            {
                return query;
            }

            var sortPrefix = $"sortby({arrayPath},";
            if (query.Contains(sortPrefix, StringComparison.Ordinal))
            {
                return query.Replace(sortPrefix, $"sortby({arrayPath}[{filterExpression}],", StringComparison.Ordinal);
            }

            var targetPrefix = $"{arrayPath}[";
            if (query.StartsWith(targetPrefix, StringComparison.Ordinal))
            {
                var closeIndex = query.IndexOf(']', targetPrefix.Length);
                if (closeIndex > -1)
                {
                    var remainder = query.Substring(closeIndex + 1);
                    return $"{arrayPath}[{filterExpression}]{remainder}";
                }
            }

            if (query.StartsWith(arrayPath, StringComparison.Ordinal))
            {
                var remainder = query.Substring(arrayPath.Length);
                return $"{arrayPath}[{filterExpression}]{remainder}";
            }

            return query;
        }

        public void RefreshTree()
        {
            RefreshXmlTree();
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
                    ExpandAll(node.Children);
            }
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
            if (node == null)
            {
                return;
            }

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
            else
            {
                var child = item.Children?.FirstOrDefault(c => c.Name == filterProp);
                return child != null && string.Equals(child.Value, filterValue, StringComparison.OrdinalIgnoreCase);
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

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    while (iterator.MoveNext())
                    {
                        string content = string.IsNullOrEmpty(iterator.Current.InnerXml) ? iterator.Current.Value : iterator.Current.InnerXml;
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
                    try
                    {
                        Result = navResult.ValueAsDateTime.ToString();
                    }
                    catch (Exception)
                    {
                        Result = navResult.Value;
                    }
                }
                else
                {
                    Result = result != null ? result.ToString() : "null";
                }
            }
            catch (Exception ex)
            {
                Result = $"Error executing query:\n{ex.Message}";
            }
        }
    }
}