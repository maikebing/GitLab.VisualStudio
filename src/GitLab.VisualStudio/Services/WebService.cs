using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GitLab.VisualStudio.Services
{
    [Export(typeof(IWebService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WebService : IWebService
    {
        [Import]
        private IStorage _storage;

        private List<Project> lstProject = new List<Project>();
        private DateTime dts = DateTime.MinValue;

        public void LoadProjects()
        {
            lock (lstProject)
            {
                if (lstProject.Count == 0 || Math.Abs(DateTime.Now.Subtract(dts).TotalSeconds) > 30)
                {
                    var client = GetClient();
                    foreach (var item in client.Projects.Membership())
                    {
                        if (!lstProject.Any(p => p.Id == item.Id))
                        {
                            lstProject.Add(item);
                        }
                    }
                    dts = DateTime.Now;
                }
            }
        }

        public IReadOnlyList<Project> GetProjects()
        {
            LoadProjects();
            return lstProject;
        }

        public Project GetProject(string namespacedpath)

        {
            var user = _storage.GetUser();
            if (user == null)
            {
                throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
            }
            var client = NGitLab.GitLabClient.Connect(user.Host, user.PrivateToken, VsApiVersionToNgitLabversion(user.ApiVersion));
            var pjt = client.Projects.Get(namespacedpath);
            return pjt;
        }

        public IReadOnlyList<Project> GetProjects(ProjectListType projectListType)
        {
            List<Project> lstpjt = new List<Project>();
            var user = _storage.GetUser();
            if (user == null)
            {
                throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
            }
            var client = NGitLab.GitLabClient.Connect(user.Host, user.PrivateToken, VsApiVersionToNgitLabversion(user.ApiVersion));
            NGitLab.Models.Project[] project = null;
            switch (projectListType)
            {
                case ProjectListType.Accessible:
                    project = client.Projects.Accessible().ToArray();
                    break;

                case ProjectListType.Owned:
                    project = client.Projects.Owned().ToArray();
                    break;

                case ProjectListType.Membership:
                    project = client.Projects.Membership().ToArray();
                    break;

                case ProjectListType.Starred:
                    project = client.Projects.Starred().ToArray();
                    break;

                default:
                    break;
            }
            if (project != null)
            {
                foreach (var item in project)
                {
                    lstpjt.Add(item);
                }
            }
            return lstpjt;
        }

        public User Login(bool enable2fa, string host, string email, string password, ApiVersion apiVersion)
        {
            NGitLab.GitLabClient client = null;
            User user = null;
            NGitLab.Impl.ApiVersion _apiVersion = VsApiVersionToNgitLabversion(apiVersion);
            if (enable2fa)
            {
                client = NGitLab.GitLabClient.Connect(host, password, _apiVersion);
            }
            else
            {
                client = NGitLab.GitLabClient.Connect(host, email, password, _apiVersion);
            }
            try
            {
                user = client.Users.Current();
                user.PrivateToken = client.ApiToken;
                user.ApiVersion = apiVersion;
                user.Host = host;
                if (user != null && user.Id > 0)
                {
                    _storage.SaveUser(user, password);
                    LoadProjects();
                }
            }
            catch (WebException ex)
            {
                var res = (HttpWebResponse)ex.Response;
                var statusCode = (int)res.StatusCode;

                throw ex;
            }
            catch (Exception ex1)
            {
                throw ex1;
            }
            return user;
        }

        private static NGitLab.Impl.ApiVersion VsApiVersionToNgitLabversion(ApiVersion apiVersion)
        {
            var result = NGitLab.Impl.ApiVersion.V4_Oauth;
            Enum.TryParse(apiVersion.ToString(), out result);
            return result;
        }

        public CreateProjectResult CreateProject(string name, string description, string VisibilityLevel)
        {
            return CreateProject(name, description, VisibilityLevel, 0);
        }

        public IReadOnlyList<NamespacesPath> GetNamespacesPathList()
        {
            List<NamespacesPath> nplist = new List<NamespacesPath>();
            try
            {
                NGitLab.GitLabClient client = GetClient();
                foreach (var item in client.Groups.GetNamespaces())
                {
                    nplist.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return nplist;
        }

        private NGitLab.GitLabClient GetClient()
        {
            var user = _storage.GetUser();
            if (user == null)
            {
                throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
            }
            var client = NGitLab.GitLabClient.Connect(user.Host, user.PrivateToken, VsApiVersionToNgitLabversion(user.ApiVersion));
            return client;
        }

        public CreateProjectResult CreateProject(string name, string description, string VisibilityLevel, int namespaceid)
        {
            var result = new CreateProjectResult();
            try
            {
                NGitLab.Models.VisibilityLevel vl_temp = NGitLab.Models.VisibilityLevel.Private;
                if (!Enum.TryParse(VisibilityLevel, out vl_temp))
                {
                    vl_temp = NGitLab.Models.VisibilityLevel.Private;
                }
                var client = GetClient();
                var pjt = client.Projects.Create(
                    new NGitLab.Models.ProjectCreate()
                    {
                        Description = description,
                        Name = name,
                        VisibilityLevel = vl_temp,
                        IssuesEnabled = true,
                        ContainerRegistryEnabled = true,
                        JobsEnabled = true,
                        LfsEnabled = true,
                        SnippetsEnabled = true,
                        WikiEnabled = true,
                        MergeRequestsEnabled = true
                            ,
                        NamespaceId = namespaceid
                    });
                result.Project = pjt;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        public CreateSnippetResult CreateSnippet(string title, string filename, string description, string code, string visibility)
        {
            CreateSnippetResult result = new CreateSnippetResult() { Message = "", Snippet = null };

            try
            {
                var client = GetClient();
                var pjt = GetActiveProject();
                if (pjt.SnippetsEnabled)
                {
                    var snp = client.GetRepository(pjt.Id)
                             .ProjectSnippets
                                     .Create(
                                      new NGitLab.Models.ProjectSnippetInsert()
                                      {
                                          Title = title
                                          ,
                                          Code = code
                                          ,
                                          Description = description
                                          ,
                                          FileName = filename
                                          ,
                                          Visibility = visibility
                                          ,
                                          Id = pjt.Id
                                      });
                    result.Snippet = snp;
                }
                else
                {
                    result.Message = Strings.TheSnippetsIsNotEnabled;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public Project GetActiveProject()
        {
            using (GitAnalysis ga = new GitAnalysis(GitLabPackage.GetSolutionDirectory()))
            {
                var url = ga.GetRepoOriginRemoteUrl();
                var pjt = from project in this.GetProjects() where string.Equals(project.Url, url, StringComparison.OrdinalIgnoreCase) select project;
                return pjt.FirstOrDefault();
            }
        }

        public Project GetActiveProject(ProjectListType projectListType)
        {
            using (GitAnalysis ga = new GitAnalysis(GitLabPackage.GetSolutionDirectory()))
            {
                var url = ga.GetRepoOriginRemoteUrl();
                var pjt = from project in this.GetProjects(projectListType) where string.Equals(project.Url, url, StringComparison.OrdinalIgnoreCase) select project;
                return pjt.FirstOrDefault();
            }
        }

        public bool CheckHaveNewChange()
        {
            bool ok = false;
            var pjt = GetActiveProject();
            if (pjt != null)
            {
                var client = GetClient();
            }
            return ok;
        }
    }
}