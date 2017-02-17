using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace GitLab.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid("bfbbc56d-5137-4398-9c13-78fb716ddcb8")]
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
