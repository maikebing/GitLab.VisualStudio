using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCloud.VisualStudio.Shared
{
    public interface IGitService
    {
        string GetRemote(string path);
        IReadOnlyList<string> GetGitIgnores();
        IReadOnlyList<string> GetLicenses();
        void FillAccessories(string fullname, string email, string path, string gitignore, string license);
        void PushInitialCommit(string fullname, string email, string password, string url, string gitignore, string license);
        void PushWithLicense(string fullname, string email, string password, string url, string path, string license);
    }
}
