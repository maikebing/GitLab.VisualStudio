using CodeCloud.TeamFoundation.ViewModels;
using CodeCloud.VisualStudio.Shared;
using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CodeCloud.TeamFoundation.Views
{
    /// <summary>
    /// Interaction logic for PublishSectionView.xaml
    /// </summary>
    public partial class PublishSectionView : UserControl, IDisposable
    {
        static PublishSectionView()
        {
            // Fix System.Windows.Interactivity not found issue
            System.Console.WriteLine(typeof(Interaction));
        }

        public PublishSectionView(IMessenger messenger, IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
        {
            InitializeComponent();

            var vm = new PublishSectionViewModel(messenger, git, shell, storage, tes, viewFactory, vs, web);

            DataContext = vm;
        }

        public void Dispose()
        {
            var disposable = DataContext as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
