using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using JmesPathWpfDemo.Views;

namespace JmesPathWpfDemo.ViewModels
{
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly JsonWorkspaceViewModel _jsonWorkspace;
        private readonly XmlWorkspaceViewModel _xmlWorkspace;

        public ShellViewModel()
        {
            _jsonWorkspace = new JsonWorkspaceViewModel();
            _xmlWorkspace = new XmlWorkspaceViewModel();
            
            Items.Add(_jsonWorkspace);
            Items.Add(_xmlWorkspace);
            
            ActivateItemAsync(_jsonWorkspace);
        }

        public JsonWorkspaceViewModel JsonWorkspace => _jsonWorkspace;
        public XmlWorkspaceViewModel XmlWorkspace => _xmlWorkspace;

        public bool IsJsonWorkspaceActive
        {
            get => ActiveItem == _jsonWorkspace;
            set 
            { 
                if (value) 
                {
                    ActivateItemAsync(_jsonWorkspace); 
                    NotifyOfPropertyChange(() => IsJsonWorkspaceActive);
                    NotifyOfPropertyChange(() => IsXmlWorkspaceActive);
                } 
            }
        }

        public bool IsXmlWorkspaceActive
        {
            get => ActiveItem == _xmlWorkspace;
            set 
            { 
                if (value) 
                {
                    ActivateItemAsync(_xmlWorkspace); 
                    NotifyOfPropertyChange(() => IsJsonWorkspaceActive);
                    NotifyOfPropertyChange(() => IsXmlWorkspaceActive);
                } 
            }
        }

        public void ConfigureClientTimeZone()
        {
            var dialog = new ClientTimeZoneSettingsDialog(ClientTimeZoneSettingsService.Current);
            dialog.Owner = System.Windows.Application.Current?.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                ClientTimeZoneSettingsService.Save(new ClientTimeZoneSettings
                {
                    TimeZoneId = dialog.SelectedTimeZoneId,
                    RespectDaylightSavings = dialog.RespectDaylightSavings
                });
            }
        }
    }
}