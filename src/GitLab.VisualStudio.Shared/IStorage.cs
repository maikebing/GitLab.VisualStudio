using System;

namespace GitLab.VisualStudio.Shared
{
    public interface IStorage
    {
        bool IsLogined { get; }
        User GetUser();
        string Host { get;  }
        string GetPassword();

        void SaveUser( User user, string password);
        void Erase();

        string GetBaseRepositoryDirectory();
    }
}
