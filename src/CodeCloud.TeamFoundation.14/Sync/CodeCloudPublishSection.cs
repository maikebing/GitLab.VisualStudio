using CodeCloud.TeamFoundation.Base;
using CodeCloud.TeamFoundation.Views;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using System;
using System.ComponentModel.Composition;

namespace CodeCloud.TeamFoundation.Sync
{


    [TeamExplorerSection(PublishSectionId, TeamExplorerPageIds.GitCommits, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CodeCloudPublishSection : TeamExplorerSection, INotifyPropertySource, IDisposable
    {
        public const string PublishSectionId = "92655B52-360D-4BF5-95C5-D9E9E596AC76";

        private readonly IVisualStudioService _vs;

        [ImportingConstructor]
        public CodeCloudPublishSection(IMessenger messenger, IGitService git, IShellService shell, IStorage storage, ITeamExplorerServices tes, IViewFactory viewFactory, IVisualStudioService vs, IWebService web)
        {
            _vs = vs;

            Title = "发布到码云";
            IsVisible = true;
            IsExpanded = true;

            SectionContent = new PublishSectionView(messenger, git, shell, storage, tes, viewFactory, vs, web);
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            //_vs.ServiceProvider = e.ServiceProvider;
        }

        public void ShowPublish()
        {
            IsVisible = true;
        }

        public override void Dispose()
        {
            var disposable = SectionContent as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}