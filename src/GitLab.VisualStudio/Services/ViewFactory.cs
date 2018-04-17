using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.UI;
using GitLab.VisualStudio.UI.Views;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace GitLab.VisualStudio.Services
{
    [Export(typeof(IViewFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ViewFactory : IViewFactory
    {
        [Import]
        private IGitService _git;

        [Import]
        private IMessenger _messenger;

        [Import]
        private IStorage _storage;

        [Import]
        private IShellService _shell;

        [Import]
        private IWebService _web;

        public T GetView<T>(ViewTypes type) where T : Control
        {
            if (type == ViewTypes.Login)
            {
                return new LoginView(_messenger, _shell, _storage, _web) as T;
            }

            if (type == ViewTypes.Clone)
            {
                return new CloneView(_messenger, _shell, _storage, _web) as T;
            }

            if (type == ViewTypes.Create)
            {
                return new CreateView(_git, _messenger, _shell, _storage, _web) as T;
            }
            if (type == ViewTypes.CreateSnippet)
            {
                return new CreateSnippet(_messenger, _shell, _storage, _web) as T;
            }
            return null;
        }
    }
}