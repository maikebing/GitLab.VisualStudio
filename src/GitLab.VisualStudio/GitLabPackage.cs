using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace GitLab.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid("54803a44-49e0-4935-bba4-7d7d91682273")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // this is the Git service GUID, so we load whenever it loads
    [ProvideAutoLoad("11B8E6D7-C08B-4385-B321-321078CDD1F8")]
    public class GitLabPackage : Package
    {
        public GitLabPackage()
        {

        }
    
    }
}
