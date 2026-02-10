using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using System;
using System.Collections.Generic;

namespace JmesPathWpfDemo.ViewModels
{
    public enum ParameterType
    {
        SavedQuery,
        StaticString,
        Number,
        NewLine,
        Space,
        TreePath,
        ArrayExpression,
        Separator,
        CurrentTabField,
        ExpressionBuilder
    }

    public enum ComparisonOperator
    {
        Equal,              // eq(a, b)
        NotEqual,           // !eq(a, b)
        GreaterThan,        // a > b (using comparison in filter)
        LessThan,           // a < b
        GreaterThanOrEqual, // a >= b
        LessThanOrEqual,    // a <= b
        Contains,           // contains(a, b)
        StartsWith,         // starts_with(a, b)
        EndsWith            // ends_with(a, b)
    }

    public class QueryParameterViewModel : PropertyChangedBase
    {
        private ParameterType _type;
        private string _staticValue;
        private SavedQuery _selectedSavedQuery;
        private ObservableCollection<SavedQuery> _availableQueries;
        private ObservableCollection<string> _availableFields;
        private string _selectedField;
        
        // For Expression Builder
        private string _expressionField;
        private ComparisonOperator _expressionOperator;
        private string _expressionValue;
        private bool _expressionValueIsField;

        public QueryParameterViewModel(ObservableCollection<SavedQuery> availableQueries, ObservableCollection<string> availableFields)
        {
            _availableQueries = availableQueries;
            _availableFields = availableFields;
            _type = ParameterType.SavedQuery;
            if (_availableQueries.Any())
                _selectedSavedQuery = _availableQueries.First();
            if (_availableFields.Any())
                _selectedField = _availableFields.First();
            
            _expressionOperator = ComparisonOperator.Equal;
            _expressionValue = "";
            _expressionValueIsField = false;
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
                NotifyOfPropertyChange(() => IsTreePathVisible);
                NotifyOfPropertyChange(() => IsArrayExpressionVisible);
                NotifyOfPropertyChange(() => IsSeparatorVisible);
                NotifyOfPropertyChange(() => IsCurrentTabFieldVisible);
                NotifyOfPropertyChange(() => IsExpressionBuilderVisible);
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

        public void SetStaticValue(string value)
        {
            StaticValue = value;
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

        public ObservableCollection<string> AvailableFields => _availableFields;

        public string SelectedField
        {
            get => _selectedField;
            set
            {
                _selectedField = value;
                NotifyOfPropertyChange(() => SelectedField);
                Parent?.NotifyQueryChange();
            }
        }

        // Expression Builder Properties
        public string ExpressionField
        {
            get => _expressionField;
            set
            {
                _expressionField = value;
                NotifyOfPropertyChange(() => ExpressionField);
                Parent?.NotifyQueryChange();
            }
        }

        public ComparisonOperator ExpressionOperator
        {
            get => _expressionOperator;
            set
            {
                _expressionOperator = value;
                NotifyOfPropertyChange(() => ExpressionOperator);
                Parent?.NotifyQueryChange();
            }
        }

        public string ExpressionValue
        {
            get => _expressionValue;
            set
            {
                _expressionValue = value;
                NotifyOfPropertyChange(() => ExpressionValue);
                Parent?.NotifyQueryChange();
            }
        }

        public bool ExpressionValueIsField
        {
            get => _expressionValueIsField;
            set
            {
                _expressionValueIsField = value;
                NotifyOfPropertyChange(() => ExpressionValueIsField);
                NotifyOfPropertyChange(() => ExpressionValueIsStatic);
                Parent?.NotifyQueryChange();
            }
        }

        public bool ExpressionValueIsStatic
        {
            get => !_expressionValueIsField;
            set
            {
                _expressionValueIsField = !value;
                NotifyOfPropertyChange(() => ExpressionValueIsField);
                NotifyOfPropertyChange(() => ExpressionValueIsStatic);
                Parent?.NotifyQueryChange();
            }
        }

        public bool IsStaticInputVisible => Type == ParameterType.StaticString;
        public bool IsNumberInputVisible => Type == ParameterType.Number;
        public bool IsSavedQueryVisible => Type == ParameterType.SavedQuery;
        public bool IsNewLineVisible => Type == ParameterType.NewLine;
        public bool IsSpaceVisible => Type == ParameterType.Space;
        public bool IsTreePathVisible => Type == ParameterType.TreePath;
        public bool IsArrayExpressionVisible => Type == ParameterType.ArrayExpression;
        public bool IsSeparatorVisible => Type == ParameterType.Separator;
        public bool IsCurrentTabFieldVisible => Type == ParameterType.CurrentTabField;
        public bool IsExpressionBuilderVisible => Type == ParameterType.ExpressionBuilder;

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
                    var numValue = string.IsNullOrWhiteSpace(StaticValue) ? "0" : StaticValue.Trim();
                    return $"`{numValue}`";
                case ParameterType.NewLine:
                    return "newline()";
                case ParameterType.Space:
                    return "' '";
                case ParameterType.TreePath:
                    return string.IsNullOrWhiteSpace(StaticValue) ? "@" : StaticValue.Trim();
                case ParameterType.ArrayExpression:
                    return string.IsNullOrWhiteSpace(StaticValue) ? "[]" : StaticValue.Trim();
                case ParameterType.Separator:
                    return $"'{StaticValue?.Replace("'", "\\'")}'";
                case ParameterType.CurrentTabField:
                    return string.IsNullOrWhiteSpace(SelectedField) ? "@" : SelectedField;
                case ParameterType.ExpressionBuilder:
                    return BuildExpressionFromBuilder();
                default:
                    return "null";
            }
        }

        private string BuildExpressionFromBuilder()
        {
            var field = string.IsNullOrWhiteSpace(ExpressionField) ? "@" : ExpressionField;
            var value = ExpressionValueIsField 
                ? ExpressionValue 
                : (IsNumeric(ExpressionValue) ? $"`{ExpressionValue}`" : $"'{ExpressionValue}'");

            switch (ExpressionOperator)
            {
                case ComparisonOperator.Equal:
                    return $"eq({field}, {value})";
                case ComparisonOperator.NotEqual:
                    return $"!eq({field}, {value})";
                case ComparisonOperator.Contains:
                    return $"contains({field}, {value})";
                case ComparisonOperator.StartsWith:
                    return $"starts_with({field}, {value})";
                case ComparisonOperator.EndsWith:
                    return $"ends_with({field}, {value})";
                case ComparisonOperator.GreaterThan:
                    return $"{field} > {value}";
                case ComparisonOperator.LessThan:
                    return $"{field} < {value}";
                case ComparisonOperator.GreaterThanOrEqual:
                    return $"{field} >= {value}";
                case ComparisonOperator.LessThanOrEqual:
                    return $"{field} <= {value}";
                default:
                    return $"eq({field}, {value})";
            }
        }

        private bool IsNumeric(string value)
        {
            return double.TryParse(value, out _);
        }
    }

    public class FunctionDefinition
    {
        public string Name { get; set; }
        public bool IsVariadic { get; set; }
        public int ParameterCount { get; set; }
    }

    public class ComplexQueryBuilderViewModel : Screen
    {
        private ObservableCollection<SavedQuery> _savedQueries;
        private ObservableCollection<string> _currentTabFields;
        private ObservableCollection<FunctionDefinition> _functions;
        private FunctionDefinition _selectedFunction;
        private ObservableCollection<QueryParameterViewModel> _parameters;
        private string _resultQuery;

        public ComplexQueryBuilderViewModel(ObservableCollection<SavedQuery> savedQueries, List<string> currentTabFields = null)
        {
            _savedQueries = savedQueries;
            _currentTabFields = currentTabFields != null 
                ? new ObservableCollection<string>(currentTabFields) 
                : new ObservableCollection<string>();
            _parameters = new ObservableCollection<QueryParameterViewModel>();
            
            _functions = new ObservableCollection<FunctionDefinition>
            {
                new FunctionDefinition { Name = "concat", IsVariadic = true },
                new FunctionDefinition { Name = "concat_ws", IsVariadic = true },
                new FunctionDefinition { Name = "merge_arrays", IsVariadic = true },
                new FunctionDefinition { Name = "flatten", IsVariadic = false, ParameterCount = 1 },
                new FunctionDefinition { Name = "join", IsVariadic = false, ParameterCount = 2 },
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

            if (SelectedFunction.Name == "join" || SelectedFunction.Name == "concat_ws")
            {
                if (Parameters.Count > 0)
                {
                    Parameters[0].Type = ParameterType.Separator;
                    Parameters[0].StaticValue = ", ";
                }
            }

            // Auto-configure iff function - only first parameter (condition) should be ExpressionBuilder
            if (SelectedFunction.Name == "iff" && Parameters.Count >= 3)
            {
                // Parameter 1: condition - use ExpressionBuilder
                Parameters[0].Type = ParameterType.ExpressionBuilder;
                if (_currentTabFields.Any())
                {
                    Parameters[0].ExpressionField = _currentTabFields.First();
                }
                
                // Parameter 2 & 3: true_value and false_value - use StaticString
                Parameters[1].Type = ParameterType.StaticString;
                Parameters[2].Type = ParameterType.StaticString;
            }
        }

        public void AddParameter()
        {
            var param = new QueryParameterViewModel(_savedQueries, _currentTabFields) { Parent = this };
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
