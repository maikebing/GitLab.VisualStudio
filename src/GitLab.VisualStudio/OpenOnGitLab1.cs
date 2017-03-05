//------------------------------------------------------------------------------
// <copyright file="OpenOnGitLab.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using EnvDTE80;
using EnvDTE;
using GitLab.VisualStudio.Services;
using System.Diagnostics;

namespace GitLab.VisualStudio
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenOnGitLab1
    {

     
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenOnGitLab"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OpenOnGitLab(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                foreach (var item in new[]
              {
                   PackageIds. MyMenuGroup,
                     PackageIds.CommandId_OpenMaster,
                    PackageIds. CommandId_OpenBranch,
                      PackageIds.CommandId_OpenRevision,
                    PackageIds.CommandId_OpenRevisionFull
                })
                {
                    var menuCommandID = new CommandID(PackageGuids.guidOpenOnGitLabPackageCmdSet, (int)item);
                    var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                    menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                    commandService.AddCommand(menuItem);
                }
            }
        }
        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (MenuCommand)sender;
            try
            {
                // TODO:is should avoid create GitAnalysis every call?
                using (var git = new GitAnalysis(GetActiveFilePath()))
                {
                    if (!git.IsDiscoveredGitRepository)
                    {
                        command.Enabled = false;
                        return;
                    }

                    var type = ToGitHubUrlType(command.CommandID.ID);
                    var targetPath = git.GetGitHubTargetPath(type);
                    if (type == GitHubUrlType.CurrentBranch && targetPath == "master")
                    {
                        command.Visible = false;
                    }
                    else
                    {
                        // command.Properties. = git.GetGitHubTargetDescription(type);
                        command.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var exstr = ex.ToString();
                Debug.Write(exstr);
                //  command.Text = "error:" + ex.GetType().Name;
                command.Enabled = false;
            }
        }
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenOnGitLab Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }
        private static DTE2 _dte;

        internal DTE2 DTE
        {
            get
            {
                if (_dte == null)
                {
                    _dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;
                }

                return _dte;
            }
        }
        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new OpenOnGitLab(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            //   string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "OpenOnGitLab";

            // Show a message box to prove we were here

            var command = (MenuCommand)sender;
            try
            {
                using (var git = new GitAnalysis(GetActiveFilePath()))
                {
                    if (!git.IsDiscoveredGitRepository)
                    {
                        return;
                    }
                    var selectionLineRange = GetSelectionLineRange();
                    var type = ToGitHubUrlType(command.CommandID.ID);
                    var gitHubUrl = git.BuildGitHubUrl(type, selectionLineRange);
                    System.Diagnostics.Process.Start(gitHubUrl); // open browser
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
        this.ServiceProvider,
        ex.Message,
        title,
        OLEMSGICON.OLEMSGICON_INFO,
        OLEMSGBUTTON.OLEMSGBUTTON_OK,
        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            }

        }


        string GetActiveFilePath()
        {
            // sometimes, DTE.ActiveDocument.Path is ToLower but GitHub can't open lower path.
            // fix proper-casing | http://stackoverflow.com/questions/325931/getting-actual-file-name-with-proper-casing-on-windows-with-net
            var path = GetExactPathName(DTE.ActiveDocument.Path + DTE.ActiveDocument.Name);
            return path;
        }

        static string GetExactPathName(string pathName)
        {
            if (!(File.Exists(pathName) || Directory.Exists(pathName)))
                return pathName;

            var di = new DirectoryInfo(pathName);

            if (di.Parent != null)
            {
                return Path.Combine(
                    GetExactPathName(di.Parent.FullName),
                    di.Parent.GetFileSystemInfos(di.Name)[0].Name);
            }
            else
            {
                return di.Name.ToUpper();
            }
        }

        Tuple<int, int> GetSelectionLineRange()
        {
            var selection = DTE.ActiveDocument.Selection as TextSelection;
            if (selection == null)
            {
                return null;
            }

            return Tuple.Create(selection.TopPoint.Line, selection.BottomPoint.Line);
        }

        static GitHubUrlType ToGitHubUrlType(int commandId)
        {
            if (commandId == PackageIds.CommandId_OpenMaster) return GitHubUrlType.Master;
            if (commandId == PackageIds.CommandId_OpenBranch) return GitHubUrlType.CurrentBranch;
            if (commandId == PackageIds.CommandId_OpenRevision) return GitHubUrlType.CurrentRevision;
            if (commandId == PackageIds.CommandId_OpenRevisionFull)
                return GitHubUrlType.CurrentRevisionFull;
            return GitHubUrlType.CurrentRevisionFull;
        }
    }
}
