using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.Shared.Controls;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace CodeCloud.TeamFoundation.Home
{
    [TeamExplorerNavigationItem(Settings.AttachmentsNavigationItemId, Settings.Attachments)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AttachmentsNavigationItem : CodeCloudNavigationItem
    {
        [ImportingConstructor]
        public AttachmentsNavigationItem(IGitService git, IShellService shell, IStorage storage, IVisualStudioService vs, IWebService ws)
           : base(Octicon.attachment, git, shell, storage, vs, ws)
        {
            Text = Strings.Items_Attachments;
        }

        protected override void SetDefaultColors()
        {
            m_defaultArgbColorBrush = new SolidColorBrush(Colors.LightBlueNavigationItem);
        }

        public override void Execute()
        {
            OpenInBrowser("attach_files");
        }
    }
}
