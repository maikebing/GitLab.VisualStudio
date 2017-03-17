//------------------------------------------------------------------------------
// <copyright file="OpenOnGitLabPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using EnvDTE80;
using EnvDTE;
using GitLab.VisualStudio.Services;
using System.IO;

namespace GitLab.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#8110", "#8112", "1.0", IconResourceID = 8400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus3.ctmenu", 1)]
    [Guid(OpenOnGitLabPackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class OpenOnGitLabPackage : Package
    {
        /// <summary>
        /// OpenOnGitLabPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "4549d811-2209-4fde-9fe6-ef50ebe00ca0";

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenOnGitLab"/> class.
        /// </summary>
        public OpenOnGitLabPackage()
        {
            ServiceProvider = (this);
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Instance = this;

            try
            {
                var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (mcs != null)
                {
                    AddCommand(mcs, PackageIds.CommandId_OpenMaster, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenMaster);
                    AddCommand(mcs, PackageIds.CommandId_OpenBranch, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenBranch);
                    AddCommand(mcs, PackageIds.CommandId_OpenRevision, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenRevision);
                    AddCommand(mcs, PackageIds.CommandId_OpenRevisionFull, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenRevisionFull);
                    AddCommand(mcs, PackageIds.CommandId_Commits, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenCommits);
                    AddCommand(mcs, PackageIds.CommandId_Blame, VSPackage.OpenOnGitLab_OpenOnGitLab_OpenBlame);
                }
            }
            catch (Exception  )
            {
 
            }
            base.Initialize();
        }

        #endregion

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6cd6c755-f5a3-4944-a799-44b346edd2ea");


        public static System.IServiceProvider ServiceProvider { get; set; }

        private static DTE2 _dte;

        internal static DTE2 DTE
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
        private void AddCommand(OleMenuCommandService commandService, int item, string text)
        {
            var menuCommandID = new CommandID(PackageGuids.guidOpenOnGitLabPackageCmdSet, (int)item);
            var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID, text);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
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

                    var type = ToGitLabUrlType(command.CommandID.ID);
                    var targetPath = git.GetGitLabTargetPath(type);
                    if (type == GitLabUrlType.CurrentBranch && targetPath == "master")
                    {
                        command.Visible = false;
                    }
                    else
                    {
                        command.Text = git.GetGitLabTargetDescription(type);
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
        public static OpenOnGitLabPackage Instance
        {
            get;
            private set;
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
                    var type = ToGitLabUrlType(command.CommandID.ID);
                    var gitLabUrl = git.BuildGitLabUrl(type, selectionLineRange);
                    System.Diagnostics.Process.Start(gitLabUrl); // open browser
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
        ServiceProvider,
        ex.Message,
        title,
        OLEMSGICON.OLEMSGICON_INFO,
        OLEMSGBUTTON.OLEMSGBUTTON_OK,
        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            }
        }

      public static  string GetActiveFilePath()
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

        static GitLabUrlType ToGitLabUrlType(int commandId)
        {
            if (commandId == PackageIds.CommandId_OpenMaster) return GitLabUrlType.Master;
            if (commandId == PackageIds.CommandId_OpenBranch) return GitLabUrlType.CurrentBranch;
            if (commandId == PackageIds.CommandId_OpenRevision) return GitLabUrlType.CurrentRevision;
            if (commandId == PackageIds.CommandId_Blame) return GitLabUrlType.Blame;
            if (commandId == PackageIds.CommandId_Commits) return GitLabUrlType.Commits;
            if (commandId == PackageIds.CommandId_OpenRevisionFull)
                return GitLabUrlType.CurrentRevisionFull;
            return GitLabUrlType.CurrentRevisionFull;
        }
    }
}
