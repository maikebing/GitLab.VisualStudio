using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.UI.ViewModels;

namespace CodeCloud.VisualStudio.UI.Views
{
    /// <summary>
    /// Interaction logic for CreateView.xaml
    /// </summary>
    public partial class CreateView : Dialog
    {
        public CreateView(IGitService git, IMessenger messenger, IShellService shell, IStorage storage, IWebService web)
        {
            InitializeComponent();

            var vm = new CreateViewModel(this, git, messenger, shell, storage, web);

            DataContext = vm;
        }

        private void OnCancel(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
