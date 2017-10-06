using GitLab.VisualStudio.Shared.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLab.VisualStudio.Shared
{
    public class User
    {
        public static implicit operator User(NGitLab.Models.Session session)
        {
            return (NGitLab.Models.User)session;
        }
        public static implicit operator User(NGitLab.Models.User session)
        {
            if (session != null)
            {
                return new User()
                {
                    AvatarUrl = session.AvatarUrl,
                    Email = session.Email,
                    Id = session.Id,
                    Name = session.Name,
                    TwoFactorEnabled = session.TwoFactorEnabled,
                    Username = session.Username
                };
            }
            else
            {
                return null;
            }
        }
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string PrivateToken { get; set; }
        public string Host { get; set; }
        public bool TwoFactorEnabled { get; set; }
 
    }

    public class Project
    {
        public static implicit operator Project(NGitLab.Models.Project p)
        {
            if (p != null)
            {
                 return new Project()
                {
                    BuildsEnabled = p.BuildsEnabled,
                    Fork = p.Fork,
                    HttpUrl = p.HttpUrl,
                    IssuesEnabled = p.IssuesEnabled,
                    Name = p.Name,
                    Owner = p.Owner,
                    Public = p.Public,
                    Path = p.Path,
                    MergeRequestsEnabled = p.MergeRequestsEnabled,
                    SnippetsEnabled = p.SnippetsEnabled,
                    SshUrl = p.SshUrl,
                    WikiEnabled = p.WikiEnabled,
                    Id = p.Id
                };
                
            }
            else
            {
                return null;
            }
        }

        public int Id { get; set; }

    
        public string Name { get; set; }

        public string Path { get; set; }

        public bool Public { get; set; }
        public string SshUrl { get; set; }
        public string HttpUrl { get; set; }
      
        public User Owner { get; set; }

        public bool Fork { get; set; }

        public bool IssuesEnabled { get; set; }

        public bool MergeRequestsEnabled { get; set; }

        public bool WikiEnabled { get; set; }
        public bool BuildsEnabled { get; set; }
        public bool SnippetsEnabled { get; set; }

        public string Url
        {
            get { return HttpUrl; }
        }

     
        public string LocalPath { get; set; }

 
        public Octicon Icon
        {
            get
            {
                return Public ? Octicon.@lock
                    : Fork
                    ? Octicon.repo_forked
                    : Octicon.repo;
            }
        }
    }

    public class CreateResult 
    {
        public string Message { get; set; }
        public Project Project { get; set; }
    }

    public interface IWebService
    {
        User LoginAsync(bool enable2fa, string host,string email, string password);
        IReadOnlyList<Project> GetProjects();
        CreateResult CreateProject(string name, string description, bool isPrivate);
    }
}
