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
        DateTime dt = DateTime.MinValue;
        public IReadOnlyList<Project> GetProjects()
        {
            lock (lstProject)
            {
                var user = _storage.GetUser();
                if (user == null)
                {
                    throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
                }
                var client = NGitLab.GitLabClient.Connect(user.Host, user.PrivateToken);
                lstProject.AddRange((Project[])client.Projects.Owned().ToArray());

                dt = DateTime.Now;
            }
            return lstProject;
        }
 
   
        public  User  LoginAsync(bool enable2fa,string host,string email, string password)
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
                user = (User)client.Users.Current();

            }
            catch (WebException ex)
            {
                var res = (HttpWebResponse)ex.Response;
                var statusCode = (int)res.StatusCode;

                throw new Exception($"错误代码: {statusCode}");
            }
            return user;
        }

        public CreateResult CreateProject(string name, string description, bool isPrivate)
        {
            var user = _storage.GetUser();
            if (user == null)
            {
                throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
            }
            var result = new CreateResult();
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
    }
}
