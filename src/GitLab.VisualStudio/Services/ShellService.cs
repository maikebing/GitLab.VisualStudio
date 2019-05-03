using GitLab.VisualStudio.Shared;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Forms;

namespace GitLab.VisualStudio.Services
{
    [Export(typeof(IShellService))]
    public class ShellService : IShellService
    {
        [ImportingConstructor]
        public ShellService()
        {
        }

        public string BrowseFolder(string title = null, string selectedPath = null)
        {
            using (var folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.RootFolder = System.Environment.SpecialFolder.Desktop;
                folderBrowser.SelectedPath = selectedPath;
                folderBrowser.ShowNewFolderButton = true;

                if (title != null)
                {
                    folderBrowser.Description = title;
                }

                var dialogResult = folderBrowser.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    return folderBrowser.SelectedPath;
                }
            }

            return null;
        }

        public void OpenUrl(string uri)
        {
            System.Diagnostics.Process.Start(uri);
        }

        public void ShowDialog(string title, Dialog content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            //var uri = new Uri(@"pack://application:,,,/GitLab.UI;component/Resources/Images/logo.png");
            //var icon = new BitmapImage(uri);
            var win = new DialogWindow
            {
                Content = content,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = title,
            };

            content.Closed += () =>
            {
                win.Close();
            };

            win.ShowModal();
        }
    }
}