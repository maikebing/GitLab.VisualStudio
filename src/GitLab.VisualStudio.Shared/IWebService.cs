using GitLab.VisualStudio.Shared.Models;
using System.Collections.Generic;

namespace GitLab.VisualStudio.Shared
{
    public enum ApiVersion
    {
        V3,
        V4,
        V3_Oauth,
        V4_Oauth,
        V3_1,
        AutoDiscovery
    }

    public enum ProjectListType
    {
        Accessible,
        Owned,
        Membership,
        Starred,
        Forked
    }

    public interface IWebService
    {
        User LoginAsync(bool enable2fa, string host, string email, string password, ApiVersion apiVersion);

        IReadOnlyList<Project> GetProjects();

        void LoadProjects();

        CreateProjectResult CreateProject(string name, string description, string VisibilityLevel, int namespaceid);

        CreateProjectResult CreateProject(string name, string description, string VisibilityLevel);

        CreateSnippetResult CreateSnippet(string title, string filename, string description, string code, string visibility);

        Project GetActiveProject();

        Project GetActiveProject(ProjectListType projectListType);

        IReadOnlyList<Project> GetProjects(ProjectListType projectListType);

        Project GetProject(string namespacedpath);

        IReadOnlyList<NamespacesPath> GetNamespacesPathList();
    }
}