using System.Threading.Tasks;
using System.Windows.Input;

namespace GitLab.VisualStudio.Shared
{
    public class RepositoryInfo
    {
        public string Branch { get; set; }
        public string Path { get; set; }
    }

    public interface INotificationService
    {
        void ShowMessage(string message);
        void ShowMessage(string message, ICommand command);
        void ShowWarning(string message);
        void ShowError(string message);
    }

    public interface ITeamExplorerServices : INotificationService
    {
        void ShowPublishSection();
        void ClearNotifications();
        RepositoryInfo GetActiveRepository();
        string GetSolutionPath();
        Task<bool> IsGitLabRepoAsync();

        Project Project { get; }
    }
}