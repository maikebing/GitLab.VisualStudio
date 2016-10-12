using CodeCloud.VisualStudio.Shared;
using CodeCloud.VisualStudio.UI;
using CodeCloud.VisualStudio.UI.Views;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CodeCloud.VisualStudio.Services
{
    [Export(typeof(IViewFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ViewFactory : IViewFactory
    {
        [Import]
        private IGitService _git;

        [Import]
        private IMessenger _messenger;

        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider _provider;

        [Import]
        private IRegistry _registry;

        [Import]
        private IStorage _storage;

        [Import]
        private IShellService _shell;

        [Import]
        private ITeamExplorerServices _teamexplorer;

        [Import]
        private IVisualStudioService _vs;

        [Import]
        private IWebService _web;

        public T GetView<T>(ViewTypes type) where T: Control
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
            return null;
        }
    }
}
