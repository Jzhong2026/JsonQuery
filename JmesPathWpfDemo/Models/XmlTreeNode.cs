using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace JmesPathWpfDemo.Models
{
    public class XmlTreeNode : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isExpanded;
        private string _sortKey;
        private bool _sortAscending = true;

        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public XmlTreeNode Parent { get; set; }

        public ObservableCollection<XmlTreeNode> Children { get; set; } = new ObservableCollection<XmlTreeNode>();
        public ObservableCollection<XmlTreeNode> Attributes { get; set; } = new ObservableCollection<XmlTreeNode>();

        public string SortKey
        {
            get => _sortKey;
            set
            {
                if (_sortKey != value)
                {
                    _sortKey = value;
                    OnPropertyChanged(nameof(SortKey));
                    OnPropertyChanged(nameof(HasSortApplied));
                }
            }
        }

        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                if (_sortAscending != value)
                {
                    _sortAscending = value;
                    OnPropertyChanged(nameof(SortAscending));
                    OnPropertyChanged(nameof(HasSortApplied));
                }
            }
        }

        public bool HasSortApplied => !string.IsNullOrEmpty(SortKey);

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
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
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public bool HasChildren => Children != null && Children.Count > 0;
        public bool HasAttributes => Attributes != null && Attributes.Count > 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}