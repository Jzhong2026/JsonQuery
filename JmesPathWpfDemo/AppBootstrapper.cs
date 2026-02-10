using Caliburn.Micro;
using System.Windows;
using JmesPathWpfDemo.ViewModels;

namespace JmesPathWpfDemo
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper() { Initialize(); }
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewForAsync<ShellViewModel>();
        }
    }
}
