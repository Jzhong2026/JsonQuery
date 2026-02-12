using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JmesPathWpfDemo.ViewModels
{
    public class MappingItemViewModel : PropertyChangedBase
    {
        private string _sourceProperty;
        private string _targetProperty;
        private string _selectedFunction;
        private ObservableCollection<string> _availableProperties;
        private ObservableCollection<string> _availableFunctions;
        private MapArrayViewModel _parent;

        public MappingItemViewModel(MapArrayViewModel parent, ObservableCollection<string> properties)
        {
            _parent = parent;
            _availableProperties = properties;
            _availableFunctions = new ObservableCollection<string>
            {
                "None", "to_string", "to_number", "length", "type", "abs", "ceil", "floor"
            };
            _selectedFunction = "None";
            
            if (_availableProperties.Any())
            {
                SourceProperty = _availableProperties.First();
            }
        }

        public ObservableCollection<string> AvailableProperties => _availableProperties;
        public ObservableCollection<string> AvailableFunctions => _availableFunctions;

        public string SourceProperty
        {
            get => _sourceProperty;
            set
            {
                if (_sourceProperty != value)
                {
                    bool shouldUpdateTarget = string.IsNullOrEmpty(_targetProperty) || _targetProperty == _sourceProperty;
                    _sourceProperty = value;
                    NotifyOfPropertyChange(() => SourceProperty);
                    
                    if (shouldUpdateTarget)
                    {
                        TargetProperty = value;
                    }
                    else
                    {
                        _parent?.UpdatePreview();
                    }
                }
            }
        }

        public string SelectedFunction
        {
            get => _selectedFunction;
            set
            {
                _selectedFunction = value;
                NotifyOfPropertyChange(() => SelectedFunction);
                _parent?.UpdatePreview();
            }
        }

        public string TargetProperty
        {
            get => _targetProperty;
            set
            {
                _targetProperty = value;
                NotifyOfPropertyChange(() => TargetProperty);
                _parent?.UpdatePreview();
            }
        }
    }

    public class MapArrayViewModel : Screen
    {
        private JsonTreeNode _targetArrayNode;
        private string _targetArrayPath;
        private ObservableCollection<MappingItemViewModel> _mappingItems;
        private ObservableCollection<string> _availableProperties;
        private string _previewQuery;

        public MapArrayViewModel(JsonTreeNode node, string targetArrayPath)
        {
            _targetArrayNode = node;
            _targetArrayPath = targetArrayPath;
            _mappingItems = new ObservableCollection<MappingItemViewModel>();
            _availableProperties = new ObservableCollection<string>();

            ExtractAvailableProperties();
            
            // Add initial item
            AddMappingItem();
        }

        public string DialogTitle => $"Map Array Items: {_targetArrayNode.Key}";

        public ObservableCollection<MappingItemViewModel> MappingItems
        {
            get => _mappingItems;
            set
            {
                _mappingItems = value;
                NotifyOfPropertyChange(() => MappingItems);
            }
        }

        public string PreviewQuery
        {
            get => _previewQuery;
            set
            {
                _previewQuery = value;
                NotifyOfPropertyChange(() => PreviewQuery);
            }
        }

        private void ExtractAvailableProperties()
        {
            bool propertiesFound = false;
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
                            propertiesFound = true;
                        }
                    }
                }
            }
            
            // Always add @ (Current Item)
            _availableProperties.Add("@");
            
            // If no child properties found (e.g. array of primitives), @ is the only option
        }

        public void AddMappingItem()
        {
            var item = new MappingItemViewModel(this, _availableProperties);
            MappingItems.Add(item);
            UpdatePreview();
        }

        public void RemoveMappingItem(MappingItemViewModel item)
        {
            if (MappingItems.Contains(item))
            {
                MappingItems.Remove(item);
                UpdatePreview();
            }
        }

        public void UpdatePreview()
        {
            if (MappingItems == null || !MappingItems.Any())
            {
                PreviewQuery = "";
                return;
            }

            var projections = new List<string>();
            foreach (var item in MappingItems)
            {
                var source = item.SourceProperty;
                if (string.IsNullOrWhiteSpace(source)) continue;

                var valueExpr = source;
                if (item.SelectedFunction != "None" && !string.IsNullOrEmpty(item.SelectedFunction))
                {
                    valueExpr = $"{item.SelectedFunction}({source})";
                }

                var target = string.IsNullOrWhiteSpace(item.TargetProperty) ? source : item.TargetProperty;
                if (target == "@") target = "Item"; // Default name if mapping raw item

                // Quote key if necessary
                if (target.Contains(" ") || target.Contains("-") || target.Any(c => !char.IsLetterOrDigit(c)))
                {
                     target = $"\"{target}\"";
                }

                projections.Add($"{target}: {valueExpr}");
            }

            if (projections.Count == 0)
            {
                PreviewQuery = "";
                return;
            }

            var projectionStr = string.Join(", ", projections);
            var query = string.IsNullOrEmpty(_targetArrayPath) ? "[*]" : $"{_targetArrayPath}[*]";
            
            PreviewQuery = $"{query}.{{{projectionStr}}}";
        }
    }
}
