using GitLab.VisualStudio.Shared.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLab.VisualStudio.Shared
{
    public class User:NGitLab.Models.Session
    {
        
        public string Host { get; set; }
      
      
    }

    public class Project:NGitLab.Models.Project
    {
         
        public new string Url
        {
            get { return HttpUrl; }
        }

        [JsonIgnore]
        public string LocalPath { get; set; }

        [JsonIgnore]
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
