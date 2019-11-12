using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Models;
using GitLab.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;
using Microsoft.VisualStudio.Threading;

using CodeContainer = Microsoft.VisualStudio.Shell.CodeContainerManagement.CodeContainer;
using ICodeContainerProvider = Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerProvider;
using Task = System.Threading.Tasks.Task;
namespace GitLab.StartPage
{
    [Guid(PackageGuids.CodeContainerProviderId)]
    public class GitLabContainerProvider : ICodeContainerProvider
    {

        readonly Lazy<IGitLabServiceProvider> _gitLabServiceProvider;

        public GitLabContainerProvider() : this(
            new Lazy<IGitLabServiceProvider>(() => Package.GetGlobalService(typeof(IGitLabServiceProvider)) as IGitLabServiceProvider))
        {
        }

        public GitLabContainerProvider(Lazy<IGitLabServiceProvider> gitLabServiceProvider)
        {
            this._gitLabServiceProvider = gitLabServiceProvider;
        }

        public async Task<CodeContainer> AcquireCodeContainerAsync(IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {
            return await RunAcquisition(downloadProgress, null, cancellationToken);
        }

        public async Task<CodeContainer> AcquireCodeContainerAsync(RemoteCodeContainer onlineCodeContainer, IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {

            return await RunAcquisition(downloadProgress, onlineCodeContainer, cancellationToken);
        }

        async Task<CodeContainer> RunAcquisition(IProgress<ServiceProgressData> downloadProgress, RemoteCodeContainer repository, CancellationToken cancellationToken)
        {
            CloneDialogResult request = null;
            try
            {
                var glsp =await Task.Run(()=>_gitLabServiceProvider.Value);
                var _viewFactory =glsp.GetService<IViewFactory>();
                request = _viewFactory?.ShowCloneDialog();
                if (request != null)
                {
                    var gitExt = glsp.GetService<IGitRepositoriesExt>();
                    gitExt.Clone(request.Url, request.Path, CloneOptions.RecurseSubmodule);
                }
            }
            catch (Exception e)
            {
                //log.Error(e, "Error showing Start Page clone dialog");
            }

            if (request == null)
                return null;

            var uri = request.Url.ToRepositoryUrl();
            var repositoryName = request.Url.RepositoryName;

            // Report all steps complete before returning a CodeContainer
            downloadProgress.Report(new ServiceProgressData(string.Empty, string.Empty, 1, 1));

            return new CodeContainer(
                localProperties: new CodeContainerLocalProperties(request.Path, CodeContainerType.Folder,
                                new CodeContainerSourceControlProperties(repositoryName, request.Path, new Guid(PackageGuids.GitSccProviderId))),
                remote: new RemoteCodeContainer(repositoryName,
                                                new Guid(PackageGuids.CodeContainerProviderId),
                                                uri,
                                                new Uri(uri.ToString().TrimSuffix(".git")),
                                                DateTimeOffset.UtcNow),
                isFavorite: false,
                lastAccessed: DateTimeOffset.UtcNow);
        }

    }
}
