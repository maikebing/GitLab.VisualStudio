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

        protected Project Project
        {
            get { return _project; }
        }

        protected string Branch
        {
            get { return _branch; }
        }

        public override void Invalidate()
        {
            IsVisible = IsCodeCloudRepo();
        }

        private bool IsCodeCloudRepo()
        {
            var projects = _vs.Projects;
            var repo = _vs.GetActiveRepository();
            if (repo == null || projects == null)
            {
                return false;
            }

            var path = repo.Path;
            var url = _git.GetRemote(path);

            var visible = false;

            foreach (var project in projects)
            {
                if (string.Equals(project.Url, url, StringComparison.OrdinalIgnoreCase))
                {
                    visible = true;

                    _project = project;
                    _branch = repo.Branch;

                    break;
                }
            }

            return visible;
        }

        protected void OpenInBrowser(string endpoint)
        {
            var user = _storage.GetUser();

            var url = $"https://git.oschina.net/{user.Username}/{_project.Name}/{endpoint}";

            _shell.OpenUrl(url);
        }
    }
}
