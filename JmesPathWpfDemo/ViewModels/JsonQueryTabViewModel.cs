using Caliburn.Micro;
using DevLab.JmesPath;
using JmesPathWpfDemo.Jmes;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using JmesPathWpfDemo.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace JmesPathWpfDemo.ViewModels
{
    public class JsonQueryTabViewModel : Screen
    {
        private readonly JmesPath _engine = new();
        private readonly JsonTreeBuilder _treeBuilder = new();
        private string _tabTitle;
        private string _jsonInput;
        private string _query;
        private string _result;
        private int _selectedTabIndex;
        private ObservableCollection<JsonTreeNode> _jsonTreeNodes;
        private JsonTreeNode _currentSelectedNode;
        private bool _canClose;
        private readonly Action<string, string> _onCreateNewTab;

        public JsonQueryTabViewModel(
            string title, 
            string initialJson, 
            bool canClose,
            Action<string, string> onCreateNewTab)
        {
            _tabTitle = title;
            _jsonInput = initialJson ?? "{}";
            _canClose = canClose;
            _onCreateNewTab = onCreateNewTab;

            // Register functions
            _engine.FunctionRepository.Register<ConcatFunction>();
            _engine.FunctionRepository.Register<ConcatWsFunction>();
            _engine.FunctionRepository.Register<ToDateTimeFunction>();
            _engine.FunctionRepository.Register<ToDateFunction>();
            _engine.FunctionRepository.Register<NewLineFunction>();
            _engine.FunctionRepository.Register<IffFunction>();
            _engine.FunctionRepository.Register<EqualFunction>();
            _engine.FunctionRepository.Register<MergeArraysFunction>();
            _engine.FunctionRepository.Register<FlattenFunction>();

            _jsonTreeNodes = new ObservableCollection<JsonTreeNode>();
            _query = "";
            _result = "";

            RefreshJsonTree();
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

        public string JsonInput
        {
            get => _jsonInput;
            set
            {
                if (_jsonInput != value)
                {
                    _jsonInput = value;
                    NotifyOfPropertyChange(() => JsonInput);
                }
            }
        }

        public string Query
        {
            get => _query;
            set
            {
                _query = value;
                NotifyOfPropertyChange(() => Query);
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                NotifyOfPropertyChange(() => Result);
                NotifyOfPropertyChange(() => HasValidResult);
            }
        }

        public bool HasValidResult
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_result) || _result == "null")
                    return false;

                try
                {
                    // Try to parse as JSON to verify it's valid
                    JToken.Parse(_result);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                NotifyOfPropertyChange(() => SelectedTabIndex);
            }
        }

        public ObservableCollection<JsonTreeNode> JsonTreeNodes
        {
            get => _jsonTreeNodes;
            set
            {
                _jsonTreeNodes = value;
                NotifyOfPropertyChange(() => JsonTreeNodes);
            }
        }

        public void Execute()
        {
            try
            {
                var jsonResult = _engine.Transform(JsonInput, Query);
                if (string.IsNullOrEmpty(jsonResult))
                {
                    Result = "null";
                    return;
                }

                try
                {
                    var token = JToken.Parse(jsonResult);
                    if (token.Type == JTokenType.String)
                    {
                        Result = token.Value<string>();
                    }
                    else
                    {
                        Result = token.ToString(Formatting.Indented);
                    }
                }
                catch
                {
                    Result = jsonResult;
                }
            }
            catch (Exception)
            {
                Result = "null";
            }
        }

        public void RefreshTree()
        {
            RefreshJsonTree();
        }

        public void CreateTabFromNode(JsonTreeNode node)
        {
            if (node == null) return;

            try
            {
                // Get the JSON for this node
                var nodeJson = GetNodeJson(node);
                if (string.IsNullOrEmpty(nodeJson)) return;

                // Create new tab with formatted JSON
                var formatted = JToken.Parse(nodeJson).ToString(Formatting.Indented);
                _onCreateNewTab?.Invoke($"{node.Key ?? "Root"}", formatted);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating tab from node: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateTabFromResult()
        {
            if (string.IsNullOrWhiteSpace(Result) || Result == "null") return;

            try
            {
                // Try to parse as JSON
                var token = JToken.Parse(Result);
                var formatted = token.ToString(Formatting.Indented);
                _onCreateNewTab?.Invoke("Result", formatted);
            }
            catch
            {
                MessageBox.Show("Result is not valid JSON and cannot be opened as a new tab.", 
                    "Invalid JSON", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void GenerateToDateQuery(JsonTreeNode node)
        {
            if (node == null) return;

            string detectedFormat = DetectDateFormat(node.Value);
            var dialog = new DateFormatDialog(detectedFormat);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                var sourceFormat = dialog.SourceFormat;
                var targetFormat = dialog.TargetFormat;
                
                var sortedPath = GetSortedPath(node);
                
                // Build the todate query with format parameters
                var sourceArg = string.IsNullOrWhiteSpace(sourceFormat) ? "''" : $"'{sourceFormat}'";
                var targetArg = string.IsNullOrWhiteSpace(targetFormat) ? "''" : $"'{targetFormat}'";
                
                Query = $"todate({sortedPath}, {sourceArg}, {targetArg})";
            }
        }

        public void GenerateToDateTimeQuery(JsonTreeNode node)
        {
            if (node == null) return;

            string detectedFormat = DetectDateFormat(node.Value);
            var dialog = new TimezoneSelectionDialog("Central Standard Time", "Pacific Standard Time", detectedFormat);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                var fromTz = dialog.SelectedFromTimezone;
                var toTz = dialog.SelectedToTimezone;
                var format = dialog.SelectedDateFormat;
                var formatArg = string.IsNullOrWhiteSpace(format) ? "''" : $"'{format}'";
                var sortedPath = GetSortedPath(node);
                Query = $"todatetime({sortedPath}, {formatArg}, '{fromTz}', '{toTz}')";
            }
        }

        public void GenerateJoinQuery(JsonTreeNode node)
        {
            if (node == null || !node.IsArray) return;

            var sortedPath = GetSortedPath(node);

            // Use the new BatchQueryDialog which supports custom expressions and builder
            var dialog = new BatchQueryDialog(node, sortedPath);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                Query = dialog.GeneratedQuery;
            }
        }

        public void GenerateMapQuery(JsonTreeNode node)
        {
            if (node == null || !node.IsArray) return;

            var sortedPath = GetSortedPath(node);

            var dialog = new MapArrayDialog(node, sortedPath);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                Query = dialog.GeneratedQuery;
            }
        }

        public void GenerateFunctionQuery(JsonTreeNode node, string functionName)
        {
            if (node == null || string.IsNullOrEmpty(functionName)) return;
            var sortedPath = GetSortedPath(node);
            Query = $"{functionName}({sortedPath})";
        }

        public void ConfigureArraySort(JsonTreeNode node)
        {
            if (node == null || !node.IsArray || !node.HasChildren) return;

            var firstItem = node.Children.FirstOrDefault();
            if (firstItem == null || !firstItem.HasChildren) return;

            var sortKeys = GetSortableKeys(firstItem);
            if (sortKeys.Count == 0)
            {
                MessageBox.Show("No sortable properties found in array items.", "Array Sort",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new ArraySortDialog(sortKeys, node.SortKey, node.SortAscending);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                node.SortKey = dialog.SelectedSortKey;
                node.SortAscending = dialog.SortAscending;

                var wasExpanded = node.IsExpanded;
                node.IsExpanded = !wasExpanded;
                System.Threading.Tasks.Task.Delay(10).Wait();
                node.IsExpanded = wasExpanded;

                if (_currentSelectedNode != null && _currentSelectedNode != node)
                {
                    _currentSelectedNode.IsSelected = false;
                }

                node.IsSelected = true;
                _currentSelectedNode = node;
                UpdateQueryWithNodeSort(node);
            }
        }

        public void ClearArraySort(JsonTreeNode node)
        {
            if (node == null) return;
            node.SortKey = null;
            node.SortAscending = true;
        }

        public void OnNodeSelected(JsonTreeNode node)
        {
            if (node == null) return;

            if (_currentSelectedNode == node)
            {
                node.IsSelected = false;
                _currentSelectedNode = null;
                Query = "";
                return;
            }

            if (_currentSelectedNode != null)
            {
                _currentSelectedNode.IsSelected = false;
            }

            node.IsSelected = true;
            _currentSelectedNode = node;
            UpdateQueryWithNodeSort(node);
        }

        private string GetNodeJson(JsonTreeNode node)
        {
            try
            {
                var rootToken = JToken.Parse(JsonInput);
                var currentToken = rootToken;

                // Navigate to the node
                if (!string.IsNullOrEmpty(node.Path))
                {
                    var pathParts = ParsePath(node.Path);
                    foreach (var part in pathParts)
                    {
                        if (part.IsArray)
                        {
                            currentToken = currentToken[part.Index];
                        }
                        else
                        {
                            currentToken = currentToken[part.Key];
                        }

                        if (currentToken == null) return null;
                    }
                }

                return currentToken.ToString(Formatting.None);
            }
            catch
            {
                return null;
            }
        }

        private List<PathPart> ParsePath(string path)
        {
            var parts = new List<PathPart>();
            var segments = path.Split('.');

            foreach (var segment in segments)
            {
                if (segment.Contains('['))
                {
                    var keyPart = segment.Substring(0, segment.IndexOf('['));
                    if (!string.IsNullOrEmpty(keyPart))
                    {
                        parts.Add(new PathPart { Key = keyPart, IsArray = false });
                    }

                    var arrayPart = segment.Substring(segment.IndexOf('['));
                    var indexStr = arrayPart.Trim('[', ']');
                    if (int.TryParse(indexStr, out int index))
                    {
                        parts.Add(new PathPart { Index = index, IsArray = true });
                    }
                }
                else
                {
                    parts.Add(new PathPart { Key = segment, IsArray = false });
                }
            }

            return parts;
        }

        private class PathPart
        {
            public string Key { get; set; }
            public int Index { get; set; }
            public bool IsArray { get; set; }
        }

        private string GetSortedPath(JsonTreeNode node)
        {
            var query = node.Path;
            var arrayAncestors = FindArrayAncestorsWithSort(node);

            if (node.IsArray && node.HasSortApplied)
            {
                arrayAncestors.Add(node);
            }

            if (arrayAncestors.Count > 0)
            {
                var sortedArrays = arrayAncestors.OrderByDescending(a => a.Path.Length).ToList();

                foreach (var arrayNode in sortedArrays)
                {
                    var arrayPath = arrayNode.Path;
                    var sortKey = arrayNode.SortKey;

                    if (arrayNode.HasChildren)
                    {
                        var firstItem = arrayNode.Children.FirstOrDefault();
                        if (firstItem != null && firstItem.HasChildren)
                        {
                            var sortKeyProp = firstItem.Children.FirstOrDefault(c => c.Key == sortKey);
                            if (sortKeyProp != null)
                            {
                                var format = DetectDateFormat(sortKeyProp.Value);
                                if (!string.IsNullOrEmpty(format))
                                {
                                    sortKey = $"todatetime({sortKey}, '{format}')";
                                }
                            }
                        }
                    }

                    var sortExpression = $"{arrayPath} | sort_by(@, &{sortKey})";
                    if (!arrayNode.SortAscending)
                    {
                        sortExpression += " | reverse(@)";
                    }

                    if (query.StartsWith(arrayPath))
                    {
                        var remainder = query.Substring(arrayPath.Length);
                        query = sortExpression + remainder;
                    }
                }
            }
            return query;
        }

        private void UpdateQueryWithNodeSort(JsonTreeNode node)
        {
            Query = GetSortedPath(node);
        }

        private List<JsonTreeNode> FindArrayAncestorsWithSort(JsonTreeNode node)
        {
            var ancestors = new List<JsonTreeNode>();
            FindArrayAncestorsRecursive(JsonTreeNodes, node, ancestors);
            return ancestors;
        }

        private bool FindArrayAncestorsRecursive(ObservableCollection<JsonTreeNode> nodes, JsonTreeNode targetNode, List<JsonTreeNode> ancestors)
        {
            foreach (var node in nodes)
            {
                if (targetNode.Path.StartsWith(node.Path) && node.Path != targetNode.Path)
                {
                    if (node.IsArray && node.HasSortApplied)
                    {
                        ancestors.Add(node);
                    }

                    if (node.HasChildren)
                    {
                        FindArrayAncestorsRecursive(node.Children, targetNode, ancestors);
                    }
                    return true;
                }

                if (node.HasChildren)
                {
                    if (FindArrayAncestorsRecursive(node.Children, targetNode, ancestors))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private List<string> GetSortableKeys(JsonTreeNode node)
        {
            var keys = new List<string>();
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    if (!child.HasChildren)
                    {
                        keys.Add(child.Key);
                    }
                }
            }
            return keys;
        }

        private string DetectDateFormat(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return "";

            string[] formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy",
                "HH:mm:ss yyyy-MM-dd"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out _))
                {
                    return format;
                }
            }
            return "";
        }

        private void RefreshJsonTree()
        {
            try
            {
                var newTree = _treeBuilder.BuildTree(_jsonInput);
                _currentSelectedNode = null;
                JsonTreeNodes = newTree;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshJsonTree Error: {ex.Message}");
                JsonTreeNodes = new ObservableCollection<JsonTreeNode>();
            }
        }
    }
}
