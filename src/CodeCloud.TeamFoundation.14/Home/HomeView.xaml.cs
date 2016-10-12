using CodeCloud.VisualStudio.Shared;
using System.Windows;
using System.Windows.Controls;

namespace CodeCloud.TeamFoundation.Home
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private readonly ITeamExplorerServices _service;
        public HomeView(ITeamExplorerServices service)
        {
            _service = service;
            InitializeComponent();
        }

        private void ShowMessage(object sender, RoutedEventArgs e)
        {
            _service.ShowMessage("This is a message");
        }

        private void ShowWarning(object sender, RoutedEventArgs e)
        {
            _service.ShowWarning("This is a warning");
        }

        private void ShowError(object sender, RoutedEventArgs e)
        {
            _service.ShowError("This is a error");
        }

        private void ShowPublish(object sender, RoutedEventArgs e)
        {
            _service.ShowPublishSection();
        }
    }
}
