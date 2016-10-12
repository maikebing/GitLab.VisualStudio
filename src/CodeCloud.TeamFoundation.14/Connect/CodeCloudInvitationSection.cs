using CodeCloud.TeamFoundation.Base;
using CodeCloud.VisualStudio.Shared;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CodeCloud.TeamFoundation.Connect
{
    [TeamExplorerServiceInvitation(InvitationSectionId, InvitationSectionPriority)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CodeCloudInvitationSection : TeamExplorerInvitation
    {
        public const string InvitationSectionId = "C2443FCC-6D62-4D31-B08A-C4DE70109C7F";
        public const int InvitationSectionPriority = 100;
        readonly IShellService _shell;
        readonly IViewFactory _viewFactory;
        readonly IMessenger _messenger;

        [ImportingConstructor]
        public CodeCloudInvitationSection(IViewFactory viewFactory, IStorage storage, IMessenger messenger, IShellService shell)
        {
            _viewFactory = viewFactory;
            _messenger = messenger;
            _shell = shell;

            _messenger.Register("OnLogined", OnLogined);
            _messenger.Register("OnSignOuted", OnSignOuted);

            CanConnect = true;
            CanSignUp = true;
            ConnectLabel = "Connect";
            SignUpLabel = "Sign up";
            Name = "CodeCloud";
            Provider = "CodeCloud, Inc.";
            Description = "Description xxx";

            var assembly = Assembly.GetExecutingAssembly().GetName().Name;
            var image = new BitmapImage(new Uri($"pack://application:,,,/{assembly};component/Resources/logo.png", UriKind.Absolute));;

            Icon = new ImageBrush(image);

            IsVisible = !storage.IsLogined;
        }

        public override void Connect()
        {
            var dialog = _viewFactory.GetView<Dialog>(ViewTypes.Login);
            _shell.ShowDialog("连接到码云", dialog);
        }

        public override void SignUp()
        {
            _shell.OpenUrl(@"https://git.oschina.net/signup");
        }

        public void OnLogined()
        {
            IsVisible = false;
        }

        public void OnSignOuted()
        {
            IsVisible = true;
        }
    }
}
