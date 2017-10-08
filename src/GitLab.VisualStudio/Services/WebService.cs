using GitLab.VisualStudio.Shared;
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

        List<Project>  lstProject = new List<Project>();
        DateTime dts = DateTime.MinValue;
        public IReadOnlyList<Project> GetProjects()
        {
            lock (lstProject)
            {
                if (lstProject.Count==0 ||  Math.Abs(DateTime.Now.Subtract(dts).TotalSeconds)>5)//缓存五秒
                {
                    lstProject.Clear();
                    var user = _storage.GetUser();
                    if (user == null)
                    {
                        throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
                    }
                    var client = NGitLab.GitLabClient.Connect(user.Host, user.PrivateToken);
                    foreach (var item in client.Projects.Membership())
                    {
                        lstProject.Add(item);
                    }
                    dts = DateTime.Now;
                }
              
            }
            return lstProject;
        }
 
   
        public User LoginAsync(bool enable2fa,string host,string email, string password)
        {
            NGitLab.GitLabClient client = null;
            User user = null;
            if (enable2fa)
            {
                   client =   NGitLab.GitLabClient.Connect(host,password);
               
            }
            else
            {
                  client =   NGitLab.GitLabClient.Connect(host,email,password);
            }
            try
            {
                user = client.Users.Current();
                user.PrivateToken = client.ApiToken;
            }
            catch (WebException ex)
            {
                var res = (HttpWebResponse)ex.Response;
                var statusCode = (int)res.StatusCode;

                throw new Exception($"错误代码: {statusCode}");
            }
            return user;
        }

        public CreateProjectResult CreateProject(string name, string description, bool isPrivate)
        {
            var user = _storage.GetUser();
            if (user == null)
            {
                throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
            }
            var result = new CreateProjectResult();
            try
            {

                var client = NGitLab.GitLabClient.Connect(user.Host, user.PrivateToken);
                var pjt = client.Projects.Create(new NGitLab.Models.ProjectCreate() { Description = description, Name = name, VisibilityLevel = isPrivate ? NGitLab.Models.VisibilityLevel.Private : NGitLab.Models.VisibilityLevel.Public });
                result.Project = (Project)pjt;

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        public CreateSnippetResult CreateSnippet(string title ,string filename, string description,string code, string visibility)
        {
            CreateSnippetResult result = new CreateSnippetResult() {  Message="", Snippet=null};
            var user = _storage.GetUser();
      
            if (user == null)
            {
                throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
            }
            try
            {
                var client = NGitLab.GitLabClient.Connect(user.Host, user.PrivateToken);
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
                    result.Message = "The snippets  is not enabled";
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
    }
}
