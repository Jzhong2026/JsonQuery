using Caliburn.Micro;
using DevLab.JmesPath;
using JmesPathWpfDemo.Jmes;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using JmesPathWpfDemo.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JmesPathWpfDemo.ViewModels
{
	public class ShellViewModel : Screen
	{
		private readonly JmesPath _engine = new();
		private readonly JsonTreeBuilder _treeBuilder = new();
		private const string SavedQueriesFileName = "saved_queries.json";
		private string _jsonInput;
		private string _query;
		private string _result;
		private int _selectedTabIndex;
		private ObservableCollection<JsonTreeNode> _jsonTreeNodes;
		private JsonTreeNode _currentSelectedNode;
		private string _currentView = "JsonQuery";
		private QueryStoreViewModel _queryStoreViewModel;
		private FunctionReferenceViewModel _functionReferenceViewModel;
		private ObservableCollection<JsonQueryTabViewModel> _jsonQueryTabs;
		private JsonQueryTabViewModel _selectedJsonQueryTab;

		public ShellViewModel()
		{
			_engine.FunctionRepository.Register<ConcatFunction>();
			_engine.FunctionRepository.Register<ConcatWsFunction>();
			_engine.FunctionRepository.Register<ToDateTimeFunction>();
            _engine.FunctionRepository.Register<NewLineFunction>();
            _engine.FunctionRepository.Register<IffFunction>();
            _engine.FunctionRepository.Register<EqualFunction>();
            _engine.FunctionRepository.Register<MergeArraysFunction>();
            _engine.FunctionRepository.Register<FlattenFunction>();

			// Initialize collections first
			_jsonTreeNodes = new ObservableCollection<JsonTreeNode>();
			
			// Initialize Query Store ViewModel
			_queryStoreViewModel = new QueryStoreViewModel(OnQueryLoadedFromStore);
			
			// Initialize Function Reference ViewModel
			_functionReferenceViewModel = new FunctionReferenceViewModel(OnTryExample);
            
			// Load from file if exists, otherwise fallback to empty object
			try
			{
				var assemblyLocation = Assembly.GetExecutingAssembly().Location;
				var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
				var samplePath = Path.Combine(assemblyDirectory, "sample.json");
				
				// If not found in bin, try to find in project structure (for development)
				if (!File.Exists(samplePath))
				{
					// Basic heuristic to find project folder from bin/Debug/net8.0-windows
					var projectDir = Path.GetFullPath(Path.Combine(assemblyDirectory, @"..\..\.."));
					var projectSamplePath = Path.Combine(projectDir, "sample.json");
					if (File.Exists(projectSamplePath))
					{
						samplePath = projectSamplePath;
					}
				}

				if (File.Exists(samplePath))
				{
					_jsonInput = File.ReadAllText(samplePath);
				}
				else
				{
					_jsonInput = "{}";
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error loading sample.json: {ex.Message}");
				_jsonInput = "{}";
			}

			_query = "UserDefinedFields";
			_result = "";

			// Initialize Tab collection and create main tab
			_jsonQueryTabs = new ObservableCollection<JsonQueryTabViewModel>();
			var mainTab = new JsonQueryTabViewModel("Main", _jsonInput, canClose: false, OnCreateNewTab);
			_jsonQueryTabs.Add(mainTab);
			_selectedJsonQueryTab = mainTab;

			// Build the tree after everything is initialized
			RefreshJsonTree();
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
					// Don't auto-refresh anymore, user will click Refresh button
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

		public QueryStoreViewModel QueryStoreViewModel
		{
			get => _queryStoreViewModel;
			set
			{
				_queryStoreViewModel = value;
				NotifyOfPropertyChange(() => QueryStoreViewModel);
			}
		}

		public FunctionReferenceViewModel FunctionReferenceViewModel
		{
			get => _functionReferenceViewModel;
			set
			{
				_functionReferenceViewModel = value;
				NotifyOfPropertyChange(() => FunctionReferenceViewModel);
			}
		}

		public ObservableCollection<JsonQueryTabViewModel> JsonQueryTabs
		{
			get => _jsonQueryTabs;
			set
			{
				_jsonQueryTabs = value;
				NotifyOfPropertyChange(() => JsonQueryTabs);
			}
		}

		public JsonQueryTabViewModel SelectedJsonQueryTab
		{
			get => _selectedJsonQueryTab;
			set
			{
				_selectedJsonQueryTab = value;
				NotifyOfPropertyChange(() => SelectedJsonQueryTab);
			}
		}

		public string CurrentView
		{
			get => _currentView;
			set
			{
				_currentView = value;
				NotifyOfPropertyChange(() => CurrentView);
				NotifyOfPropertyChange(() => JsonQueryViewVisibility);
				NotifyOfPropertyChange(() => QueryStoreViewVisibility);
				NotifyOfPropertyChange(() => FunctionReferenceViewVisibility);
			}
		}

		public Visibility JsonQueryViewVisibility => 
			CurrentView == "JsonQuery" ? Visibility.Visible : Visibility.Collapsed;
		
		public Visibility QueryStoreViewVisibility => 
			CurrentView == "QueryStore" ? Visibility.Visible : Visibility.Collapsed;
		
		public Visibility FunctionReferenceViewVisibility => 
			CurrentView == "FunctionReference" ? Visibility.Visible : Visibility.Collapsed;

		public void ShowJsonQuery()
		{
			CurrentView = "JsonQuery";
		}

		public void ShowQueryStore()
		{
			CurrentView = "QueryStore";
		}

		public void ShowFunctionReference()
		{
			CurrentView = "FunctionReference";
		}

		private void OnQueryLoadedFromStore(string queryExpression)
		{
			// Load query into current selected tab
			if (_selectedJsonQueryTab != null)
			{
				_selectedJsonQueryTab.Query = queryExpression;
			}
			// Switch back to JSON Query view
			CurrentView = "JsonQuery";
		}

		private void OnTryExample(string jsonData, string query)
		{
			try
			{
				// Create a new tab with the example data
				var exampleTab = new JsonQueryTabViewModel("Example", jsonData, canClose: true, OnCreateNewTab);
				exampleTab.Query = query;
				
				_jsonQueryTabs.Add(exampleTab);
				SelectedJsonQueryTab = exampleTab;
				
				// Switch to JSON Query view
				CurrentView = "JsonQuery";
				
				// Auto-execute the query
				exampleTab.Execute();
				
				System.Diagnostics.Debug.WriteLine($"Created example tab with query: {query}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error trying example: {ex.Message}", "Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnCreateNewTab(string title, string json)
		{
			try
			{
				// Create new tab
				var newTab = new JsonQueryTabViewModel(title, json, canClose: true, OnCreateNewTab);
				_jsonQueryTabs.Add(newTab);
				
				// Switch to the new tab
				SelectedJsonQueryTab = newTab;
				
				System.Diagnostics.Debug.WriteLine($"Created new tab: {title}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error creating new tab: {ex.Message}", "Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void CloseTab(JsonQueryTabViewModel tab)
		{
			if (tab == null || !tab.CanClose)
				return;

			try
			{
				var index = _jsonQueryTabs.IndexOf(tab);
				_jsonQueryTabs.Remove(tab);

				// Switch to previous tab or first tab
				if (_jsonQueryTabs.Count > 0)
				{
					SelectedJsonQueryTab = index > 0
						? _jsonQueryTabs[index - 1]
						: _jsonQueryTabs[0];
				}

				System.Diagnostics.Debug.WriteLine($"Closed tab: {tab.TabTitle}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error closing tab: {ex.Message}", "Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

        public void LoadQuery(SavedQuery savedQuery)
		{
			if (savedQuery != null)
			{
				Query = savedQuery.Expression;
			}
		}

		public void DeleteQuery(SavedQuery savedQuery)
		{
			if (savedQuery != null)
			{
				_queryStoreViewModel.DeleteQuery(savedQuery);
			}
		}
        
        public void SaveCurrentQuery()
        {
			// Get query from current selected tab
			if (_selectedJsonQueryTab == null)
			{
				MessageBox.Show("No active tab.", "Save Query", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var query = _selectedJsonQueryTab.Query;
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Query string is empty.", "Save Query", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveQueryDialog(query);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                var newQuery = new SavedQuery 
                { 
                    Name = dialog.QueryName, 
                    Description = dialog.QueryDescription, 
                    Expression = query 
                };
                _queryStoreViewModel.AddQuery(newQuery);
                MessageBox.Show("Query saved successfully.", "Save Query", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadSavedQueries()
        {
            // No longer needed - handled by QueryStoreViewModel
        }

        private void SaveSavedQueries()
        {
            // No longer needed - handled by QueryStoreViewModel
        }

        public async void BuildComplexQuery()
        {
            var vm = new ComplexQueryBuilderViewModel(_queryStoreViewModel.SavedQueries);
            var windowManager = new WindowManager();
            var result = await windowManager.ShowDialogAsync(vm);

            if (result == true && !string.IsNullOrEmpty(vm.ResultQuery) && _selectedJsonQueryTab != null)
            {
                _selectedJsonQueryTab.Query = vm.ResultQuery;
            }
        }

		public void Execute()
		{
			try
			{
				var jsonResult = _engine.Transform(JsonInput, Query);
                // The result is a JSON string. We want to display raw strings without quotes/escapes,
                // but keep objects/arrays as JSON (formatted nicely).
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
                        // Unescape string
                        Result = token.Value<string>();
                    }
                    else
                    {
                        // Format object/array
                        Result = token.ToString(Formatting.Indented);
                    }
                }
                catch
                {
                    // Fallback if parsing fails
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
				
				// If format is provided, use it, otherwise use ''
				var formatArg = string.IsNullOrWhiteSpace(format) ? "''" : $"'{format}'";
				
				var sortedPath = GetSortedPath(node);
				Query = $"todatetime({sortedPath}, {formatArg}, '{fromTz}', '{toTz}')";
			}
		}

		public void GenerateJoinQuery(JsonTreeNode node)
		{
			if (node == null || !node.IsArray) return;

			// Determine if array contains objects and get their properties
			List<string> properties = null;
			if (node.HasChildren)
			{
				var firstItem = node.Children.FirstOrDefault();
				if (firstItem != null && firstItem.HasChildren) // It's an object
				{
					properties = GetSortableKeys(firstItem);
				}
			}

			var dialog = new JoinQueryDialog(properties, _queryStoreViewModel.SavedQueries.ToList());
			dialog.Owner = Application.Current.MainWindow;

			if (dialog.ShowDialog() == true)
			{
				var separator = dialog.SelectedSeparator;
				var property = dialog.SelectedProperty;
				var sortedPath = GetSortedPath(node);

				// If using pipeline with saved query
				if (dialog.UsePipeline && dialog.SelectedSavedQuery != null)
				{
					if (!string.IsNullOrEmpty(property))
					{
						Query = $"({dialog.SelectedSavedQuery.Expression}) | join('{separator}', {sortedPath}[*].{property})";
					}
					else
					{
						Query = $"({dialog.SelectedSavedQuery.Expression}) | join('{separator}', {sortedPath})";
					}
				}
				else if (!string.IsNullOrEmpty(property))
				{
					Query = $"join('{separator}', {sortedPath}[*].{property})";
				}
				else
				{
					Query = $"join('{separator}', {sortedPath})";
				}
			}
		}

        public void GenerateFunctionQuery(JsonTreeNode node, string functionName)
        {
            if (node == null || string.IsNullOrEmpty(functionName)) return;

            var sortedPath = GetSortedPath(node);
            Query = $"{functionName}({sortedPath})";
        }

		public void GenerateCombineQuery(JsonTreeNode node)
		{
			if (node == null) return;

			// Do not include the sort of the current node, because we are projecting it (list of lists)
            // and sorting the list of lists with item-properties is invalid/confusing.
			var sortedPath = GetSortedPath(node, false);
            
            bool replaced = false;
            // basic heuristic: replace specific array indices [0], [1] etc with [*]
            // We use Regex to replace indices that are not inside quotes
            var combinedPath = System.Text.RegularExpressions.Regex.Replace(sortedPath, @"\[\d+\]", (match) => 
            {
                replaced = true;
                return "[*]";
            });

            // If we performed a replacement (meaning we are projecting from a list)
            // AND the node itself is an array (meaning we now have a list of lists)
            // Then flatten it.
            if (replaced && node.IsArray)
            {
                combinedPath += "[]";
            }

            Query = combinedPath;
		}

		private string DetectDateFormat(string dateString)
		{
			if (string.IsNullOrWhiteSpace(dateString)) return "";

			// List of common formats to check against
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
				if (DateTime.TryParseExact(dateString, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
				{
					return format;
				}
			}

			return "";
		}

		public void ConfigureArraySort(JsonTreeNode node)
		{
			if (node == null || !node.IsArray || !node.HasChildren)
				return;

			// Get all possible sort keys from first array item
			var firstItem = node.Children.FirstOrDefault();
			if (firstItem == null || !firstItem.HasChildren)
				return;

			var sortKeys = GetSortableKeys(firstItem);
			if (sortKeys.Count == 0)
			{
				MessageBox.Show("No sortable properties found in array items.", "Array Sort",
					MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			// Show sort configuration dialog
			var dialog = new ArraySortDialog(sortKeys, node.SortKey, node.SortAscending);
			dialog.Owner = Application.Current.MainWindow;

			if (dialog.ShowDialog() == true)
			{
				System.Diagnostics.Debug.WriteLine($"Before: SortKey={node.SortKey}, SortAscending={node.SortAscending}");

				// Update properties - this will trigger PropertyChanged automatically
				node.SortKey = dialog.SelectedSortKey;
				node.SortAscending = dialog.SortAscending;

				System.Diagnostics.Debug.WriteLine($"After: SortKey={node.SortKey}, SortAscending={node.SortAscending}");
				System.Diagnostics.Debug.WriteLine($"DisplayText: {node.DisplayText}");

				// Force UI refresh by temporarily toggling IsExpanded
				var wasExpanded = node.IsExpanded;
				node.IsExpanded = !wasExpanded;
				System.Threading.Tasks.Task.Delay(10).Wait(); // Small delay
				node.IsExpanded = wasExpanded;

				// Auto-select the node and update query (selection is always enabled)
				// Deselect previous node
				if (_currentSelectedNode != null && _currentSelectedNode != node)
				{
					_currentSelectedNode.IsSelected = false;
				}

				// Select this node
				node.IsSelected = true;
				_currentSelectedNode = node;

				// Update query with sort
				UpdateQueryWithNodeSort(node);
			}
		}

		public void OnNodeSelected(JsonTreeNode node)
		{
			if (node == null)
				return;

			// If clicking the same node, toggle it off
			if (_currentSelectedNode == node)
			{
				node.IsSelected = false;
				_currentSelectedNode = null;
				Query = "";
				return;
			}

			// Deselect the previously selected node
			if (_currentSelectedNode != null)
			{
				_currentSelectedNode.IsSelected = false;
			}

			// Select the new node
			node.IsSelected = true;
			_currentSelectedNode = node;

			// Update query based on node and its sort settings
			UpdateQueryWithNodeSort(node);
		}

		private string GetSortedPath(JsonTreeNode node, bool includeSelfSort = true)
		{
			var query = node.Path;

			// Find all array ancestors with sorting
			var arrayAncestors = FindArrayAncestorsWithSort(node);

			// Also include the current node if it is a sorted array
			if (includeSelfSort && node.IsArray && node.HasSortApplied)
			{
				arrayAncestors.Add(node);
			}

			if (arrayAncestors.Count > 0)
			{
				// Build query with sorting for all array ancestors
				// Process deepest arrays first so prefix replacement works correctly
				var sortedArrays = arrayAncestors.OrderByDescending(a => a.Path.Length).ToList();

				foreach (var arrayNode in sortedArrays)
				{
					var arrayPath = arrayNode.Path;
					var sortKey = arrayNode.SortKey;
					
					// Check if sort key looks like a date
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
									// Use todatetime for sorting
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

					// Replace the array path in the query
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

			// Search through all nodes in the tree to find ancestors
			FindArrayAncestorsRecursive(JsonTreeNodes, node, ancestors);

			return ancestors;
		}

		private bool FindArrayAncestorsRecursive(ObservableCollection<JsonTreeNode> nodes, JsonTreeNode targetNode, List<JsonTreeNode> ancestors)
		{
			foreach (var node in nodes)
			{
				// Check if this node's path is a prefix of the target node's path
				if (targetNode.Path.StartsWith(node.Path) && node.Path != targetNode.Path)
				{
					// This is an ancestor
					if (node.IsArray && node.HasSortApplied)
					{
						ancestors.Add(node);
					}

					// Continue searching in children
					if (node.HasChildren)
					{
						FindArrayAncestorsRecursive(node.Children, targetNode, ancestors);
					}

					return true;
				}

				// Search in children
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
					// Only add leaf properties (not objects or arrays)
					if (!child.HasChildren)
					{
						keys.Add(child.Key);
					}
				}
			}

			return keys;
		}

		private void RefreshJsonTree()
		{
			try
			{
				var newTree = _treeBuilder.BuildTree(_jsonInput);

				System.Diagnostics.Debug.WriteLine($"RefreshJsonTree: Built tree with {newTree.Count} root nodes");

				// Clear current selection when tree is refreshed
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
