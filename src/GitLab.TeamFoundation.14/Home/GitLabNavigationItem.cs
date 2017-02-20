using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.Windows.Media;

namespace GitLab.TeamFoundation.Home
{
    public abstract class GitLabNavigationItem : TeamExplorerNavigationItemBase
    {
        private readonly IGitService _git;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _tes;
        private readonly IWebService _web;

        private Project _project;
        private string _branch;

        public GitLabNavigationItem(Octicon icon, IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService web)
        {
            _git = git;
            _shell = shell;
            _storage = storage;
            _tes = tes;
            _web = web;

            var brush = new SolidColorBrush(Color.FromRgb(66, 66, 66));
            brush.Freeze();
            m_icon = SharedResources.GetDrawingForIcon(icon, brush);
        }

        public override async void Invalidate()
        {
            IsVisible = await System.Threading.Tasks.Task.Factory.StartNew(() => _tes.IsGitLabRepo()) && _tes.Project != null;
        }

        protected void OpenInBrowser(string endpoint)
        {
            var user = _storage.GetUser();

            var url = $"{_storage.Host}/{user.Username}/{_tes.Project.Name}/{endpoint}";

            _shell.OpenUrl(url);
        }
    }
}
