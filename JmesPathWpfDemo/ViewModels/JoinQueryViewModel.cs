using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using System.Collections.Generic;
using System.Linq;

namespace JmesPathWpfDemo.ViewModels
{
    public class JoinQueryViewModel : Screen
    {
        private string _separator;
        private string _selectedProperty;
        private SavedQuery _selectedSavedQuery;
        private bool _usePipeline;
        private List<string> _properties;
        private List<SavedQuery> _savedQueries;
        private string _previewText;
        private string _separatorText = ", ";

        public JoinQueryViewModel(List<string> properties, List<SavedQuery> savedQueries)
        {
            _properties = properties ?? new List<string>();
            _savedQueries = savedQueries ?? new List<SavedQuery>();
            
            if (_properties.Any())
            {
                SelectedProperty = _properties.First();
            }
            
            if (_savedQueries.Any())
            {
                SelectedSavedQuery = _savedQueries.First();
            }

            UpdatePreview();
        }

        public List<string> Properties => _properties;
        public List<SavedQuery> SavedQueries => _savedQueries;

        public bool HasProperties => _properties.Any();
        public bool HasSavedQueries => _savedQueries.Any();

        public string SeparatorText
        {
            get => _separatorText;
            set
            {
                _separatorText = value;
                NotifyOfPropertyChange(() => SeparatorText);
                UpdatePreview();
            }
        }

        public string SelectedProperty
        {
            get => _selectedProperty;
            set
            {
                _selectedProperty = value;
                NotifyOfPropertyChange(() => SelectedProperty);
                UpdatePreview();
            }
        }

        public SavedQuery SelectedSavedQuery
        {
            get => _selectedSavedQuery;
            set
            {
                _selectedSavedQuery = value;
                NotifyOfPropertyChange(() => SelectedSavedQuery);
                UpdatePreview();
            }
        }

        public bool UsePipeline
        {
            get => _usePipeline;
            set
            {
                _usePipeline = value;
                NotifyOfPropertyChange(() => UsePipeline);
                UpdatePreview();
            }
        }

        public string PreviewText
        {
            get => _previewText;
            set
            {
                _previewText = value;
                NotifyOfPropertyChange(() => PreviewText);
            }
        }

        public void AddSeparator(string separator)
        {
            SeparatorText = separator; // JoinQueryDialog behavior seems to replace or append? 
            // In JoinQueryDialog.xaml.cs: SeparatorTextBox.SelectedText = sep; (inserts at cursor)
            // For simplicity in ViewModel, let's append or set. 
            // The original logic was complex (insert at caret).
            // Users usually want to *set* the separator or append.
            // Let's implement Append for now or just Set?
            // "SeparatorTextBox.SelectedText = sep" -> This is View logic (caret).
            // If we want to move to VM, we might simplify to "Append" or "Set".
            // Let's simplify to "Set" because typically separator is simple.
            // Or "Append" if it's complex. 
            // Let's Append.
            // Wait, old code: "SeparatorTextBox.SelectedText = sep". It replaces selection or inserts.
            // Without passing CaretIndex, we can't do this easily in VM.
            // Let's just Append for MVP refactor.
            // SeparatorText += separator;
            // But wait, the Separator is usually short, e.g. ", ".
            // If I click "Comma", it inserts comma.
            SeparatorText += separator;
        }

        private void UpdatePreview()
        {
            var preview = "Preview:\n";
            var sep = string.IsNullOrEmpty(SeparatorText) ? "" : SeparatorText;
            
            if (UsePipeline && SelectedSavedQuery != null)
            {
                preview += $"({SelectedSavedQuery.Expression}) | join('{sep}', @)";
            }
            else
            {
                preview += $"join('{sep}', @)";
            }

            PreviewText = preview;
        }
    }
}
