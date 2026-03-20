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
                    var projectDir = Path.GetFullPath(Path.Combine(assemblyDirectory, @"..\..\.."));
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
            Query = node.Path;
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

                    var query = $"{targetPath}[{filterProp}='{filterValue}']";

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

            string targetPath = "";
            var properties = new System.Collections.Generic.HashSet<string>();
            try
            {
                var targetNode = node;
                if (targetNode.Children.Count == 0 && targetNode.Attributes.Count == 0 && targetNode.Parent != null)
                {
                    targetNode = targetNode.Parent;
                }
                
                targetPath = targetNode.Parent != null ? $"{targetNode.Parent.Path}/{targetNode.Name}" : $"/{targetNode.Name}";

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

                Query = $"join({targetPath}/{prop}, '{sep}')";
            }
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