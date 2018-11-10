using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Controls;
using GitLab.VisualStudio.Shared.Models;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GitLab.TeamFoundation.Home
{
    public abstract class GitLabNavigationItem : TeamExplorerNavigationItemBase, ITeamExplorerNavigationItem2
    {
        private readonly IGitService _git;
        private readonly IShellService _shell;
        private readonly IStorage _storage;
        private readonly ITeamExplorerServices _tes;
        private readonly IWebService _web;

        private Project _project;
        private string _branch;
        private Octicon octicon;

        public GitLabNavigationItem(Octicon icon, IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IWebService web)
        {
            _git = git;
            _shell = shell;
            _storage = storage;
            _tes = tes;
            _web = web;
            octicon = icon;
            var brush = new SolidColorBrush(Color.FromRgb(66, 66, 66));

            brush.Freeze();
            OnThemeChanged();
            VSColorTheme.ThemeChanged += _ =>
            {
                OnThemeChanged();
                Invalidate();
            };
        }
        static DateTime dateTime = DateTime.MinValue;
        static bool? lastIsVisible = null;
        public override void Invalidate()
        {
            if (DateTime.Now.Subtract(dateTime).TotalSeconds > 5 || !lastIsVisible.HasValue)
            {
                IsVisible = false;
                ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
                   {
                       IsVisible = await _tes.IsGitLabRepoAsync() && _tes.Project != null;
                       lastIsVisible = IsVisible;
                       dateTime = DateTime.Now;
                   });
            }
            else
            {
                IsVisible = lastIsVisible.GetValueOrDefault();
            }
        }

        private void OnThemeChanged()
        {
            var theme = Colors.DetectTheme();
            var dark = theme == "Dark";
            m_defaultArgbColorBrush = new SolidColorBrush(dark ? Colors.DarkThemeNavigationItem : Colors.LightBlueNavigationItem);
            m_icon = SharedResources.GetDrawingForIcon(octicon, dark ? Colors.DarkThemeNavigationItem : Colors.LightThemeNavigationItem, theme);
        }

        protected void OpenInBrowser(string endpoint)
        {
            var url = $"{_tes.Project.WebUrl}/{endpoint}";
            _shell.OpenUrl(url);
        }

        protected void OpenHostUrlInBrowser(string endpoint)
        {
            var url = $"{_storage.Host}/{endpoint}";
            _shell.OpenUrl(url);
        }
    }
}