using GitLab.VisualStudio.Shared;
using GitLab.VisualStudio.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

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
                    foreach (var item in GetProjects(ProjectListType.Membership))
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
            if (IsRestV4(user.ApiVersion))
            {
                return GetProjectFromRest(user, namespacedpath);
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
            if (IsRestV4(user.ApiVersion))
            {
                return GetProjectsFromRest(user, projectListType);
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
            User user;
            if (apiVersion == ApiVersion.V4_Oauth && !enable2fa)
            {
                user = LoginV4WithPasswordGrant(host, email, password);
            }
            else if (apiVersion == ApiVersion.V4 || apiVersion == ApiVersion.V4_Oauth)
            {
                user = LoginV4WithAccessToken(host, password);
            }
            else
            {
                user = LoginWithLegacyClient(enable2fa, host, email, password, apiVersion);
            }

            if (user != null && user.Id > 0)
            {
                _storage.SaveUser(user, user.PrivateToken);
                try
                {
                    LoadProjects();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return user;
        }

        private static User LoginWithLegacyClient(bool enable2fa, string host, string email, string password, ApiVersion apiVersion)
        {
            NGitLab.GitLabClient client;
            NGitLab.Impl.ApiVersion _apiVersion = VsApiVersionToNgitLabversion(apiVersion);
            if (enable2fa)
            {
                client = NGitLab.GitLabClient.Connect(host, password, _apiVersion);
            }
            else
            {
                client = NGitLab.GitLabClient.Connect(host, email, password, _apiVersion);
            }

            User user = client.Users.Current();
            user.PrivateToken = client.ApiToken;
            user.ApiVersion = apiVersion;
            user.Host = host;
            return user;
        }

        private static User LoginV4WithPasswordGrant(string host, string username, string password)
        {
            var token = RequestOAuthPasswordToken(host, username, password);
            return GetCurrentUser(host, token, ApiVersion.V4_Oauth, TokenAuthenticationMode.Bearer);
        }

        private static string RequestOAuthPasswordToken(string host, string username, string password)
        {
            var json = PostForm(BuildGitLabUrl(host, "oauth/token"), new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            });

            var token = json.Value<string>("access_token");
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("OAuth token response does not include access_token.");
            }

            return token;
        }

        private static User LoginV4WithAccessToken(string host, string token)
        {
            try
            {
                return GetCurrentUser(host, token, ApiVersion.V4, TokenAuthenticationMode.PrivateToken);
            }
            catch (Exception privateTokenException)
            {
                try
                {
                    return GetCurrentUser(host, token, ApiVersion.V4_Oauth, TokenAuthenticationMode.Bearer);
                }
                catch (Exception bearerException)
                {
                    throw new InvalidOperationException(
                        $"Access token authentication failed. PRIVATE-TOKEN: {privateTokenException.Message}; Bearer: {bearerException.Message}",
                        bearerException);
                }
            }
        }

        private static User GetCurrentUser(string host, string token, ApiVersion apiVersion, TokenAuthenticationMode authenticationMode)
        {
            var json = GetJson(BuildGitLabUrl(host, "api/v4/user"), token, authenticationMode);
            return new User
            {
                Id = json.Value<int>("id"),
                Username = json.Value<string>("username"),
                Name = json.Value<string>("name"),
                Email = json.Value<string>("email") ?? json.Value<string>("public_email"),
                AvatarUrl = json.Value<string>("avatar_url"),
                TwoFactorEnabled = json.Value<bool?>("two_factor_enabled") ?? false,
                PrivateToken = token,
                ApiVersion = apiVersion,
                Host = new Uri(host).ToString()
            };
        }

        private static bool IsRestV4(ApiVersion apiVersion)
        {
            return apiVersion == ApiVersion.V4 || apiVersion == ApiVersion.V4_Oauth;
        }

        private static IReadOnlyList<Project> GetProjectsFromRest(User user, ProjectListType projectListType)
        {
            var query = "simple=true";
            switch (projectListType)
            {
                case ProjectListType.Owned:
                    query += "&owned=true";
                    break;

                case ProjectListType.Membership:
                    query += "&membership=true";
                    break;

                case ProjectListType.Starred:
                    query += "&starred=true";
                    break;
            }

            return GetPagedArray(user, $"api/v4/projects?{query}")
                .Select(ToProject)
                .ToList();
        }

        private static Project GetProjectFromRest(User user, string namespacedpath)
        {
            var id = Uri.EscapeDataString(namespacedpath);
            return ToProject(GetJson(BuildGitLabUrl(user.Host, $"api/v4/projects/{id}"), user.PrivateToken, GetTokenAuthenticationMode(user)));
        }

        private static IEnumerable<JObject> GetPagedArray(User user, string relativePath)
        {
            var page = 1;
            while (true)
            {
                var separator = relativePath.Contains("?") ? "&" : "?";
                var url = BuildGitLabUrl(user.Host, $"{relativePath}{separator}per_page=100&page={page}");
                var request = CreateJsonRequest(url, user.PrivateToken, GetTokenAuthenticationMode(user));
                var result = ReadJsonArrayResponse(request, out var nextPage);
                foreach (var item in result.OfType<JObject>())
                {
                    yield return item;
                }

                if (string.IsNullOrEmpty(nextPage) || !int.TryParse(nextPage, out page))
                {
                    yield break;
                }
            }
        }

        private static Project ToProject(JObject json)
        {
            var namespaceInfo = json["namespace"] as JObject;
            return new Project
            {
                Id = json.Value<int>("id"),
                Name = json.Value<string>("name"),
                Path = json.Value<string>("path"),
                PathWithNamespace = json.Value<string>("path_with_namespace"),
                Public = string.Equals(json.Value<string>("visibility"), "public", StringComparison.OrdinalIgnoreCase),
                SshUrl = json.Value<string>("ssh_url_to_repo"),
                HttpUrl = json.Value<string>("http_url_to_repo"),
                WebUrl = json.Value<string>("web_url"),
                Fork = json["forked_from_project"] != null,
                IssuesEnabled = json.Value<bool?>("issues_enabled") ?? true,
                MergeRequestsEnabled = json.Value<bool?>("merge_requests_enabled") ?? true,
                WikiEnabled = json.Value<bool?>("wiki_enabled") ?? false,
                BuildsEnabled = json.Value<bool?>("jobs_enabled") ?? json.Value<bool?>("builds_enabled") ?? false,
                SnippetsEnabled = json.Value<bool?>("snippets_enabled") ?? false,
                Description = json.Value<string>("description"),
                Namespace = namespaceInfo?.Value<string>("full_path")
            };
        }

        private enum TokenAuthenticationMode
        {
            PrivateToken,
            Bearer
        }

        private static string BuildGitLabUrl(string host, string relativePath)
        {
            return new Uri(new Uri(host), relativePath).ToString();
        }

        private static JObject PostForm(string url, IDictionary<string, string> form)
        {
            var body = string.Join("&", form.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value ?? string.Empty)}"));
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/x-www-form-urlencoded";
            var bytes = Encoding.UTF8.GetBytes(body);
            request.ContentLength = bytes.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            return ReadJsonResponse(request);
        }

        private static JObject GetJson(string url, string token, TokenAuthenticationMode authenticationMode)
        {
            return ReadJsonResponse(CreateJsonRequest(url, token, authenticationMode));
        }

        private static HttpWebRequest CreateJsonRequest(string url, string token, TokenAuthenticationMode authenticationMode)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "application/json";
            if (authenticationMode == TokenAuthenticationMode.Bearer)
            {
                request.Headers[HttpRequestHeader.Authorization] = $"Bearer {token}";
            }
            else
            {
                request.Headers["PRIVATE-TOKEN"] = token;
            }

            return request;
        }

        private static TokenAuthenticationMode GetTokenAuthenticationMode(User user)
        {
            return user.ApiVersion == ApiVersion.V4_Oauth ? TokenAuthenticationMode.Bearer : TokenAuthenticationMode.PrivateToken;
        }

        private static JArray ReadJsonArrayResponse(HttpWebRequest request, out string nextPage)
        {
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    nextPage = response.Headers["X-Next-Page"];
                    return JArray.Parse(reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                throw CreateGitLabRequestException(ex);
            }
        }

        private static JObject ReadJsonResponse(HttpWebRequest request)
        {
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    return JObject.Parse(reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                throw CreateGitLabRequestException(ex);
            }
        }

        private static Exception CreateGitLabRequestException(WebException ex)
        {
            var response = ex.Response as HttpWebResponse;
            if (response == null)
            {
                return ex;
            }

            string body;
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                body = reader.ReadToEnd();
            }

            var status = $"{(int)response.StatusCode} {response.StatusDescription}";
            var message = ExtractGitLabErrorMessage(body);
            return new InvalidOperationException(string.IsNullOrEmpty(message) ? status : $"{status}: {message}", ex);
        }

        private static string ExtractGitLabErrorMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            try
            {
                var json = JObject.Parse(body);
                return json.Value<string>("error_description")
                    ?? json.Value<string>("error")
                    ?? json.Value<string>("message")
                    ?? body;
            }
            catch (JsonException)
            {
                return body;
            }
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

        public IReadOnlyList<GitLabWorkItem> GetIssues(GitLabWorkItemQuery query)
        {
            var user = GetStoredUser();
            var projectPath = ResolveProjectPath(query?.ProjectPath);
            var parameters = BuildWorkItemQuery(query, false);
            return GetPagedArray(user, $"api/v4/projects/{Uri.EscapeDataString(projectPath)}/issues?{parameters}")
                .Select(ToIssue)
                .ToList();
        }

        public IReadOnlyList<GitLabWorkItem> GetMergeRequests(GitLabWorkItemQuery query)
        {
            var user = GetStoredUser();
            var projectPath = ResolveProjectPath(query?.ProjectPath);
            var parameters = BuildWorkItemQuery(query, true);
            return GetPagedArray(user, $"api/v4/projects/{Uri.EscapeDataString(projectPath)}/merge_requests?{parameters}")
                .Select(ToMergeRequest)
                .ToList();
        }

        private NGitLab.GitLabClient GetClient()
        {
            var user = GetStoredUser();
            var client = NGitLab.GitLabClient.Connect(user.Host, user.PrivateToken, VsApiVersionToNgitLabversion(user.ApiVersion));
            return client;
        }

        private User GetStoredUser()
        {
            var user = _storage.GetUser();
            if (user == null)
            {
                throw new UnauthorizedAccessException(Strings.WebService_CreateProject_NotLoginYet);
            }
            return user;
        }

        private string ResolveProjectPath(string configuredProjectPath)
        {
            if (!string.IsNullOrWhiteSpace(configuredProjectPath))
            {
                return configuredProjectPath.Trim();
            }

            var project = GetActiveProject();
            if (project == null)
            {
                throw new InvalidOperationException("GitLab project is not configured and no active GitLab repository was resolved.");
            }

            if (!string.IsNullOrWhiteSpace(project.PathWithNamespace))
            {
                return project.PathWithNamespace;
            }

            if (!string.IsNullOrWhiteSpace(project.Namespace) && !string.IsNullOrWhiteSpace(project.Path))
            {
                return $"{project.Namespace}/{project.Path}";
            }

            return project.Id.ToString();
        }

        private static string BuildWorkItemQuery(GitLabWorkItemQuery query, bool includeTargetBranch)
        {
            var parameters = new Dictionary<string, string>
            {
                { "state", string.IsNullOrWhiteSpace(query?.State) ? "opened" : query.State.Trim() },
                { "scope", string.IsNullOrWhiteSpace(query?.Scope) ? "all" : query.Scope.Trim() }
            };

            AddQuery(parameters, "author_username", query?.AuthorUsername);
            AddQuery(parameters, "assignee_username", query?.AssigneeUsername);
            AddQuery(parameters, "labels", query?.Labels);
            AddQuery(parameters, "search", query?.Search);
            if (includeTargetBranch)
            {
                AddQuery(parameters, "target_branch", query?.TargetBranch);
            }

            return string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        }

        private static void AddQuery(IDictionary<string, string> parameters, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                parameters[key] = value.Trim();
            }
        }

        private static GitLabWorkItem ToIssue(JObject json)
        {
            return ToWorkItem(json, null, null, "#");
        }

        private static GitLabWorkItem ToMergeRequest(JObject json)
        {
            return ToWorkItem(
                json,
                json.Value<string>("source_branch"),
                json.Value<string>("target_branch"),
                "!");
        }

        private static GitLabWorkItem ToWorkItem(JObject json, string sourceBranch, string targetBranch, string referencePrefix)
        {
            var labels = json["labels"] is JArray labelArray
                ? string.Join(", ", labelArray.Select(x => x.ToString()))
                : string.Empty;
            var assignees = json["assignees"] is JArray assigneeArray
                ? string.Join(", ", assigneeArray.OfType<JObject>().Select(x => x.Value<string>("username")).Where(x => !string.IsNullOrEmpty(x)))
                : string.Empty;

            return new GitLabWorkItem
            {
                Id = json.Value<int>("id"),
                Iid = json.Value<int>("iid"),
                Title = json.Value<string>("title"),
                State = json.Value<string>("state"),
                Author = (json["author"] as JObject)?.Value<string>("username"),
                Assignees = assignees,
                Labels = labels,
                WebUrl = json.Value<string>("web_url"),
                ReferencePrefix = referencePrefix,
                SourceBranch = sourceBranch,
                TargetBranch = targetBranch,
                CreatedAt = json.Value<DateTime?>("created_at"),
                UpdatedAt = json.Value<DateTime?>("updated_at")
            };
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
