using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using System;
using System.Collections.ObjectModel;

namespace JmesPathWpfDemo.ViewModels
{
    public class FunctionReferenceViewModel : Screen
    {
        private readonly Action<string, string> _onTryExample;
        private ObservableCollection<FunctionCategory> _categories;
        private FunctionCategory _selectedCategory;
        private FunctionInfo _selectedFunction;

        public FunctionReferenceViewModel(Action<string, string> onTryExample)
        {
            _onTryExample = onTryExample;
            InitializeCategories();
        }

        public ObservableCollection<FunctionCategory> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                NotifyOfPropertyChange(() => Categories);
            }
        }

        public FunctionCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                NotifyOfPropertyChange(() => SelectedCategory);
                SelectedFunction = null;
            }
        }

        public FunctionInfo SelectedFunction
        {
            get => _selectedFunction;
            set
            {
                _selectedFunction = value;
                NotifyOfPropertyChange(() => SelectedFunction);
            }
        }

        public void TryExample(FunctionExample example)
        {
            if (example != null && _onTryExample != null)
            {
                _onTryExample(example.JsonData, example.Query);
            }
        }

        private void InitializeCategories()
        {
            _categories = new ObservableCollection<FunctionCategory>
            {
                CreateArrayFunctionsCategory(),
                CreateStringFunctionsCategory(),
                CreateNumberFunctionsCategory(),
                CreateObjectFunctionsCategory(),
                CreateConversionFunctionsCategory(),
                CreateCustomFunctionsCategory(),
                CreateLogicalFunctionsCategory(),
                CreateDateTimeFunctionsCategory()
            };

            if (_categories.Count > 0)
            {
                SelectedCategory = _categories[0];
            }
        }

        private FunctionCategory CreateArrayFunctionsCategory()
        {
            return new FunctionCategory
            {
                CategoryName = "Array Functions",
                Description = "Functions for working with arrays",
                Functions = new System.Collections.Generic.List<FunctionInfo>
                {
                    new FunctionInfo
                    {
                        Name = "length",
                        Description = "Returns the length of an array or object",
                        Syntax = "length(array|object|string)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Get array length",
                                JsonData = @"{
  ""users"": [
    {""name"": ""Alice"", ""age"": 30},
    {""name"": ""Bob"", ""age"": 25},
    {""name"": ""Charlie"", ""age"": 35}
  ]
}",
                                Query = "length(users)",
                                ExpectedResult = "3"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "sort",
                        Description = "Sorts an array of strings or numbers",
                        Syntax = "sort(array)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Sort numbers",
                                JsonData = @"{
  ""numbers"": [5, 2, 8, 1, 9, 3]
}",
                                Query = "sort(numbers)",
                                ExpectedResult = "[1, 2, 3, 5, 8, 9]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "sort_by",
                        Description = "Sorts an array of objects by a specific field",
                        Syntax = "sort_by(array, &expression)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Sort by age",
                                JsonData = @"{
  ""users"": [
    {""name"": ""Alice"", ""age"": 30},
    {""name"": ""Bob"", ""age"": 25},
    {""name"": ""Charlie"", ""age"": 35}
  ]
}",
                                Query = "sort_by(users, &age)",
                                ExpectedResult = @"[
  {""name"": ""Bob"", ""age"": 25},
  {""name"": ""Alice"", ""age"": 30},
  {""name"": ""Charlie"", ""age"": 35}
]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "reverse",
                        Description = "Reverses the order of elements in an array",
                        Syntax = "reverse(array)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Reverse array",
                                JsonData = @"{
  ""numbers"": [1, 2, 3, 4, 5]
}",
                                Query = "reverse(numbers)",
                                ExpectedResult = "[5, 4, 3, 2, 1]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "max",
                        Description = "Returns the maximum value from an array of numbers",
                        Syntax = "max(array)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Find maximum",
                                JsonData = @"{
  ""scores"": [85, 92, 78, 95, 88]
}",
                                Query = "max(scores)",
                                ExpectedResult = "95"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "min",
                        Description = "Returns the minimum value from an array of numbers",
                        Syntax = "min(array)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Find minimum",
                                JsonData = @"{
  ""scores"": [85, 92, 78, 95, 88]
}",
                                Query = "min(scores)",
                                ExpectedResult = "78"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "sum",
                        Description = "Returns the sum of all numbers in an array",
                        Syntax = "sum(array)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Calculate total",
                                JsonData = @"{
  ""prices"": [10.5, 20.0, 15.75, 8.25]
}",
                                Query = "sum(prices)",
                                ExpectedResult = "54.5"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "avg",
                        Description = "Returns the average of all numbers in an array",
                        Syntax = "avg(array)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Calculate average score",
                                JsonData = @"{
  ""scores"": [85, 92, 78, 95, 88]
}",
                                Query = "avg(scores)",
                                ExpectedResult = "87.6"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "join",
                        Description = "Joins array elements into a string with a separator",
                        Syntax = "join(separator, array)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Join names with comma",
                                JsonData = @"{
  ""users"": [
    {""name"": ""Alice""},
    {""name"": ""Bob""},
    {""name"": ""Charlie""}
  ]
}",
                                Query = "join(', ', users[*].name)",
                                ExpectedResult = "\"Alice, Bob, Charlie\""
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "map",
                        Description = "Projects array elements using an expression",
                        Syntax = "array[*].expression",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Extract all names",
                                JsonData = @"{
  ""users"": [
    {""name"": ""Alice"", ""age"": 30},
    {""name"": ""Bob"", ""age"": 25}
  ]
}",
                                Query = "users[*].name",
                                ExpectedResult = "[\"Alice\", \"Bob\"]"
                            }
                        }
                    }
                }
            };
        }

        private FunctionCategory CreateStringFunctionsCategory()
        {
            return new FunctionCategory
            {
                CategoryName = "String Functions",
                Description = "Functions for string manipulation",
                Functions = new System.Collections.Generic.List<FunctionInfo>
                {
                    new FunctionInfo
                    {
                        Name = "concat (custom)",
                        Description = "Concatenates multiple strings together",
                        Syntax = "concat(string1, string2, ...)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Combine first and last name",
                                JsonData = @"{
  ""person"": {
    ""firstName"": ""John"",
    ""lastName"": ""Doe""
  }
}",
                                Query = "concat(person.firstName, ' ', person.lastName)",
                                ExpectedResult = "\"John Doe\""
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "concat_ws (custom)",
                        Description = "Concatenates strings with a separator",
                        Syntax = "concat_ws(separator, string1, string2, ...)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Join with separator",
                                JsonData = @"{
  ""address"": {
    ""street"": ""123 Main St"",
    ""city"": ""Seattle"",
    ""state"": ""WA"",
    ""zip"": ""98101""
  }
}",
                                Query = "concat_ws(', ', address.street, address.city, address.state, address.zip)",
                                ExpectedResult = "\"123 Main St, Seattle, WA, 98101\""
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "starts_with",
                        Description = "Checks if a string starts with a prefix",
                        Syntax = "starts_with(string, prefix)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Filter names starting with 'A'",
                                JsonData = @"{
  ""names"": [""Alice"", ""Bob"", ""Andrew"", ""Charlie""]
}",
                                Query = "names[?starts_with(@, 'A')]",
                                ExpectedResult = "[\"Alice\", \"Andrew\"]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "ends_with",
                        Description = "Checks if a string ends with a suffix",
                        Syntax = "ends_with(string, suffix)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Filter emails ending with gmail.com",
                                JsonData = @"{
  ""emails"": [""alice@gmail.com"", ""bob@yahoo.com"", ""charlie@gmail.com""]
}",
                                Query = "emails[?ends_with(@, 'gmail.com')]",
                                ExpectedResult = "[\"alice@gmail.com\", \"charlie@gmail.com\"]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "contains",
                        Description = "Checks if a string or array contains a value",
                        Syntax = "contains(haystack, needle)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Check if string contains text",
                                JsonData = @"{
  ""message"": ""Hello World""
}",
                                Query = "contains(message, 'World')",
                                ExpectedResult = "true"
                            }
                        }
                    }
                }
            };
        }

        private FunctionCategory CreateNumberFunctionsCategory()
        {
            return new FunctionCategory
            {
                CategoryName = "Number Functions",
                Description = "Mathematical functions for numbers",
                Functions = new System.Collections.Generic.List<FunctionInfo>
                {
                    new FunctionInfo
                    {
                        Name = "abs",
                        Description = "Returns the absolute value of a number",
                        Syntax = "abs(number)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Get absolute values",
                                JsonData = @"{
  ""temperature"": -15
}",
                                Query = "abs(temperature)",
                                ExpectedResult = "15"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "ceil",
                        Description = "Rounds a number up to the nearest integer",
                        Syntax = "ceil(number)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Round up price",
                                JsonData = @"{
  ""price"": 19.99
}",
                                Query = "ceil(price)",
                                ExpectedResult = "20"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "floor",
                        Description = "Rounds a number down to the nearest integer",
                        Syntax = "floor(number)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Round down value",
                                JsonData = @"{
  ""value"": 19.99
}",
                                Query = "floor(value)",
                                ExpectedResult = "19"
                            }
                        }
                    }
                }
            };
        }

        private FunctionCategory CreateObjectFunctionsCategory()
        {
            return new FunctionCategory
            {
                CategoryName = "Object Functions",
                Description = "Functions for working with objects",
                Functions = new System.Collections.Generic.List<FunctionInfo>
                {
                    new FunctionInfo
                    {
                        Name = "keys",
                        Description = "Returns an array of all keys in an object",
                        Syntax = "keys(object)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Get all property names",
                                JsonData = @"{
  ""person"": {
    ""name"": ""Alice"",
    ""age"": 30,
    ""city"": ""Seattle""
  }
}",
                                Query = "keys(person)",
                                ExpectedResult = "[\"name\", \"age\", \"city\"]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "values",
                        Description = "Returns an array of all values in an object",
                        Syntax = "values(object)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Get all property values",
                                JsonData = @"{
  ""person"": {
    ""name"": ""Alice"",
    ""age"": 30,
    ""city"": ""Seattle""
  }
}",
                                Query = "values(person)",
                                ExpectedResult = "[\"Alice\", 30, \"Seattle\"]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "merge",
                        Description = "Merges multiple objects into one",
                        Syntax = "merge(object1, object2, ...)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Combine two objects",
                                JsonData = @"{
  ""defaults"": {""color"": ""blue"", ""size"": ""medium""},
  ""custom"": {""size"": ""large"", ""style"": ""modern""}
}",
                                Query = "merge(defaults, custom)",
                                ExpectedResult = @"{""color"": ""blue"", ""size"": ""large"", ""style"": ""modern""}"
                            }
                        }
                    }
                }
            };
        }

        private FunctionCategory CreateConversionFunctionsCategory()
        {
            return new FunctionCategory
            {
                CategoryName = "Type Conversion",
                Description = "Functions for converting between types",
                Functions = new System.Collections.Generic.List<FunctionInfo>
                {
                    new FunctionInfo
                    {
                        Name = "to_string",
                        Description = "Converts a value to string",
                        Syntax = "to_string(value)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Convert number to string",
                                JsonData = @"{
  ""count"": 42
}",
                                Query = "to_string(count)",
                                ExpectedResult = "\"42\""
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "to_number",
                        Description = "Converts a string to number",
                        Syntax = "to_number(string)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Convert string to number",
                                JsonData = @"{
  ""price"": ""19.99""
}",
                                Query = "to_number(price)",
                                ExpectedResult = "19.99"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "type",
                        Description = "Returns the type of a value",
                        Syntax = "type(value)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Check value type",
                                JsonData = @"{
  ""values"": [42, ""hello"", true, null, [1, 2], {""key"": ""value""}]
}",
                                Query = "values[*].type(@)",
                                ExpectedResult = "[\"number\", \"string\", \"boolean\", \"null\", \"array\", \"object\"]"
                            }
                        }
                    }
                }
            };
        }

        private FunctionCategory CreateCustomFunctionsCategory()
        {
            return new FunctionCategory
            {
                CategoryName = "Custom Functions",
                Description = "Custom functions specific to this application",
                Functions = new System.Collections.Generic.List<FunctionInfo>
                {
                    new FunctionInfo
                    {
                        Name = "merge_arrays",
                        Description = "Merges multiple arrays into one",
                        Syntax = "merge_arrays(array1, array2, ...)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Combine two arrays",
                                JsonData = @"{
  ""list1"": [1, 2, 3],
  ""list2"": [4, 5, 6]
}",
                                Query = "merge_arrays(list1, list2)",
                                ExpectedResult = "[1, 2, 3, 4, 5, 6]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "flatten",
                        Description = "Flattens nested arrays recursively",
                        Syntax = "flatten(array)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Flatten nested arrays",
                                JsonData = @"{
  ""nested"": [[1, 2], [3, [4, 5]], 6]
}",
                                Query = "flatten(nested)",
                                ExpectedResult = "[1, 2, 3, 4, 5, 6]"
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "newline",
                        Description = "Returns a newline character",
                        Syntax = "newline()",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Join with newlines",
                                JsonData = @"{
  ""lines"": [""First line"", ""Second line"", ""Third line""]
}",
                                Query = "join(newline(), lines)",
                                ExpectedResult = "\"First line\\nSecond line\\nThird line\""
                            }
                        }
                    }
                }
            };
        }

        private FunctionCategory CreateLogicalFunctionsCategory()
        {
            return new FunctionCategory
            {
                CategoryName = "Logical Functions",
                Description = "Functions for conditional logic",
                Functions = new System.Collections.Generic.List<FunctionInfo>
                {
                    new FunctionInfo
                    {
                        Name = "not_null",
                        Description = "Returns the first non-null value",
                        Syntax = "value1 || value2 || ...",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Get first non-null value",
                                JsonData = @"{
  ""user"": {
    ""nickname"": null,
    ""username"": ""john_doe""
  }
}",
                                Query = "user.nickname || user.username",
                                ExpectedResult = "\"john_doe\""
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "iff (custom)",
                        Description = "If-then-else conditional function",
                        Syntax = "iff(condition, true_value, false_value)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Conditional value",
                                JsonData = @"{
  ""person"": {
    ""age"": 17
  }
}",
                                Query = "iff(person.age >= `18`, 'Adult', 'Minor')",
                                ExpectedResult = "\"Minor\""
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "eq (custom)",
                        Description = "Checks if two values are equal",
                        Syntax = "eq(value1, value2)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Compare values",
                                JsonData = @"{
  ""expected"": 42,
  ""actual"": 42
}",
                                Query = "eq(expected, actual)",
                                ExpectedResult = "true"
                            }
                        }
                    }
                }
            };
        }

        private FunctionCategory CreateDateTimeFunctionsCategory()
        {
            return new FunctionCategory
            {
                CategoryName = "Date/Time Functions",
                Description = "Functions for date and time operations",
                Functions = new System.Collections.Generic.List<FunctionInfo>
                {
                    new FunctionInfo
                    {
                        Name = "todate (custom)",
                        Description = "Converts and formats date strings between different formats",
                        Syntax = "todate(value, source_format, target_format)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Convert date format (MM/dd/yyyy to yyyy-MM-dd)",
                                JsonData = @"{
  ""events"": [
    {""name"": ""Meeting"", ""date"": ""01/15/2024""},
    {""name"": ""Call"", ""date"": ""02/20/2024""}
  ]
}",
                                Query = "todate(events[*].date, 'MM/dd/yyyy', 'yyyy-MM-dd')",
                                ExpectedResult = @"[
  ""2024-01-15"",
  ""2024-02-20""
]"
                            },
                            new FunctionExample
                            {
                                Description = "Auto-detect source format and use default output",
                                JsonData = @"{
  ""dates"": [""2024-01-15 14:30:00"", ""2024-02-20 09:15:00""]
}",
                                Query = "todate(dates, '', '')",
                                ExpectedResult = @"[
  ""2024-01-15"",
  ""2024-02-20""
]"
                            },
                            new FunctionExample
                            {
                                Description = "Custom output format (long date)",
                                JsonData = @"{
  ""eventDate"": ""2024-01-15""
}",
                                Query = "todate(eventDate, 'yyyy-MM-dd', 'MMMM dd, yyyy')",
                                ExpectedResult = "\"January 15, 2024\""
                            }
                        }
                    },
                    new FunctionInfo
                    {
                        Name = "todatetime (custom)",
                        Description = "Converts and formats date/time with timezone conversion",
                        Syntax = "todatetime(value, format, from_tz, to_tz)",
                        Examples = new System.Collections.Generic.List<FunctionExample>
                        {
                            new FunctionExample
                            {
                                Description = "Convert timezone",
                                JsonData = @"{
  ""events"": [
    {""name"": ""Meeting"", ""time"": ""2024-01-15 14:00:00""},
    {""name"": ""Call"", ""time"": ""2024-01-15 16:30:00""}
  ]
}",
                                Query = "todatetime(events[*].time, 'yyyy-MM-dd HH:mm:ss', 'Central Standard Time', 'Pacific Standard Time')",
                                ExpectedResult = @"[
  ""2024-01-15 12:00:00"",
  ""2024-01-15 14:30:00""
]"
                            }
                        }
                    }
                }
            };
        }
    }
}
