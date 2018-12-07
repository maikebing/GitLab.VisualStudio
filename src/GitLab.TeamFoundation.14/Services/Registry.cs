using GitLab.VisualStudio.Shared;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitLab.TeamFoundation.Services
{
    public partial class Registry
    {
        static string tfver = null;
        private static RegistryKey OpenGitKey(string path)
        {
            if (tfver == null)
            {
                string asmname = typeof(Registry).Assembly.GetName().Name;
                tfver = asmname.Substring(asmname.LastIndexOf('.') + 1);
            }
            return Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\VisualStudio\{tfver}.0\TeamFoundation\GitSourceControl\{path}", true);
        }

        public static IReadOnlyList<Repository> GetKnownRepositories()
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