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
        private readonly IVisualStudioService _vs;
        private readonly IWebService _web;

        private Project _project;
        private string _branch;

        public CodeCloudNavigationItem(Octicon icon, IGitService git, IShellService shell, IStorage storage, IVisualStudioService vs, IWebService web)
        {
            _git = git;
            _shell = shell;
            _storage = storage;
            _vs = vs;
            _web = web;

            var brush = new SolidColorBrush(Color.FromRgb(66, 66, 66));
            brush.Freeze();
            m_icon = SharedResources.GetDrawingForIcon(icon, brush);
        }

        public override void Invalidate()
        {
            IsVisible = _vs.IsCodeCloudProject;
        }

        protected void OpenInBrowser(string endpoint)
        {
            var user = _storage.GetUser();

            var url = $"https://git.oschina.net/{user.Username}/{_vs.Current.Name}/{endpoint}";

            _shell.OpenUrl(url);
        }
    }
}
