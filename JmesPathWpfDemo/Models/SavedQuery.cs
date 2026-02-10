namespace JmesPathWpfDemo.Models
{
    public class SavedQuery
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Expression { get; set; }

        public string DisplayText => $"{Name} - {Description}";
    }
}
