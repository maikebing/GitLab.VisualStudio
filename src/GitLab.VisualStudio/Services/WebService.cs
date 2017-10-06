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

        private HttpWebRequest GetRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            //request.UserAgent = "CC/1.0-$ExtensionValue ExtensionValue";

            return request;
        }
        List<Project>  lstProject = new List<Project>();
        DateTime dt = DateTime.MinValue;
        public IReadOnlyList<Project> GetProjects()
        {
            lock (lstProject)
            {


                if (DateTime.Now.Subtract(dt).TotalMinutes > 1)
                {
                    lstProject.Clear();
                    var user = _storage.GetUser();
                    if (user == null)
                    {
                        throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
                    }
                    var page = 1;
                    while (true)
                    {
                        var projects = GetProjectsOfPage(page, user.Token);
                        if (!projects.Any())
                        {
                            break;
                        }

                        page++;
                        lstProject.AddRange(projects);
                    }
                    dt = DateTime.Now;
                }
            }
            return lstProject;
        }

        private IReadOnlyList<Project> GetProjectsOfPage(int page, string token)
        {
            var url = $"{_storage.GetUser().Host}/api/v4/projects?page={page}&private_token={token}";

            var request = GetRequest(url);
            request.Method = "GET";

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var reader = new JsonTextReader(new StreamReader(response.GetResponseStream())))
                {
                    var serializer = new JsonSerializer();
                    var repositories = serializer.Deserialize<List<Project>>(reader);

                    return repositories;
                }
            }
            catch (WebException ex)
            {
                var res = (HttpWebResponse)ex.Response;
                var statusCode = (int)res.StatusCode;

                throw new Exception($"错误代码: {statusCode}");
            }
        }
   
        public User Login(bool enable2fa,string host,string email, string password)
        {
            HttpWebRequest request = null;
            if (enable2fa)
            {
                
                request = GetRequest($"{host}/api/v4/user?private_token={password}");
                request.Method = "GET";
            }
            else
            {

                request = GetRequest($"{host}/api/v4/session");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                var content = (email.Contains("@") ? $"email={HttpUtility.UrlEncode(email)}" : $"login={HttpUtility.UrlEncode(email)}") + $"&password={HttpUtility.UrlEncode(password)}";
                var bytes = Encoding.UTF8.GetBytes(content);
                request.ContentLength = bytes.Length;
                request.GetRequestStream().Write(bytes, 0, bytes.Length);
            }
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var reader = new JsonTextReader(new StreamReader(response.GetResponseStream())))
                {
                    var serializer = new JsonSerializer();
                    var user = serializer.Deserialize<User>(reader);
                    user.Host = host;
                    return user;
                }
            }
            catch (WebException ex)
            {
                var res = (HttpWebResponse)ex.Response;
                var statusCode = (int)res.StatusCode;

                throw new Exception($"错误代码: {statusCode}");
            }
        }

        public CreateResult CreateProject(string name, string description, bool isPrivate)
        {
            var user = _storage.GetUser();
            if (user == null)
            {
                throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
            }

            var result = new CreateResult();

            var url = $"{user.Host}/api/v4/projects?private_token={user.Token}";

            var request = GetRequest(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            var body = string.Format("name={0}&description={1}&private={2}",
                HttpUtility.UrlEncode(name),
                HttpUtility.UrlEncode(description),
                isPrivate ? 1 : 0);

            var bytes = Encoding.UTF8.GetBytes(body);
            request.ContentLength = bytes.Length;
            request.GetRequestStream().Write(bytes, 0, bytes.Length);

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var txt = reader.ReadToEnd();
                    var o = (JObject)JsonConvert.DeserializeObject(txt);

                    var message = o["message"];
                    if (message != null)
                    {
                        result.Message = message.Value<string>();
                    }
                    else
                    {
                        var project = JsonConvert.DeserializeObject<Project>(txt);
                        result.Project = project;

                    }
                }
                return result;
            }
            catch (WebException ex)
            {
                var res = (HttpWebResponse)ex.Response;
                var statusCode = (int)res.StatusCode;

                throw new Exception($"错误代码: {statusCode}");
            }
        }
    }
}
