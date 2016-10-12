using CodeCloud.TeamFoundation.Base;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerSection(HomeSectionId, TeamExplorerPageIds.Home, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CodeCloudHomeSection : TeamExplorerSection
    {
        public const string HomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";

        [ImportingConstructor]
        public CodeCloudHomeSection(ITeamExplorerServices service)
        {
            Title = "CodeCloud";
            IsVisible = true;

            SectionContent = new HomeView(service);
        }
    }
}