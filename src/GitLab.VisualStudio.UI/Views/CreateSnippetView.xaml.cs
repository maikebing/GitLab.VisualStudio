using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GitLab.VisualStudio.UI.Views
{
    /// <summary>
    /// CreateSnippet.xaml 的交互逻辑
    /// </summary>
    public partial class CreateSnippet : Dialog
    {
        public CreateSnippet(IMessenger messenger, IShellService shell, IStorage storage, IWebService web)
        {
            InitializeComponent();
            var vm = new CreateSnippetViewModel(this, messenger, shell, storage, web);
            DataContext = vm;
        }

        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
