using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.UI.ViewModels;
using System;
using System.Windows.Media;

namespace CodeCloud.VisualStudio.UI.Views
{
    /// <summary>
    /// Interaction logic for CloneView.xaml
    /// </summary>
    public partial class CloneView : Dialog
    {
        public CloneView(IMessenger messenger, IShellService shell, IStorage storage, IWebService web)
        {
            InitializeComponent();

            var vm = new CloneViewModel(this, messenger, shell, storage, web);

            DataContext = vm;
        }
    }
}
