using GitLab.TeamFoundation.Home;
using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Helpers;
using GitLab.VisualStudio.Shared.Helpers.Commands;
using GitLab.VisualStudio.Shared.Models;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace GitLab.TeamFoundation.ViewModels
{
    public class GitLabWorkItemsViewModel : Bindable
    {
        private readonly IStorage _storage;
        private readonly IWebService _web;
        private readonly IShellService _shell;
        private readonly ITeamExplorerServices _tes;
        private readonly GitLabWorkItemsPageMode _mode;

        private string _projectPath;
        private string _state;
        private string _scope;
        private string _authorUsername;
        private string _assigneeUsername;
        private string _labels;
        private string _search;
        private string _targetBranch;
        private string _statusMessage;
        private bool _isBusy;
        private GitLabWorkItem _selectedItem;

        public GitLabWorkItemsViewModel(IStorage storage, IWebService web, IShellService shell, ITeamExplorerServices tes, GitLabWorkItemsPageMode mode)
        {
            _storage = storage;
            _web = web;
            _shell = shell;
            _tes = tes;
            _mode = mode;

            Items = new ObservableCollection<GitLabWorkItem>();
            RefreshCommand = new DelegateCommand(Refresh);
            OpenCommand = new DelegateCommand(OpenSelected);

            var settings = _storage.AppSettings ?? new AppSettings();
            DefaultBranch = NormalizeDefaultBranch(settings.DefaultBranch);
            State = "opened";
            Scope = "all";
            TargetBranch = mode == GitLabWorkItemsPageMode.MergeRequests ? DefaultBranch : string.Empty;
            ProjectPath = GetConfiguredProjectPath(settings);

            Refresh();
        }

        public ObservableCollection<GitLabWorkItem> Items { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand OpenCommand { get; private set; }

        public string Title
        {
            get { return IsMergeRequests ? Strings.Items_MergeRequests : Strings.Items_Issues; }
        }

        public bool IsMergeRequests
        {
            get { return _mode == GitLabWorkItemsPageMode.MergeRequests; }
        }

        public string DefaultBranch { get; private set; }

        public string ProjectPath
        {
            get { return _projectPath; }
            set { SetProperty(ref _projectPath, value); }
        }

        public string State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public string Scope
        {
            get { return _scope; }
            set { SetProperty(ref _scope, value); }
        }

        public string AuthorUsername
        {
            get { return _authorUsername; }
            set { SetProperty(ref _authorUsername, value); }
        }

        public string AssigneeUsername
        {
            get { return _assigneeUsername; }
            set { SetProperty(ref _assigneeUsername, value); }
        }

        public string Labels
        {
            get { return _labels; }
            set { SetProperty(ref _labels, value); }
        }

        public string Search
        {
            get { return _search; }
            set { SetProperty(ref _search, value); }
        }

        public string TargetBranch
        {
            get { return _targetBranch; }
            set { SetProperty(ref _targetBranch, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public GitLabWorkItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public void Refresh()
        {
            if (IsBusy)
            {
                return;
            }

            var query = BuildQuery();
            var mode = _mode;
            IsBusy = true;
            StatusMessage = "Loading...";

            Task.Run(() =>
            {
                return mode == GitLabWorkItemsPageMode.MergeRequests
                    ? _web.GetMergeRequests(query)
                    : _web.GetIssues(query);
            }).ContinueWith(async t =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IsBusy = false;
                Items.Clear();

                if (t.IsFaulted)
                {
                    var ex = t.Exception != null ? t.Exception.GetBaseException() : null;
                    StatusMessage = ex != null ? ex.Message : "Failed to load GitLab items.";
                    _tes.ShowError(StatusMessage);
                    return;
                }

                foreach (var item in t.Result.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt ?? DateTime.MinValue))
                {
                    Items.Add(item);
                }

                StatusMessage = Items.Count + " item(s)";
            }, TaskScheduler.Default).Forget();
        }

        private GitLabWorkItemQuery BuildQuery()
        {
            return new GitLabWorkItemQuery
            {
                ProjectPath = ProjectPath,
                State = State,
                Scope = Scope,
                AuthorUsername = AuthorUsername,
                AssigneeUsername = AssigneeUsername,
                Labels = Labels,
                Search = Search,
                TargetBranch = IsMergeRequests ? TargetBranch : null
            };
        }

        private void OpenSelected()
        {
            if (SelectedItem != null && !string.IsNullOrWhiteSpace(SelectedItem.WebUrl))
            {
                _shell.OpenUrl(SelectedItem.WebUrl);
            }
        }

        private string GetConfiguredProjectPath(AppSettings settings)
        {
            var configuredPath = IsMergeRequests ? settings.MergeRequestsProjectPath : settings.IssuesProjectPath;
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return configuredPath;
            }

            var project = _tes.Project;
            if (project != null)
            {
                if (!string.IsNullOrWhiteSpace(project.PathWithNamespace))
                {
                    return project.PathWithNamespace;
                }

                if (!string.IsNullOrWhiteSpace(project.Namespace) && !string.IsNullOrWhiteSpace(project.Path))
                {
                    return project.Namespace + "/" + project.Path;
                }
            }

            return string.Empty;
        }

        private static string NormalizeDefaultBranch(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "develop" : value.Trim();
        }
    }
}
