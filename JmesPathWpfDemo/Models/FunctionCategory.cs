using System.Collections.Generic;

namespace JmesPathWpfDemo.Models
{
    public class FunctionCategory
    {
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public List<FunctionInfo> Functions { get; set; } = new List<FunctionInfo>();
    }

    public class FunctionInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Syntax { get; set; }
        public List<FunctionExample> Examples { get; set; } = new List<FunctionExample>();
    }

    public class FunctionExample
    {
        public string Description { get; set; }
        public string JsonData { get; set; }
        public string Query { get; set; }
        public string ExpectedResult { get; set; }
    }
}
