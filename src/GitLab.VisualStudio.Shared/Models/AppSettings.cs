using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLab.VisualStudio.Shared.Models
{
    public class AppSettings
    {
        public string BasePath { get; set; }
        public string DefaultBranch { get; set; }
        public string IssuesProjectPath { get; set; }
        public string MergeRequestsProjectPath { get; set; }
    }
}
