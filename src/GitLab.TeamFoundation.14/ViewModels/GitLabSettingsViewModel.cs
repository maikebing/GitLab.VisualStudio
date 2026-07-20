using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Helpers;
using GitLab.VisualStudio.Shared.Helpers.Commands;
using GitLab.VisualStudio.Shared.Models;
using System;
using System.Windows.Input;

namespace GitLab.TeamFoundation.ViewModels
{
    public class GitLabSettingsViewModel : Bindable
    {
        private readonly IStorage _storage;
        private string _defaultBranch;
        private string _issuesProjectPath;
        private string _mergeRequestsProjectPath;
        private string _statusMessage;

        public GitLabSettingsViewModel(IStorage storage)
        {
            _storage = storage;
            SaveCommand = new DelegateCommand(SaveSettings);

            var settings = _storage.AppSettings ?? new AppSettings();
            DefaultBranch = NormalizeDefaultBranch(settings.DefaultBranch);
            IssuesProjectPath = settings.IssuesProjectPath;
            MergeRequestsProjectPath = settings.MergeRequestsProjectPath;
        }

        public ICommand SaveCommand { get; private set; }

        public string DefaultBranch
        {
            get { return _defaultBranch; }
            set { SetProperty(ref _defaultBranch, value); }
        }

        public string IssuesProjectPath
        {
            get { return _issuesProjectPath; }
            set { SetProperty(ref _issuesProjectPath, value); }
        }

        public string MergeRequestsProjectPath
        {
            get { return _mergeRequestsProjectPath; }
            set { SetProperty(ref _mergeRequestsProjectPath, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = _storage.AppSettings ?? new AppSettings();
                settings.DefaultBranch = NormalizeDefaultBranch(DefaultBranch);
                settings.IssuesProjectPath = NormalizePath(IssuesProjectPath);
                settings.MergeRequestsProjectPath = NormalizePath(MergeRequestsProjectPath);
                _storage.AppSettings = settings;
                _storage.SaveConfig();
                StatusMessage = "Saved.";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private static string NormalizeDefaultBranch(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "develop" : value.Trim();
        }

        private static string NormalizePath(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
