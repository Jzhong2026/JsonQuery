using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using JmesPathWpfDemo.Models;

namespace JmesPathWpfDemo.ViewModels
{
    public enum ParameterType
    {
        StaticString,
        Number,
        SavedQuery,
        NewLine,
        Space
    }

    public class QueryParameterViewModel : PropertyChangedBase
    {
        private ParameterType _type;
        private string _staticValue;
        private SavedQuery _selectedSavedQuery;
        private ObservableCollection<SavedQuery> _availableQueries;

        public QueryParameterViewModel(ObservableCollection<SavedQuery> availableQueries)
        {
            _availableQueries = availableQueries;
            _type = ParameterType.SavedQuery;
            if (_availableQueries.Any())
                _selectedSavedQuery = _availableQueries.First();
        }

        public ParameterType Type
        {
            get => _type;
            set
            {
                _type = value;
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => IsStaticInputVisible);
                NotifyOfPropertyChange(() => IsNumberInputVisible);
                NotifyOfPropertyChange(() => IsSavedQueryVisible);
                NotifyOfPropertyChange(() => IsNewLineVisible);
                NotifyOfPropertyChange(() => IsSpaceVisible);
                Parent?.NotifyQueryChange();
            }
        }

        public string StaticValue
        {
            get => _staticValue;
            set
            {
                _staticValue = value;
                NotifyOfPropertyChange(() => StaticValue);
                Parent?.NotifyQueryChange();
            }
        }

        public SavedQuery SelectedSavedQuery
        {
            get => _selectedSavedQuery;
            set
            {
                _selectedSavedQuery = value;
                NotifyOfPropertyChange(() => SelectedSavedQuery);
                Parent?.NotifyQueryChange();
            }
        }

        public ObservableCollection<SavedQuery> AvailableQueries => _availableQueries;

        public bool IsStaticInputVisible => Type == ParameterType.StaticString;
        public bool IsNumberInputVisible => Type == ParameterType.Number;
        public bool IsSavedQueryVisible => Type == ParameterType.SavedQuery;
        public bool IsNewLineVisible => Type == ParameterType.NewLine;
        public bool IsSpaceVisible => Type == ParameterType.Space;

        public ComplexQueryBuilderViewModel Parent { get; set; }

        public string GetExpression()
        {
            switch (Type)
            {
                case ParameterType.SavedQuery:
                    return SelectedSavedQuery?.Expression ?? "null";
                case ParameterType.StaticString:
                    return $"'{StaticValue?.Replace("'", "\\'")}'";
                case ParameterType.Number:
                    // Return the number with backticks for JMESPath literal syntax
                    var numValue = string.IsNullOrWhiteSpace(StaticValue) ? "0" : StaticValue.Trim();
                    return $"`{numValue}`";
                case ParameterType.NewLine:
                    return "newline()";
                case ParameterType.Space:
                    return "' '";
                default:
                    return "null";
            }
        }
    }

    public class FunctionDefinition
    {
        public string Name { get; set; }
        public bool IsVariadic { get; set; }
        public int ParameterCount { get; set; } // If not variadic
    }

    public class ComplexQueryBuilderViewModel : Screen
    {
        private ObservableCollection<SavedQuery> _savedQueries;
        private ObservableCollection<FunctionDefinition> _functions;
        private FunctionDefinition _selectedFunction;
        private ObservableCollection<QueryParameterViewModel> _parameters;
        private string _resultQuery;

        public ComplexQueryBuilderViewModel(ObservableCollection<SavedQuery> savedQueries)
        {
            _savedQueries = savedQueries;
            _parameters = new ObservableCollection<QueryParameterViewModel>();
            
            _functions = new ObservableCollection<FunctionDefinition>
            {
                new FunctionDefinition { Name = "concat", IsVariadic = true },
                new FunctionDefinition { Name = "to_string", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "to_number", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "length", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "abs", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "avg", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "ceil", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "floor", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "max", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "min", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "sum", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "type", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "iff", IsVariadic = false, ParameterCount = 3 },
                new FunctionDefinition { Name = "eq", IsVariadic = false, ParameterCount = 2 },
            };



            SelectedFunction = _functions.First();
        }

        public ObservableCollection<FunctionDefinition> Functions => _functions;

        public FunctionDefinition SelectedFunction
        {
            get => _selectedFunction;
            set
            {
                _selectedFunction = value;
                NotifyOfPropertyChange(() => SelectedFunction);
                NotifyOfPropertyChange(() => IsAddParameterVisible);
                SetupParameters();
                NotifyQueryChange();
            }
        }

        public ObservableCollection<QueryParameterViewModel> Parameters
        {
            get => _parameters;
            set
            {
                _parameters = value;
                NotifyOfPropertyChange(() => Parameters);
            }
        }

        public string ResultQuery
        {
            get => _resultQuery;
            set
            {
                _resultQuery = value;
                NotifyOfPropertyChange(() => ResultQuery);
            }
        }

        public bool IsAddParameterVisible => SelectedFunction?.IsVariadic ?? false;

        private void SetupParameters()
        {
            Parameters.Clear();
            if (SelectedFunction.IsVariadic)
            {
                // Start with 2 parameters for variadic like concat
                AddParameter();
                AddParameter();
            }
            else
            {
                for (int i = 0; i < SelectedFunction.ParameterCount; i++)
                {
                    AddParameter();
                }
            }
        }

        public void AddParameter()
        {
            var param = new QueryParameterViewModel(_savedQueries) { Parent = this };
            Parameters.Add(param);
            NotifyQueryChange();
        }

        public void RemoveParameter(QueryParameterViewModel param)
        {
            if (Parameters.Contains(param))
            {
                Parameters.Remove(param);
                NotifyQueryChange();
            }
        }

        public void NotifyQueryChange()
        {
            if (SelectedFunction == null) return;

            var args = string.Join(", ", Parameters.Select(p => p.GetExpression()));
            ResultQuery = $"{SelectedFunction.Name}({args})";
        }

        public void Ok()
        {
            TryCloseAsync(true);
        }

        public void Cancel()
        {
            TryCloseAsync(false);
        }
    }
}
