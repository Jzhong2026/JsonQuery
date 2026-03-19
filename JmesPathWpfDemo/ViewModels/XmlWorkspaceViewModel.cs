using Caliburn.Micro;
using System.Windows;

namespace JmesPathWpfDemo.ViewModels
{
    public class XmlWorkspaceViewModel : Screen
    {
        private string _currentView = "XmlQuery";
        
        public XmlWorkspaceViewModel()
        {
        }

        public string CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                NotifyOfPropertyChange(() => CurrentView);
            }
        }
    }
}