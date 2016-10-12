using System;

namespace CodeCloud.VisualStudio.Shared
{
    public interface IStorage
    {
        bool IsLogined { get; }
        User GetUser();

        string GetPassword();

        void SaveUser(User user, string password);
        void Erase();

        string GetBaseRepositoryDirectory();
    }
}
