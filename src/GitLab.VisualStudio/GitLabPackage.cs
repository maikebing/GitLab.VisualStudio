using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GitLab.VisualStudio.Shared;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using GitLab.VisualStudio.Services;
using GitLab.VisualStudio.UI.Views;
using GitLab.VisualStudio.UI.ViewModels;
using Microsoft.VisualStudio;
using System.Windows;
using GitLab.VisualStudio.Helpers;
using Microsoft.VisualStudio.ComponentModelHost;

namespace GitLab.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideBindingPath]
    [InstalledProductRegistration("#110", "#112", PackageVersion.Version, IconResourceID = 8400)]
    [Guid(PackageGuids.guidGitLabPkgString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(IssuesToolWindow), MultiInstances = false, Height = 100, Width = 500, Style = Microsoft.VisualStudio.Shell.VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom, Window = EnvDTE.Constants.vsWindowKindMainWindow)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string)]
    public class GitLabPackage : Package, IVsInstalledProduct
    {
        [Import]
        private IShellService _shell;

        [Import]
        private IViewFactory _viewFactory;

        [Import]
        private IWebService _webService;

        [Import]
        private IStorage _storage;

        public GitLabPackage()
        {

            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            }
        }
        #region IVsInstalledProduct Members

        public int IdBmpSplash(out uint pIdBmp)
        {
            pIdBmp = 8400;
            return VSConstants.S_OK;
        }

        public int IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = 8400;
            return VSConstants.S_OK;
        }

        public int OfficialName(out string pbstrName)
        {
            pbstrName = GetResourceString("@101");
            return VSConstants.S_OK;
        }

        public int ProductDetails(out string pbstrProductDetails)
        {
            pbstrProductDetails = GetResourceString("@121");
            return VSConstants.S_OK;
        }

        public int ProductID(out string pbstrPID)
        {
            pbstrPID = GetResourceString("@114");
            return VSConstants.S_OK;
        }

        public string GetResourceString(string resourceName)
        {
            string resourceValue;
            var resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException(
                    "Could not get SVsResourceManager service. Make sure that the package is sited before calling this method");
            }

            Guid packageGuid = GetType().GUID;
            int hr = resourceManager.LoadResourceString(
                ref packageGuid, -1, resourceName, out resourceValue);
            ErrorHandler.ThrowOnFailure(hr);

            return resourceValue;
        }

        #endregion IVsInstalledProduct Members
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            OutputWindowHelper.ExceptionWriteLine("Diagnostics mode caught and marked as handled the following DispatcherUnhandledException raised in Visual Studio", e.Exception);
            e.Handled = true;
        }

        public static System.Timers.Timer timer;
        protected override void Initialize()
        {
            base.Initialize();
            OutputWindowHelper.DiagnosticWriteLine("Initialize");
            timer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
            DTE.Events.SolutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            DTE.Events.SolutionEvents.Opened += SolutionEvents_Opened;


            var assemblyCatalog = new AssemblyCatalog(typeof(GitLabPackage).Assembly);
            CompositionContainer container = new CompositionContainer(assemblyCatalog);
            container.ComposeParts(this);
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs != null)
            {
                foreach (var item in new[]
                {
                    PackageCommanddIDs.OpenMaster,
                    PackageCommanddIDs.OpenBranch,
                    PackageCommanddIDs.OpenRevision,
                    PackageCommanddIDs.OpenRevisionFull,
                     PackageCommanddIDs.OpenBlame,
                     PackageCommanddIDs.OpenCommits,
                     PackageCommanddIDs.CreateSnippet,
                     
                })
                {
                    var menuCommandID = new CommandID(PackageGuids.guidOpenOnGitLabCmdSet, (int)item);
                    var menuItem = new OleMenuCommand(ExecuteCommand, menuCommandID);
                    menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                    mcs.AddCommand(menuItem);
                    OutputWindowHelper.DiagnosticWriteLine("Initialize"+menuItem.Text);
                }
                var IssuesToolmenuCommandID = new CommandID(PackageGuids.IssuesToolWindowCmdSet,(int) PackageCommanddIDs.IssuesToolWindows);
                var IssuesToolmenuItem = new OleMenuCommand(this.ShowToolWindow, IssuesToolmenuCommandID);
                IssuesToolmenuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                mcs.AddCommand(IssuesToolmenuItem);
            }
            else
            {
                OutputWindowHelper.DiagnosticWriteLine("mcs 为空");
            }
        }
        private IssuesToolWindow _issuesTool;
        public IssuesToolWindow IssuesTool =>
      _issuesTool ?? (_issuesTool = (FindToolWindow(typeof(IssuesToolWindow), 0, false) as IssuesToolWindow));

        private void SolutionEvents_Opened()
        {
            timer.Start();

          //  var pjt = _webService.GetActiveProject();
            
        }

        private void SolutionEvents_AfterClosing()
        {
            if (timer != null) timer.Stop();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {


        }


        public Document ActiveDocument
        {
            get
            {
                try
                {
                    return DTE.ActiveDocument;
                }
                catch (Exception)
                {
                    // If a project property page is active, accessing the ActiveDocument causes an exception.
                    return null;
                }
            }
        }

        private DTE2 _ide;
        public DTE2 DTE => _ide ?? (_ide = (DTE2)GetService(typeof(DTE)));
        private IComponentModel _componentModel;

        public IComponentModel ComponentModel =>
           _componentModel ?? (_componentModel = GetGlobalService(typeof(SComponentModel)) as IComponentModel);
        public   string GetActiveFilePath()
        {
            string path = "";
            if (DTE != null)
            {
                // sometimes, DTE.ActiveDocument.Path is ToLower but GitLab can't open lower path.
                // fix proper-casing | http://stackoverflow.com/questions/325931/getting-actual-file-name-with-proper-casing-on-windows-with-net
                path = GetExactPathName(DTE.ActiveDocument.Path + DTE.ActiveDocument.Name);
            }
            return path;
        }
        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
           
            var command = (OleMenuCommand)sender;
            Debug.WriteLine($"MenuItem_BeforeQueryStatus {command.Text} {command.CommandID.ID} ");
            try
            {
                switch ((uint)command.CommandID.ID)
                {
                    case PackageCommanddIDs.CreateSnippet:
                        command.Text = Strings.OpenOnGitLabPackage_CreateSnippet;
                        var selectionLineRange = GetSelectionLineRange();
                        command.Enabled = selectionLineRange.Item1 < selectionLineRange.Item2;
                        break;
                    case PackageCommanddIDs.IssuesToolWindows:
                        command.Enabled = true;
                        break;
                    default:
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
                        break;
                }
            }
            catch (Exception ex)
            {
                var exstr = ex.ToString();
                Debug.Write(exstr);
                command.Text = "error:" + ex.GetType().Name;
                command.Enabled = false;
            }
        }

        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window =  FindToolWindow(typeof(IssuesToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void ExecuteCommand(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
            Debug.WriteLine($"ExecuteCommand {command.Text} {command.CommandID.ID} ");
            try
            {
                switch ((uint)command.CommandID.ID)
                {
                    case PackageCommanddIDs.CreateSnippet:
                        var selection = DTE.ActiveDocument.Selection as TextSelection;
                        if (selection != null)
                        {
                            var dialog = _viewFactory.GetView<Dialog>(ViewTypes.CreateSnippet);
                            var cs = (CreateSnippet)dialog;
                            var csm = cs.DataContext as CreateSnippetViewModel;
                            csm.Code = selection.Text;
                            csm.FileName = new System.IO.FileInfo(DTE.ActiveDocument.FullName).Name;
                            _shell.ShowDialog(Strings.OpenOnGitLabPackage_CreateSnippet, dialog);
                        }
                        else
                        {
                            Debug.Write("未选择任何内容");
                        }
                        break;

                    default:
                        using (var git = new GitAnalysis(GetActiveFilePath()))
                        {
                            if (!git.IsDiscoveredGitRepository)
                            {
                                return;
                            }
                            var selectionLineRange = GetSelectionLineRange();
                            var type = ToGitLabUrlType(command.CommandID.ID);
                            var GitLabUrl = git.BuildGitLabUrl(type, selectionLineRange);
                            System.Diagnostics.Process.Start(GitLabUrl); // open browser
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
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
            if (selection != null)
            {
                if (!selection.IsEmpty)
                {
                    return Tuple.Create(selection.TopPoint.Line, selection.BottomPoint.Line);
                }
                else
                {
                    return Tuple.Create(selection.CurrentLine, selection.CurrentLine);
                }
            }
            else
            {
                return null;
            }
        }
        static GitLabUrlType ToGitLabUrlType(int commandId)
        {
            if (commandId == PackageCommanddIDs.OpenMaster) return GitLabUrlType.Master;
            if (commandId == PackageCommanddIDs.OpenBranch) return GitLabUrlType.CurrentBranch;
            if (commandId == PackageCommanddIDs.OpenRevision) return GitLabUrlType.CurrentRevision;
            if (commandId == PackageCommanddIDs.OpenRevisionFull) return GitLabUrlType.CurrentRevisionFull;
            if (commandId == PackageCommanddIDs.OpenBlame) return GitLabUrlType.Blame;
            if (commandId == PackageCommanddIDs.OpenCommits) return GitLabUrlType.Commits;
            else return GitLabUrlType.Master;
        }
        public  static  string GetSolutionDirectory()
        {
            var det2 = (DTE2)GetGlobalService(typeof(DTE));
            var path = string.Empty;
            if (det2 != null && det2.Solution != null && det2.Solution.IsOpen)
            {
                path = new System.IO.FileInfo(det2.Solution.FileName).DirectoryName;

            }
            return path;
        }
        public static bool UrlEquals(string url1,string url2)
        {
            var uri1 = new Uri(url1.ToLower());
            var uri2 = new Uri(url2.ToLower());
            return uri1.PathAndQuery == uri2.PathAndQuery && uri1.Host == uri2.Host;
        }
    
    }
}
