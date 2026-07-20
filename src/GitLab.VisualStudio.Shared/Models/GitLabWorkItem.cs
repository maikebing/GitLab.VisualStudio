using System;

namespace GitLab.VisualStudio.Shared.Models
{
    public class GitLabWorkItem
    {
        public int Id { get; set; }
        public int Iid { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string Author { get; set; }
        public string Assignees { get; set; }
        public string Labels { get; set; }
        public string WebUrl { get; set; }
        public string ReferencePrefix { get; set; }
        public string SourceBranch { get; set; }
        public string TargetBranch { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Reference
        {
            get { return Iid > 0 ? (ReferencePrefix ?? string.Empty) + Iid : string.Empty; }
        }
    }
}
