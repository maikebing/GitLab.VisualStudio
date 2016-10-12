using CodeCloud.TeamFoundation.Base;
using CodeCloud.TeamFoundation.Views;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CodeCloud.TeamFoundation.Connect
{
    [TeamExplorerSection(ConnectSectionId, TeamExplorerPageIds.Connect, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CodeCloudConnectSection : TeamExplorerSection
    {
        public const string ConnectSectionId = "519B47D3-F2A9-4E19-8491-8C9FA25ABE90";

        private readonly IVisualStudioService _vs;

        [ImportingConstructor]
        public CodeCloudConnectSection(IMessenger messenger, IRegistry registry, IShellService shell, IStorage storage, ITeamExplorerServices teamexplorer, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
        {
            messenger.Register("OnLogined", OnLogined);
            messenger.Register("OnSignOuted", OnSignOuted);
            messenger.Register<string, string>("OnClone", OnClone);
            messenger.Register<string>("OnOpenSolution", OnOpenSolution);
            Title = "Code Cloud";
            IsExpanded = true;
            IsVisible = storage.IsLogined;

            _vs = vs;
            SectionContent = new ConnectSectionView(messenger, registry, shell, storage, teamexplorer, viewFactory, vs, web);
        }

        public void OnLogined()
        {
            IsVisible = true;
        }

        public void OnSignOuted()
        {
            IsVisible = false;
        }

        private IServiceProvider _serviceProvider;
        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            _serviceProvider = e.ServiceProvider;
            _vs.ServiceProvider = _serviceProvider;
        }

        public void OnClone(string url, string path)
        {
            var gitExt = _serviceProvider.GetService<IGitRepositoriesExt>();

            gitExt.Clone(url, path, CloneOptions.RecurseSubmodule);
        }

        public void OnOpenSolution(string path)
        {
            var x = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (x != null)
            {
                x.OpenSolutionViaDlg(path, 1);
            }
        }

        public override void Refresh()
        {
            base.Refresh();
        }
    }
}
