using System;

namespace GitLab.VisualStudio.Shared
{
    public interface IStorage
    {
        bool IsLogined { get; }
        User GetUser();
        string Host { get;  }
        string Path { get; }
        string GetPassword(string _host);

        void SaveUser( User user, string password);
        void Erase();

        string GetBaseRepositoryDirectory();
    }
}
