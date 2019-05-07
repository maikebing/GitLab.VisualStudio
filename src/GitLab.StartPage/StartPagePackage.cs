using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Models;
using GitLab.VisualStudio.UI.Views;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;
using Microsoft.VisualStudio.Threading;
 
using CodeContainer = Microsoft.VisualStudio.Shell.CodeContainerManagement.CodeContainer;
using ICodeContainerProvider = Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerProvider;
using Task = System.Threading.Tasks.Task;

namespace GitLab.StartPage
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(PackageGuids.StartPagePackageId)]
    [ProvideCodeContainerProvider("GitLab Container", PackageGuids.StartPagePackageId,PackageGuids.ImageMonikerGuid, 1, "#110", "#111", typeof(GitLabContainerProvider))]
    public sealed class StartPagePackage : ExtensionPointPackage
    {
       internal static IServiceProvider serviceProvider;

        public StartPagePackage()
        {
            serviceProvider = this;
         
        }
    }
  
}
