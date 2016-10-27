using CodeCloud.VisualStudio.Shared;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCloud.VisualStudio.Services
{
    [Export(typeof(IRegistry))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Registry : IRegistry
    {
        const string TEGitKey = @"Software\Microsoft\VisualStudio\14.0\TeamFoundation\GitSourceControl";
        static RegistryKey OpenGitKey(string path)
        {
            return Microsoft.Win32.Registry.CurrentUser.OpenSubKey(TEGitKey + "\\" + path, true);
        }

        public IReadOnlyList<Repository> GetKnownRepositories()
        {
            using (var key = OpenGitKey("Repositories"))
            {
                if (key == null)
                {
                    return null;
                }

                return key.GetSubKeyNames().Select(x =>
                {
                    using (var subkey = key.OpenSubKey(x))
                    {
                        try
                        {
                            var name = subkey?.GetValue("Name") as string;
                            var path = subkey?.GetValue("Path") as string;
                            if (path != null && Directory.Exists(path))
                                return new Repository
                                {
                                    Name = name,
                                    Path = path
                                };
                        }
                        catch (Exception)
                        {
                            // no sense spamming the log, the registry might have ton of stale things we don't care about
                        }
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToList();
            }
        }
    }
}
