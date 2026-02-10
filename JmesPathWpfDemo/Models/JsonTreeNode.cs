using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace JmesPathWpfDemo.Models
{
	public class JsonTreeNode : INotifyPropertyChanged
	{
		private bool _isSelected;
		private bool _isExpanded;
		private string _sortKey;
		private bool _sortAscending = true;
		private List<JsonTreeNode> _originalChildrenOrder;

		public string Key { get; set; }
		public string Value { get; set; }
		public string Type { get; set; }
		public string Path { get; set; }
		public ObservableCollection<JsonTreeNode> Children { get; set; }

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					OnPropertyChanged();
				}
			}
		}

		public bool IsExpanded
		{
			get => _isExpanded;
			set
			{
				if (_isExpanded != value)
				{
					_isExpanded = value;
					OnPropertyChanged();
				}
			}
		}

		// Sort key for array items (which property to sort by)
		public string SortKey
		{
			get => _sortKey;
			set
			{
				// Save original order before first sort
				if (_sortKey == null && value != null && IsArray && HasChildren)
				{
					SaveOriginalOrder();
				}

				_sortKey = value;
				System.Diagnostics.Debug.WriteLine($"SortKey set to: {value} for node: {Key}");
				OnPropertyChanged();
				OnPropertyChanged(nameof(DisplayText));
				OnPropertyChanged(nameof(ValueText));
				OnPropertyChanged(nameof(HasSortApplied));

				// Apply sorting to children when sort key changes
				if (!string.IsNullOrEmpty(value))
				{
					ApplySortToChildren();
				}
				else
				{
					RestoreOriginalOrder();
				}
			}
		}

		// Sort direction
		public bool SortAscending
		{
			get => _sortAscending;
			set
			{
				_sortAscending = value;
				System.Diagnostics.Debug.WriteLine($"SortAscending set to: {value} for node: {Key}");
				OnPropertyChanged();
				OnPropertyChanged(nameof(DisplayText));
				OnPropertyChanged(nameof(ValueText));

				// Apply sorting to children when direction changes
				if (HasSortApplied)
				{
					ApplySortToChildren();
				}
			}
		}

		public bool IsArray => Type == "Array";
		public bool HasChildren => Children != null && Children.Count > 0;
		public bool HasSortApplied => !string.IsNullOrEmpty(SortKey);
        public bool HasKey => !string.IsNullOrEmpty(Key);

		public string DisplayText
		{
			get
			{
				if (!string.IsNullOrEmpty(Key))
				{
					if (HasChildren)
					{
						var text = $"{Key}: {Type}";
						if (IsArray && HasSortApplied)
						{
							var direction = SortAscending ? "¡ü" : "¡ý";
							text += $" [Sorted by {SortKey} {direction}]";
						}
						return text;
					}
					else
						return $"{Key}: {Value}";
				}
				return Value;
			}
		}
		
		public string SortDescription
        {
            get
            {
                if (IsArray && HasSortApplied)
                {
                     var direction = SortAscending ? "¡ü" : "¡ý";
                     return $" [Sorted by {SortKey} {direction}]";
                }
                return string.Empty;
            }
        }

        public string ValueText
        {
            get
            {
                if (HasChildren)
                {
                    return $"{Type}{SortDescription}";
                }
                return Value;
            }
        }

		public JsonTreeNode()
		{
			Children = new ObservableCollection<JsonTreeNode>();
			IsExpanded = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Save the original order of children before sorting
		/// </summary>
		private void SaveOriginalOrder()
		{
			if (Children == null || Children.Count == 0)
				return;

			_originalChildrenOrder = new List<JsonTreeNode>(Children);
			System.Diagnostics.Debug.WriteLine($"Saved original order for {Key}: {Children.Count} items");
		}

		/// <summary>
		/// Restore the original order of children
		/// </summary>
		private void RestoreOriginalOrder()
		{
			if (_originalChildrenOrder == null || _originalChildrenOrder.Count == 0)
				return;

			// Restore original indices and paths
			for (int i = 0; i < _originalChildrenOrder.Count; i++)
			{
				var child = _originalChildrenOrder[i];
				var oldPath = child.Path;

				// Restore original key
				child.Key = $"[{i}]";

				// Restore original path
				var basePath = Path;
				var newPath = $"{basePath}[{i}]";
				child.Path = newPath;

				// Update all descendant paths recursively
				UpdateDescendantPaths(child, oldPath, newPath);
			}

			Children.Clear();
			foreach (var child in _originalChildrenOrder)
			{
				Children.Add(child);
			}

			System.Diagnostics.Debug.WriteLine($"Restored original order for {Key}: {Children.Count} items");
		}

		/// <summary>
		/// Apply sorting to the children collection based on SortKey and SortAscending
		/// </summary>
		private void ApplySortToChildren()
		{
			if (!IsArray || !HasSortApplied || !HasChildren)
				return;

			try
			{
				// Get the sorted list
				var sortedChildren = SortAscending
					? Children.OrderBy(GetSortValue).ToList()
					: Children.OrderByDescending(GetSortValue).ToList();

				// Update the array indices and paths for sorted children
				for (int i = 0; i < sortedChildren.Count; i++)
				{
					var child = sortedChildren[i];
					var oldKey = child.Key;
					var oldPath = child.Path;

					// Update key
					child.Key = $"[{i}]";

					// Update path to reflect new index
					var basePath = Path;
					var newPath = $"{basePath}[{i}]";
					child.Path = newPath;

					// Update all descendant paths recursively
					UpdateDescendantPaths(child, oldPath, newPath);

					System.Diagnostics.Debug.WriteLine($"Reindexed: {oldKey} (path: {oldPath}) -> {child.Key} (path: {newPath})");
				}

				// Clear and re-add in sorted order
				Children.Clear();
				foreach (var child in sortedChildren)
				{
					Children.Add(child);
				}

				var direction = SortAscending ? "ASC" : "DESC";
				System.Diagnostics.Debug.WriteLine($"Applied sort to {Key}: {Children.Count} items sorted by {SortKey} {direction}");
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error sorting children: {ex.Message}");
			}
		}

		/// <summary>
		/// Recursively update paths of all descendant nodes
		/// </summary>
		private void UpdateDescendantPaths(JsonTreeNode node, string oldParentPath, string newParentPath)
		{
			if (node == null || !node.HasChildren)
				return;

			foreach (var child in node.Children)
			{
				// Replace the old parent path with new parent path
				if (child.Path.StartsWith(oldParentPath))
				{
					var oldChildPath = child.Path;
					child.Path = newParentPath + child.Path.Substring(oldParentPath.Length);

					System.Diagnostics.Debug.WriteLine($"  Updated descendant path: {oldChildPath} -> {child.Path}");

					// Recursively update children
					UpdateDescendantPaths(child, oldChildPath, child.Path);
				}
			}
		}

		/// <summary>
		/// Get the value to sort by from a child node
		/// </summary>
		private object GetSortValue(JsonTreeNode child)
		{
			if (child == null || !child.HasChildren)
				return null;

			// Find the property in the child's children
			var targetChild = child.Children.FirstOrDefault(c => c.Key == SortKey);

			if (targetChild == null)
				return null;

			var valueStr = targetChild.Value;

			// Try to parse as number for proper numeric sorting
			if (targetChild.Type == "Number" && double.TryParse(valueStr, out double numValue))
			{
				return numValue;
			}

            // Try to parse as Date for proper date sorting
            // Try standard formats first
            if (System.DateTime.TryParse(valueStr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out System.DateTime dateValue))
            {
                return dateValue;
            }
            
            // Try custom formats
            string[] customFormats = new[] { "HH:mm:ss yyyy-MM-dd" };
            if (System.DateTime.TryParseExact(valueStr, customFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }

			// Return string value for other types
			return valueStr ?? string.Empty;
		}

		/// <summary>
		/// Clear sorting configuration and restore original order
		/// </summary>
		public void ClearSort()
		{
			SortKey = null;
			SortAscending = true;
		}
	}
}
