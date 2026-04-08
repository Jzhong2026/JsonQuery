using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace JmesPathWpfDemo.Views
{
    public partial class XmlWorkspaceView : UserControl
    {
        public XmlWorkspaceView()
        {
            InitializeComponent();
        }

        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;

            if (originalSource?.TemplatedParent is ToggleButton)
                return;

            if (originalSource is TextBlock)
                return;
        }
    }
}
