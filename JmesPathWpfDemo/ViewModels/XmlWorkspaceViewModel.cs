using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using Newtonsoft.Json;
using System;
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
                    // Add a root node because json usually is an object and DeserializeXmlNode requires one root
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
            Query = GetSortedPath(node);
        }

        public void GenerateArrayFilterQuery(XmlTreeNode node)
        {
            if (node == null) return;

            var targetNode = node;

            // If it's a leaf node/has no children/attributes, we likely want to filter the parent collection
            if (targetNode.Children.Count == 0 && targetNode.Attributes.Count == 0 && targetNode.Parent != null)
            {
                targetNode = targetNode.Parent;
            }

            var targetPath = targetNode.Parent != null ? $"{targetNode.Parent.Path}/{targetNode.Name}" : $"/{targetNode.Name}";

            var map = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // Select items in the same level under the specified parent
                var siblings = targetNode.Parent != null
                    ? targetNode.Parent.Children.Where(c => c.Name == targetNode.Name).ToList()
                    : new System.Collections.Generic.List<XmlTreeNode> { targetNode };

                foreach (var n in siblings)
                {
                    if (n.Attributes != null)
                    {
                        foreach (var attr in n.Attributes)
                        {
                            var key = attr.Name; // Already starts with '@' in XmlTreeBuilder
                            if (!key.StartsWith("@")) key = "@" + key;

                            if (!map.TryGetValue(key, out var values))
                            {
                                values = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                map[key] = values;
                            }
                            values.Add(attr.Value ?? string.Empty);
                        }
                    }

                    foreach (var child in n.Children)
                    {
                        if (child.Children.Count == 0)
                        {
                            var key = child.Name;
                            if (!map.TryGetValue(key, out var values))
                            {
                                values = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                    System.Windows.MessageBox.Show("No simple properties found to filter in this node or its siblings.", "Array Filter",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                var dialog = new Views.ArrayFilterDialog(propertyValues);
                dialog.Owner = System.Windows.Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    var filterProp = dialog.SelectedFilterProperty;
                    var filterValue = dialog.SelectedFilterValue;
                    var returnProp = dialog.SelectedReturnProperty;

                    var queryPath = GetSortedPath(targetNode, omitLastIndex: true);
                    var query = $"{queryPath}[{filterProp}='{filterValue}']";

                    if (!string.IsNullOrWhiteSpace(returnProp) && returnProp != "(Whole item)")
                    {
                        query = $"{query}/{returnProp}";
                    }

                    Query = query;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generating filter query: {ex.Message}");
            }
        }

        public void GenerateJoinQuery(XmlTreeNode node)
        {
            if (node == null) return;

            var properties = new System.Collections.Generic.HashSet<string>();
            var targetNode = node;
            if (targetNode.Children.Count == 0 && targetNode.Attributes.Count == 0 && targetNode.Parent != null)
            {
                targetNode = targetNode.Parent;
            }

            try
            {
                var siblings = targetNode.Parent != null
                    ? targetNode.Parent.Children.Where(c => c.Name == targetNode.Name).ToList()
                    : new System.Collections.Generic.List<XmlTreeNode> { targetNode };

                foreach (var n in siblings)
                {
                    if (n.Attributes != null)
                    {
                        foreach (var attr in n.Attributes)
                        {
                            var key = attr.Name;
                            if (!key.StartsWith("@")) key = "@" + key;
                            properties.Add(key);
                        }
                    }

                    foreach (var child in n.Children)
                    {
                        if (child.Children.Count == 0)
                        {
                            properties.Add(child.Name);
                        }
                    }
                }
            }
            catch { }

            var propsList = properties.OrderBy(x => x).ToList();
            if (propsList.Count == 0) return;

            var dialog = new Views.JoinQueryDialog(propsList, new System.Collections.Generic.List<Models.SavedQuery>());
            dialog.Owner = System.Windows.Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                var prop = dialog.SelectedProperty;
                var sep = dialog.SelectedSeparator ?? ", ";

                var queryPath = GetSortedPath(targetNode, omitLastIndex: true);

                Query = $"join({queryPath}/{prop}, '{sep}')";
            }
        }

        public void ConfigureArraySort(XmlTreeNode node)
        {
            if (node == null) return;

            var targetNode = node;
            if (targetNode.Children.Count == 0 && targetNode.Attributes.Count == 0 && targetNode.Parent != null)
            {
                targetNode = targetNode.Parent;
            }

            var siblings = targetNode.Parent != null
                ? targetNode.Parent.Children.Where(c => c.Name == targetNode.Name).ToList()
                : new System.Collections.Generic.List<XmlTreeNode> { targetNode };

            // For XML, any repeating element is naturally an array
            if (siblings.Count == 0 && targetNode.Parent == null) return;

            var sortKeys = new System.Collections.Generic.List<string>();
            var firstItem = siblings.FirstOrDefault();
            if (firstItem != null)
            {
                foreach (var attr in firstItem.Attributes)
                {
                    var key = attr.Name;
                    if (!key.StartsWith("@")) key = "@" + key;
                    sortKeys.Add(key);
                }
                foreach (var child in firstItem.Children)
                {
                    if (child.Children.Count == 0)
                        sortKeys.Add(child.Name);
                }
            }

            if (sortKeys.Count == 0)
            {
                System.Windows.MessageBox.Show("No sortable properties found in array items.", "Array Sort",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var dialog = new Views.ArraySortDialog(sortKeys, targetNode.SortKey, targetNode.SortAscending);
            dialog.Owner = System.Windows.Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                foreach (var sibling in siblings)
                {
                    sibling.SortKey = dialog.SelectedSortKey;
                    sibling.SortAscending = dialog.SortAscending;
                }

                if (targetNode.Parent != null)
                {
                    var parent = targetNode.Parent;
                    var sortedChildren = dialog.SortAscending 
                        ? siblings.OrderBy(s => GetNodeSortValue(s, dialog.SelectedSortKey), new NumericStringComparer()).ToList()
                        : siblings.OrderByDescending(s => GetNodeSortValue(s, dialog.SelectedSortKey), new NumericStringComparer()).ToList();

                    int firstIdx = parent.Children.IndexOf(siblings.First());
                    foreach(var sibling in siblings) {
                        parent.Children.Remove(sibling);
                    }
                    for(int i = 0; i < sortedChildren.Count; i++) {
                        parent.Children.Insert(firstIdx + i, sortedChildren[i]);
                    }
                }

                var wasSelected = node.IsSelected;
                if (wasSelected)
                {
                    node.IsSelected = false;
                    node.IsSelected = true;
                }
                else
                {
                    node.IsSelected = true;
                }

                Query = GetSortedPath(targetNode, omitLastIndex: true);
            }
        }

        public void ClearArraySort(XmlTreeNode node)
        {
            if (node == null) return;
            var targetNode = node;
            if (targetNode.Children.Count == 0 && targetNode.Attributes.Count == 0 && targetNode.Parent != null)
            {
                targetNode = targetNode.Parent;
            }

            var siblings = targetNode.Parent != null
                ? targetNode.Parent.Children.Where(c => c.Name == targetNode.Name).ToList()
                : new System.Collections.Generic.List<XmlTreeNode> { targetNode };

            foreach (var sibling in siblings)
            {
                sibling.SortKey = null;
                sibling.SortAscending = true;
            }

            Query = GetSortedPath(targetNode, omitLastIndex: true);
            RefreshXmlTree();
        }

        private class NumericStringComparer : System.Collections.Generic.IComparer<string>
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

        private string GetSortedPath(XmlTreeNode node, bool omitLastIndex = false)
        {
            if (node == null) return "";

            var steps = new System.Collections.Generic.List<XmlTreeNode>();
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
                string stepName = step.Name;
                bool isArrayElement = false;
                int index = 1;

                if (step.Parent != null)
                {
                    var siblings = step.Parent.Children.Where(c => c.Name == step.Name).ToList();
                    if (siblings.Count > 1)
                    {
                        isArrayElement = true;
                        index = siblings.IndexOf(step) + 1;
                    }
                }

                bool omitThisIndex = (omitLastIndex && i == steps.Count - 1);

                if (step.HasSortApplied && isArrayElement)
                {
                    string arrayExpr = string.IsNullOrEmpty(expr) ? $"/{stepName}" : $"{expr}/{stepName}";
                    string order = step.SortAscending ? "asc" : "desc";
                    expr = $"sortby({arrayExpr}, '{step.SortKey}', '{order}')";

                    if (!omitThisIndex) {
                        expr = $"{expr}[{index}]"; 
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(expr)) {
                        expr = "/" + stepName;
                    } else if (stepName.StartsWith("@")) {
                        expr = expr + "/" + stepName;
                    } else {
                        expr = expr + "/" + stepName;
                    }

                    if (isArrayElement && !omitThisIndex) {
                        expr = expr + $"[{index}]";
                    }
                }
            }

            return expr;
        }

        public void RefreshTree()
        {
            RefreshXmlTree();
        }

        private void RefreshXmlTree()
        {
            var nodes = _treeBuilder.BuildTree(XmlInput);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.IsExpanded = true;
                }
            }
            XmlTreeNodes = nodes;
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
                        // Use InnerXml to omit wrapper tags, fallback to Value for attributes/text 
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
                    // It's a scalar value (e.g. string, number, boolean) from functions like count(), string()
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