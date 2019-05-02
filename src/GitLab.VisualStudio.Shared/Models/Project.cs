using GitLab.VisualStudio.Shared.Controls;

namespace GitLab.VisualStudio.Shared.Models
{
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
                    Id = p.Id,
                    WebUrl = p.WebUrl,
                    Description = p.Description,
                    Namespace = p.Namespace.FullPath                    
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
        public string WebUrl { get; set; }
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

    public string Description { get; set; }
    public string Namespace { get; set; }
}
}