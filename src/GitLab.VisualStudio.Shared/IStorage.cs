using GitLab.VisualStudio.Shared.Models;

namespace GitLab.VisualStudio.Shared
{
    public interface IStorage
    {
        bool IsLogined { get; }

        User GetUser();

        string Host { get; }
        

        string GetPassword(string _host);

        void SaveUser(User user, string password);

        void Erase();

        string GetBaseRepositoryDirectory();

        void LoadHostVersionInfo();

        ApiVersion GetApiVersion(string host);

        bool HaveHost(string host);

        void AddHostVersionInfo(string host, ApiVersion apiVersion);
    }
}