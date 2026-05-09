using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace JmesPathWpfDemo.ViewModels
{
	public class XmlWorkspaceViewModel : Screen
	{
		private string _currentView = "XmlQuery";
		private QueryStoreViewModel _queryStoreViewModel;
		private FunctionReferenceViewModel _functionReferenceViewModel;
		private ObservableCollection<XmlQueryTabViewModel> _xmlQueryTabs;
		private XmlQueryTabViewModel _selectedXmlQueryTab;

		public XmlWorkspaceViewModel()
		{
			_queryStoreViewModel = new QueryStoreViewModel(OnQueryLoadedFromStore, "xml_saved_queries.json", CreateDefaultXmlQueries());
			_functionReferenceViewModel = new FunctionReferenceViewModel(OnTryExample);
			_xmlQueryTabs = new ObservableCollection<XmlQueryTabViewModel>();

			var initialXml = LoadInitialXml();
            var mainTab = new XmlQueryTabViewModel("Main", initialXml, canClose: false, OnCreateNewTab, GetSavedQueries);
			mainTab.Query = "/Root";
			_xmlQueryTabs.Add(mainTab);
			_selectedXmlQueryTab = mainTab;
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

		public ObservableCollection<XmlQueryTabViewModel> XmlQueryTabs
		{
			get => _xmlQueryTabs;
			set
			{
				_xmlQueryTabs = value;
				NotifyOfPropertyChange(() => XmlQueryTabs);
			}
		}

		public XmlQueryTabViewModel SelectedXmlQueryTab
		{
			get => _selectedXmlQueryTab;
			set
			{
				_selectedXmlQueryTab = value;
				NotifyOfPropertyChange(() => SelectedXmlQueryTab);
			}
		}

		public string CurrentView
		{
			get => _currentView;
			set
			{
				_currentView = value;
				NotifyOfPropertyChange(() => CurrentView);
				NotifyOfPropertyChange(() => XmlQueryViewVisibility);
				NotifyOfPropertyChange(() => QueryStoreViewVisibility);
				NotifyOfPropertyChange(() => FunctionReferenceViewVisibility);
			}
		}

		public Visibility XmlQueryViewVisibility => CurrentView == "XmlQuery" ? Visibility.Visible : Visibility.Collapsed;

		public Visibility QueryStoreViewVisibility => CurrentView == "QueryStore" ? Visibility.Visible : Visibility.Collapsed;

		public Visibility FunctionReferenceViewVisibility => CurrentView == "FunctionReference" ? Visibility.Visible : Visibility.Collapsed;

		public void ShowXmlQuery()
		{
			CurrentView = "XmlQuery";
		}

		public void ShowQueryStore()
		{
			CurrentView = "QueryStore";
		}

		public void ShowFunctionReference()
		{
			CurrentView = "FunctionReference";
		}

		public void CloseTab(XmlQueryTabViewModel tab)
		{
			if (tab == null || !tab.CanClose)
			{
				return;
			}

			var index = _xmlQueryTabs.IndexOf(tab);
			_xmlQueryTabs.Remove(tab);
			if (_xmlQueryTabs.Count > 0)
			{
				SelectedXmlQueryTab = index > 0 ? _xmlQueryTabs[index - 1] : _xmlQueryTabs[0];
			}
		}

		public void SaveCurrentQuery()
		{
			if (_selectedXmlQueryTab == null)
			{
				MessageBox.Show("No active tab.", "Save Query", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var query = _selectedXmlQueryTab.Query;
			if (string.IsNullOrWhiteSpace(query))
			{
				MessageBox.Show("Query string is empty.", "Save Query", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var dialog = new Views.SaveQueryDialog(query);
			dialog.Owner = Application.Current.MainWindow;
			if (dialog.ShowDialog() == true)
			{
				QueryStoreViewModel.AddQuery(new SavedQuery
				{
					Name = dialog.QueryName,
					Description = dialog.QueryDescription,
					Expression = query
				});
				MessageBox.Show("Query saved successfully.", "Save Query", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private void OnQueryLoadedFromStore(string queryExpression)
		{
			if (_selectedXmlQueryTab != null)
			{
				_selectedXmlQueryTab.Query = queryExpression;
			}

			CurrentView = "XmlQuery";
		}

		private void OnTryExample(string jsonData, string query)
		{
			try
			{
				var doc = JsonConvert.DeserializeXmlNode(jsonData, "Root");
				if (doc == null)
				{
					return;
				}

             var exampleTab = new XmlQueryTabViewModel("Example", doc.OuterXml, canClose: true, OnCreateNewTab, GetSavedQueries);
				exampleTab.Query = query;
				_xmlQueryTabs.Add(exampleTab);
				SelectedXmlQueryTab = exampleTab;
				CurrentView = "XmlQuery";
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error trying example: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnCreateNewTab(string title, string xml)
		{
			try
			{
              var newTab = new XmlQueryTabViewModel(title, xml, canClose: true, OnCreateNewTab, GetSavedQueries);
				_xmlQueryTabs.Add(newTab);
				SelectedXmlQueryTab = newTab;
				CurrentView = "XmlQuery";
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error creating new tab: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private static List<SavedQuery> CreateDefaultXmlQueries()
		{
			return new List<SavedQuery>
			{
				new SavedQuery
				{
					Name = "Root",
					Description = "Select root node",
					Expression = "/Root"
				},
				new SavedQuery
				{
					Name = "All UserDefinedFields",
					Description = "Select all UserDefinedFields elements",
					Expression = "/Root/UserDefinedFields"
				}
			};
		}

		private static string LoadInitialXml()
		{
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
					return doc?.OuterXml ?? "<Root/>";
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error loading sample.json and generating XML: {ex.Message}");
			}

			return "<Root/>";
		}

		private List<SavedQuery> GetSavedQueries()
		{
			return QueryStoreViewModel?.SavedQueries?.ToList() ?? new List<SavedQuery>();
		}
	}
}