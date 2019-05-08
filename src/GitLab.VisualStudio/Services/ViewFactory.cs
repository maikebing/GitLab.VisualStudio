using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Helpers.Commands;
using GitLab.VisualStudio.Shared.Models;
using GitLab.VisualStudio.UI;
using GitLab.VisualStudio.UI.ViewModels;
using GitLab.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
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



        public CloneDialogResult ShowCloneDialog(IProgress<ServiceProgressData> downloadProgress)
        {
            CloneDialogResult result = null;
            var dlg = this.GetView<Dialog>(ViewTypes.Clone);
            var dc = (CloneViewModel)dlg.DataContext;

            dc.CloneCommand = new DelegateCommand(() =>
               {
                   try
                   {

                       ITeamExplorerServices _tes = ServiceProvider.GlobalProvider.GetService<ITeamExplorerServices>();
                       string pathx = System.IO.Path.Combine(dc.BaseRepositoryPath, dc.SelectedRepository.Name);
                       Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                       _tes.OnClone(dc.SelectedRepository.Url, pathx);
                       dc.Save();
                       dlg.Close();
                   }
                   catch (Exception)
                   {

                   }
               });

            dc.Progress = downloadProgress;
            _shell.ShowDialog(Strings.Common_Clone, dlg);
            string path = System.IO.Path.Combine(dc.BaseRepositoryPath, dc.SelectedRepository.Name);
            if (dc.SelectedRepository!=null && !string.IsNullOrEmpty(path))
            {
                result= new CloneDialogResult(path, new Shared.Helpers.UriString(dc.SelectedRepository.Url)); 
            }
            return result;
        }


    }
}