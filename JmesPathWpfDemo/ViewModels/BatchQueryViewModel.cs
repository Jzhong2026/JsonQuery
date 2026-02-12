using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JmesPathWpfDemo.ViewModels
{
    public class BatchQueryViewModel : Screen
    {
        private string _queryTemplate;
        private string _separator;
        private JsonTreeNode _targetArrayNode;
        private string _targetArrayPath;
        private ObservableCollection<SavedQuery> _savedQueries;
        private SavedQuery _selectedSavedQuery;
        private const string SavedQueriesFileName = "saved_queries.json";
        private ObservableCollection<string> _availableProperties;
        private bool _isForField = true;
        private string _selectedField;

        public BatchQueryViewModel(JsonTreeNode node, string targetArrayPath)
        {
            _targetArrayNode = node;
            _targetArrayPath = targetArrayPath;
            _queryTemplate = "Name"; // Default
            _separator = "\\n"; // Default separator
            _availableProperties = new ObservableCollection<string>();
            ExtractAvailableProperties();
            LoadSavedQueries();
            
            // Set initial selected field if match with template
            if (_availableProperties.Contains(_queryTemplate))
            {
                SelectedField = _queryTemplate;
            }
        }
        
        public bool IsForField
        {
            get => _isForField;
            set
            {
                _isForField = value;
                NotifyOfPropertyChange(() => IsForField);
                NotifyOfPropertyChange(() => IsForSavedQuery);
            }
        }

        public bool IsForSavedQuery
        {
            get => !_isForField;
            set
            {
                _isForField = !value;
                NotifyOfPropertyChange(() => IsForField);
                NotifyOfPropertyChange(() => IsForSavedQuery);
            }
        }

        public string SelectedField
        {
            get => _selectedField;
            set
            {
                _selectedField = value;
                NotifyOfPropertyChange(() => SelectedField);
                // Only update template if in field mode and selection is valid
                if (IsForField && !string.IsNullOrEmpty(value))
                {
                    QueryTemplate = value;
                }
            }
        }

        public string DialogTitle => $"Apply Query to Array Items: {_targetArrayNode.Key}";

        public string QueryTemplate
        {
            get => _queryTemplate;
            set
            {
                _queryTemplate = value;
                NotifyOfPropertyChange(() => QueryTemplate);
                NotifyOfPropertyChange(() => PreviewQuery);
            }
        }

        public string Separator
        {
            get => _separator;
            set
            {
                _separator = value;
                NotifyOfPropertyChange(() => Separator);
                NotifyOfPropertyChange(() => PreviewQuery);
            }
        }

        public ObservableCollection<SavedQuery> SavedQueries
        {
            get => _savedQueries;
            set
            {
                _savedQueries = value;
                NotifyOfPropertyChange(() => SavedQueries);
            }
        }

        public SavedQuery SelectedSavedQuery
        {
            get => _selectedSavedQuery;
            set
            {
                _selectedSavedQuery = value;
                NotifyOfPropertyChange(() => SelectedSavedQuery);
                if (_selectedSavedQuery != null && IsForSavedQuery)
                {
                    QueryTemplate = _selectedSavedQuery.Expression;
                }
            }
        }

        public ObservableCollection<string> AvailableProperties => _availableProperties;

        public string PreviewQuery
        {
            get
            {
                // 构建预览查询
                // join(separator, Path[*].expression)
                
                string sepDisplay = ParseSeparator(Separator);
                
                // 去掉表达式外面的括号
                return $"join({sepDisplay}, {_targetArrayPath}[*].{QueryTemplate})";
            }
        }

        private string ParseSeparator(string input)
        {
            if (string.IsNullOrEmpty(input)) return "''";

            // Backward compatibility: if strictly "newline()", treat as function
            if (string.Equals(input, "newline()", StringComparison.OrdinalIgnoreCase))
            {
                return "newline()";
            }

            // Treat "\n" string sequence as newline char placeholder
            if (input.Contains("\\n"))
            {
                var parts = input.Split(new[] { "\\n" }, StringSplitOptions.None);
                var exprParts = new List<string>();

                for (int i = 0; i < parts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parts[i]))
                    {
                        exprParts.Add($"'{parts[i].Replace("'", "\\'")}'");
                    }
                    
                    if (i < parts.Length - 1)
                    {
                        exprParts.Add("newline()");
                    }
                }
                
                if (exprParts.Count == 0) return "''";
                if (exprParts.Count == 1) return exprParts[0];

                return $"concat({string.Join(", ", exprParts)})";
            }

            // Normal string literal
            return $"'{input.Replace("'", "\\'")}'";
        }

        private void LoadSavedQueries()
        {
            try
            {
                if (File.Exists(SavedQueriesFileName))
                {
                    var json = File.ReadAllText(SavedQueriesFileName);
                    var queries = System.Text.Json.JsonSerializer.Deserialize<List<SavedQuery>>(json);
                    if (queries != null)
                    {
                        SavedQueries = new ObservableCollection<SavedQuery>(queries);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved queries: {ex.Message}");
            }

            // Fallback if load failed or file doesn't exist
            SavedQueries = new ObservableCollection<SavedQuery>();
            // Add some defaults if empty
            SavedQueries.Add(new SavedQuery 
            { 
                Name = "Default Name", 
                Description = "Select Name property", 
                Expression = "Name" 
            });
        }

        private void ExtractAvailableProperties()
        {
            if (_targetArrayNode != null && _targetArrayNode.Children != null && _targetArrayNode.Children.Count > 0)
            {
                // Assuming array of objects, take the first item to discover properties
                var firstItem = _targetArrayNode.Children[0];
                if (firstItem.Children != null && firstItem.Children.Count > 0)
                {
                    foreach (var child in firstItem.Children)
                    {
                        if (!string.IsNullOrEmpty(child.Key))
                        {
                            _availableProperties.Add(child.Key);
                        }
                    }
                }
            }
            // Always add @ (Current Item) if not present, though usually we want properties.
            if (!_availableProperties.Contains("@"))
            {
                _availableProperties.Add("@");
            }
            
            // If we found a "Name" property, use it as default, otherwise first available
            if (_availableProperties.Contains("Name"))
            {
                _queryTemplate = "Name";
            }
            else if (_availableProperties.Count > 0)
            {
                _queryTemplate = _availableProperties[0] == "@" && _availableProperties.Count > 1 
                    ? _availableProperties[1] 
                    : _availableProperties[0];
            }
        }
    }
}
