using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLab.VisualStudio
{
    static class PackageGuids
    {
        public const string guidGitLabPkgString = "54803a44-49e0-4935-bba4-7d7d91682273";
        public const string guidIssuesToolWindowCmdSetString = "78e9c435-500f-4e4d-8e31-dc16d9f26032";
        public static readonly Guid IssuesToolWindowCmdSet = new Guid(guidIssuesToolWindowCmdSetString);
        public const string guidOpenOnGitLabCmdSetString = "72B54F2E-FE93-4950-88BF-C536D1CFD91F";
        public const string Version = "1.0";
        public static readonly Guid guidGitLabPackagePkg = new Guid(guidGitLabPkgString);
        public static readonly Guid guidOpenOnGitLabCmdSet = new Guid(guidOpenOnGitLabCmdSetString);
    };

    static class PackageCommanddIDs
    {
        public const uint OpenMaster = 0x100;
        public const uint OpenBranch = 0x200;
        public const uint OpenRevision = 0x300;
        public const uint OpenRevisionFull = 0x400;
        public const uint OpenBlame = 0x500;
        public const uint OpenCommits = 0x600;
        public const uint CreateSnippet = 0x700;
        public const uint IssuesToolWindows = 0x800;
    };

 
}
