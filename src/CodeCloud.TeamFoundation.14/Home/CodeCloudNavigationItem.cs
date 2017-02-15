using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using System;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    public abstract class CodeCloudNavigationItem : TeamExplorerNavigationItemBase
    {
        private readonly IGitService _git;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _tes;
        private readonly IWebService _web;

        private Project _project;
        private string _branch;

        public CodeCloudNavigationItem(Octicon icon, IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService web)
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

        public override void Invalidate()
        {
            IsVisible = _tes.IsCodeCloudRepo() && _tes.Project != null;
        }

        protected void OpenInBrowser(string endpoint)
        {
            var user = _storage.GetUser();

            var url = $"https://gitclub.cn/{user.Username}/{_tes.Project.Name}/{endpoint}";

            _shell.OpenUrl(url);
        }
    }
}
