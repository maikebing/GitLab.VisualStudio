using CodeCloud.TeamFoundation.ViewModels;
using CodeCloud.VisualStudio.Shared;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CodeCloud.TeamFoundation.Views
{
    /// <summary>
    /// Interaction logic for ConnectSectionView.xaml
    /// </summary>
    public partial class ConnectSectionView : UserControl
    {
        static ConnectSectionView()
        {
            // Fix System.Windows.Interactivity not found issue
            System.Console.WriteLine(typeof(Interaction));
        }

        public ConnectSectionView(IMessenger messenger, IRegistry registry, IShellService shell, IStorage storage, ITeamExplorerServices teamexplorer, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
        {
            InitializeComponent();

            var vm = new ConnectSectionViewModel(messenger, registry, shell, storage, teamexplorer, viewFactory, vs, web);

            DataContext = vm;
        }
    }

}
