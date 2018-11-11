using EnvDTE;
using EnvDTE80;
using GitLab.VisualStudio.Helpers;
using GitLab.VisualStudio.Services;
using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.UI.ViewModels;
using GitLab.VisualStudio.UI.Views;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace GitLab.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideBindingPath]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuids.guidGitLabPackagePkgString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(GitLabToolWindow), MultiInstances = false, Height = 100, Width = 500, Style = Microsoft.VisualStudio.Shell.VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom, Window = EnvDTE.Constants.vsWindowKindMainWindow)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.RepositoryOpen_string, PackageAutoLoadFlags.BackgroundLoad )]
    public class GitLabPackage : AsyncPackage, IVsInstalledProduct
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
            pIdBmp = 400;
            return VSConstants.S_OK;
        }

        public int IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = 400;
            return VSConstants.S_OK;
        }

        public int OfficialName(out string pbstrName)
        {
            pbstrName =  GetResourceString("@101");
            return VSConstants.S_OK;
        }

        public int ProductDetails(out string pbstrProductDetails)
        {
            pbstrProductDetails = Vsix.Description;
            return VSConstants.S_OK;
        }

        public int ProductID(out string pbstrPID)
        {
            pbstrPID = Vsix.Id;
            return VSConstants.S_OK;
        }

        public  string   GetResourceString(string resourceName)
        {
            string resourceValue;
           
            var resourceManager =  (IVsResourceManager)GetService(typeof(SVsResourceManager));
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
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            await JoinableTaskFactory.RunAsync(VsTaskRunContext.UIThreadNormalPriority, async delegate
                    {

                        timer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
                        timer.Elapsed += Timer_Elapsed;
                        DTE.Events.SolutionEvents.AfterClosing += SolutionEvents_AfterClosing;
                        DTE.Events.SolutionEvents.Opened += SolutionEvents_Opened;
                        var assemblyCatalog = new AssemblyCatalog(typeof(GitLabPackage).Assembly);
                        CompositionContainer container = new CompositionContainer(assemblyCatalog);
                        container.ComposeParts(this);
                        var mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                        if (mcs != null)
                        {
                            foreach (var item in new[]
                            {
                    PackageIds.OpenMaster,
                    PackageIds.OpenBranch,
                    PackageIds.OpenRevision,
                    PackageIds.OpenRevisionFull,
                     PackageIds.OpenBlame,
                     PackageIds.OpenCommits,
                     PackageIds.OpenCreateSnippet,
                     PackageIds.OpenFromUrl
                               })
                            {
                                var menuCommandID = new CommandID(PackageGuids.guidOpenOnGitLabCmdSet, (int)item);
                                var menuItem = new OleMenuCommand(ExecuteCommand, menuCommandID);
                                menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                                mcs.AddCommand(menuItem);
                            }
                            var IssuesToolmenuCommandID = new CommandID(PackageGuids.guidIssuesToolWindowPackageCmdSet, (int)PackageIds.IssuesToolWindowCommandId);
                            var IssuesToolmenuItem = new OleMenuCommand(this.ShowToolWindow, IssuesToolmenuCommandID);
                            IssuesToolmenuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                            IssuesToolmenuItem.Enabled = false;
                            mcs.AddCommand(IssuesToolmenuItem);
                        }
                        else
                        {
                            OutputWindowHelper.DiagnosticWriteLine("mcs 为空");
                        }

                    });
        }
        private GitLabToolWindow _issuesTool;

        public GitLabToolWindow IssuesTool =>
      _issuesTool ?? (_issuesTool = (FindToolWindow(typeof(GitLabToolWindow), 0, false) as GitLabToolWindow));

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
        public DTE2 DTE
        {
            get
            {
                if (_ide == null)
                {
                    ThreadHelper.JoinableTaskFactory.Run(async delegate
                    {
                        _ide = (DTE2)await GetServiceAsync(typeof(DTE));
                    });
                }
                return _ide;
            }
        }

        private IComponentModel _componentModel;

        public IComponentModel ComponentModel =>
           _componentModel ?? (_componentModel = GetGlobalService(typeof(SComponentModel)) as IComponentModel);

        public string GetActiveFilePath()
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
                    case PackageIds.OpenCreateSnippet:
                        command.Text = Strings.OpenOnGitLabPackage_CreateSnippet;
                        var selectionLineRange = GetSelectionLineRange();
                        command.Enabled = selectionLineRange.Item1 < selectionLineRange.Item2;
                        break;

                    case PackageIds.IssuesToolWindowCommandId:
                        command.Enabled = true;
                        break;
                    case PackageIds.OpenFromUrl:
                        command.Enabled = Clipboard.ContainsText(TextDataFormat.Text) && Regex.IsMatch(Clipboard.GetText(TextDataFormat.Text), "[a-zA-z]+://[^\\s]*");
                        command.Text = Strings.OpenFormURL;
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
            ToolWindowPane window = FindToolWindow(typeof(GitLabToolWindow), 0, true);
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
 
            try
            {
                switch ((uint)command.CommandID.ID)
                {
                    case PackageIds.OpenCreateSnippet:
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
                            OutputWindowHelper.DiagnosticWriteLine(GitLab.VisualStudio.Shared.Strings.PleaseCodes);
                        }
                        break;
                    case PackageIds.OpenFromUrl:

                        if (Clipboard.ContainsText(TextDataFormat.Text))
                        {
                            var match = Regex.Match(Clipboard.GetText(TextDataFormat.Text), "[a-zA-z]+://[^\\s]*");
                            if (match.Success)
                            {
                                try
                                {
                                    TryOpenFile(match.Value);
                                }
                                catch (Exception ex)
                                {
                                    OutputWindowHelper.ExceptionWriteLine(string.Format(GitLab.VisualStudio.Shared.Strings.Canotopenurl, match.Value,ex.Message), ex);
                                }
                            }
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
                OutputWindowHelper.ExceptionWriteLine($"Command:{command.Text}，Message:{ex.Message}",ex);
            }
        }
        public void  TryOpenFile( string url)
        {
            Uri uri = new Uri(url);
            using (var git = new GitAnalysis(GetActiveFilePath()))
            {
                if (git.IsDiscoveredGitRepository)
                {
                    var blob=Regex.Match(url, "/blob/(?<treeish>[^/]*)/");
                    if (blob.Success)
                    {
                        string p1 = uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped).ToString();
                        string p2 = p1.Substring(p1.IndexOf(blob.Value) + blob.Value.Length);
                        var path = System.IO.Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(git.RepositoryPath, "../")),  p2);
                        var textView = OpenDocument(path);
                    }
                }
            }
        }
        IVsTextView OpenDocument(string fullPath)
        {
            var logicalView = VSConstants.LOGVIEWID.TextView_guid;
            IVsUIHierarchy hierarchy;
            uint itemID;
            IVsWindowFrame windowFrame;
            IVsTextView view;
          //  DTE.Solution.FindProjectItem(fullPath).Open();
            VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, fullPath, logicalView, out hierarchy, out itemID, out windowFrame, out view);
            return view;
        }
        private static string GetExactPathName(string pathName)
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

        private Tuple<int, int> GetSelectionLineRange()
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

        private static GitLabUrlType ToGitLabUrlType(int commandId)
        {
            if (commandId == PackageIds.OpenMaster) return GitLabUrlType.Master;
            if (commandId == PackageIds.OpenBranch) return GitLabUrlType.CurrentBranch;
            if (commandId == PackageIds.OpenRevision) return GitLabUrlType.CurrentRevision;
            if (commandId == PackageIds.OpenRevisionFull) return GitLabUrlType.CurrentRevisionFull;
            if (commandId == PackageIds.OpenBlame) return GitLabUrlType.Blame;
            if (commandId == PackageIds.OpenCommits) return GitLabUrlType.Commits;
            else return GitLabUrlType.Master;
        }

        public static string GetSolutionDirectory()
        {
            var det2 = (DTE2)GetGlobalService(typeof(DTE));
            var path = string.Empty;
            if (det2 != null && det2.Solution != null && det2.Solution.IsOpen)
            {
                path = new System.IO.FileInfo(det2.Solution.FileName).DirectoryName;
            }
            return path;
        }

        public static bool UrlEquals(string url1, string url2)
        {
            var uri1 = new Uri(url1.ToLower());
            var uri2 = new Uri(url2.ToLower());
            return uri1.PathAndQuery == uri2.PathAndQuery && uri1.Host == uri2.Host;
        }
    }
}