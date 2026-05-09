using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace JmesPathWpfDemo.ViewModels
{
    public class QueryStoreViewModel : Screen
    {
        private const string SavedQueriesFileName = "saved_queries.json";
        private ObservableCollection<SavedQuery> _savedQueries;
        private readonly Action<string> _onQuerySelected;
        private readonly string _savedQueriesFileName;
        private readonly List<SavedQuery> _defaultQueries;

      public QueryStoreViewModel(Action<string> onQuerySelected, string savedQueriesFileName = SavedQueriesFileName, IEnumerable<SavedQuery> defaultQueries = null)
        {
            _onQuerySelected = onQuerySelected;
         _savedQueriesFileName = string.IsNullOrWhiteSpace(savedQueriesFileName) ? SavedQueriesFileName : savedQueriesFileName;
            _defaultQueries = defaultQueries?.ToList() ?? CreateDefaultQueries();
            LoadSavedQueries();
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

        public void LoadQuery(SavedQuery savedQuery)
        {
            if (savedQuery != null)
            {
                _onQuerySelected?.Invoke(savedQuery.Expression);
            }
        }

        public void DeleteQuery(SavedQuery savedQuery)
        {
            if (savedQuery != null && _savedQueries.Contains(savedQuery))
            {
                if (MessageBox.Show($"Are you sure you want to delete '{savedQuery.Name}'?", 
                    "Confirm Delete", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _savedQueries.Remove(savedQuery);
                    SaveSavedQueries();
                }
            }
        }

        public void AddQuery(SavedQuery query)
        {
            if (query != null)
            {
                _savedQueries.Add(query);
                SaveSavedQueries();
            }
        }

        private void LoadSavedQueries()
        {
            try
            {
              if (File.Exists(_savedQueriesFileName))
                {
                  var json = File.ReadAllText(_savedQueriesFileName);
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

            // Fallback to defaults if load failed or file doesn't exist
          SavedQueries = new ObservableCollection<SavedQuery>(_defaultQueries.Select(q => new SavedQuery
            {
                Name = q.Name,
                Description = q.Description,
                Expression = q.Expression
            }));
        }

        private void SaveSavedQueries()
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var json = System.Text.Json.JsonSerializer.Serialize(SavedQueries, options);
              File.WriteAllText(_savedQueriesFileName, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving queries: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static List<SavedQuery> CreateDefaultQueries()
        {
            return new List<SavedQuery>
            {
                new SavedQuery
                {
                    Name = "All Fields",
                    Description = "Selects all user defined fields",
                    Expression = "UserDefinedFields"
                },
                new SavedQuery
                {
                    Name = "High Priority",
                    Description = "Filters for high priority items",
                    Expression = "[?Priority == 'High']"
                }
            };
        }
    }
}
