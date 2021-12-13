using GitLab.TeamFoundation.Sync;
using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Models;
using Microsoft;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace GitLab.TeamFoundation
{
    [Export(typeof(ITeamExplorerServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServices : ITeamExplorerServices
    {
        private readonly IServiceProvider serviceProvider;
#pragma warning disable 0649

        [Import]
        private IGitService _git;

        [Import]
        private IWebService _web;

        [Import]
        private IStorage _storage;
#pragma warning restore 0649
        /// <summary>
        /// This MEF export requires specific versions of TeamFoundation. ITeamExplorerNotificationManager is declared here so
        /// that instances of this type cannot be created if the TeamFoundation dlls are not available
        /// (otherwise we'll have multiple instances of ITeamExplorerServices exports, and that would be Bad(tm))
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private ITeamExplorerNotificationManager manager;

        [ImportingConstructor]
        public TeamExplorerServices([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void ShowPublishSection()
        {
            var te = serviceProvider.TryGetService<ITeamExplorer>();
            var foo = te.NavigateToPage(new Guid(TeamExplorerPageIds.GitCommits), null);
            var publish = foo?.GetSection(new Guid(Settings.PublishSectionId)) as GitLabPublishSection;
            publish?.ShowPublish();
        }

        public void ShowMessage(string message)
        {
            manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ShowNotification(message, NotificationType.Information, NotificationFlags.None, null, default(Guid));
        }

        public void ShowMessage(string message, ICommand command)
        {
            manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ShowNotification(message, NotificationType.Information, NotificationFlags.None, command, default(Guid));
        }

        public void ShowWarning(string message)
        {
            manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ShowNotification(message, NotificationType.Warning, NotificationFlags.None, null, default(Guid));
        }

        public void ShowError(string message)
        {
            manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ShowNotification(message, NotificationType.Error, NotificationFlags.None, null, default(Guid));
        }

        public void ClearNotifications()
        {
            manager = serviceProvider.TryGetService<ITeamExplorer>() as ITeamExplorerNotificationManager;
            manager?.ClearNotifications();
        }

        public RepositoryInfo GetActiveRepository()
        {
            if (serviceProvider == null)
            {
                return null;
            }

            var git = serviceProvider.GetService<IGitExt>();
            if (git == null)
            {
                throw new Exception("git is null");
            }

            if (git.ActiveRepositories == null)
            {
                throw new Exception("git.activeRepositories is null");
            }

            var repo = git.ActiveRepositories.FirstOrDefault();

            if (repo != null && repo.CurrentBranch != null && !string.IsNullOrEmpty(repo.CurrentBranch.Name))
            {
                return new RepositoryInfo
                {
                    Path = repo.RepositoryPath,
                    Branch = repo.CurrentBranch.Name
                };
            }
            return null;
        }

        public string GetSolutionPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (serviceProvider == null)
            {
                return null;
            }
            string solutionDir=null, solutionFile, userFile;
            var solution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
            if (solution == null)
            {
             
                if (!ErrorHandler.Succeeded(solution.GetSolutionInfo(out solutionDir, out solutionFile, out userFile)))
                {
                    return null;
                }
            }
            return solutionDir;
        }

        public Project Project { get; private set; }
        bool _loading = false;
        public bool IsGitLabRepo()
        {
            var repo = GetActiveRepository();
            if (repo == null)
            {
                return false;
            }

            var path = repo.Path;
            var url = _git.GetRemote(path);

            if (url == null)
            {
                return false;
            }
            if (!_loading)
            {
                Task.Run(() =>
                {
                    _loading = true;
                    if (Project == null || !string.Equals(Project.Url, url, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var projects = _web.GetProjects();
                            foreach (var project in projects)
                            {
                                if (string.Equals(project.Url, url, StringComparison.OrdinalIgnoreCase))
                                {
                                    Project = project;
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        // Ignore
                    }
                    }
                    _loading = false;
                }).Forget();
            }
            return _storage.HaveHost(url);
        }
    }
}