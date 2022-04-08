using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace VSIXProject1
{
    /// <summary>
    /// Interaction logic for ToolboxControl2.xaml.
    /// </summary>
    [ProvideToolboxControl("VSIXProject1.ToolboxControl2", true)]
    public partial class ToolboxControl2 : UserControl
    {
        public ToolboxControl2()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "We are inside {0}.Button1_Click()", this.ToString()));
        }
    }
}
