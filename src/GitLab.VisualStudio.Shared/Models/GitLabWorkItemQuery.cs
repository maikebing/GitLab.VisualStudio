namespace GitLab.VisualStudio.Shared.Models
{
    public class GitLabWorkItemQuery
    {
        public string ProjectPath { get; set; }
        public string State { get; set; }
        public string Scope { get; set; }
        public string AuthorUsername { get; set; }
        public string AssigneeUsername { get; set; }
        public string Labels { get; set; }
        public string Search { get; set; }
        public string TargetBranch { get; set; }
    }
}
