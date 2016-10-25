using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System.ComponentModel.Composition;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerSection(HomeSectionId, TeamExplorerPageIds.Home, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CodeCloudHomeSection : TeamExplorerSectionBase
    {
        public const string HomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";

        private readonly ITeamExplorerServices _service;

        private readonly IMessenger _messenger;
        private readonly IRegistry _registry;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _teamexplorer;
        private readonly IViewFactory _viewFactory;
        private readonly IVisualStudioService _vs;
        private readonly IWebService _web;

        [ImportingConstructor]
        public CodeCloudHomeSection(ITeamExplorerServices service, IMessenger messenger, IRegistry registry, IShellService shell, IStorage storage, ITeamExplorerServices teamexplorer, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
        {
            _service = service;

            _messenger = messenger;
            _registry = registry;
            _shell = shell;
            _storage = storage;
            _teamexplorer = teamexplorer;
            _viewFactory = viewFactory;
            _vs = vs;
            _web = web;
        }

        protected override ITeamExplorerSection CreateViewModel(SectionInitializeEventArgs e)
        {
            var temp = new TeamExplorerSectionViewModelBase();
            temp.Title = "Code Cloud";

            IsVisible = true;

            return temp;
        }

        protected override object CreateView(SectionInitializeEventArgs e)
        {
            return new HomeView(_service);
        }
    }
}