using System;
using System.Collections.Generic;

namespace GitLab.VisualStudio.Shared
{
    public class RepositoryInfo
    {
        public string Branch { get; set; }
        public string Path { get; set; }
    }

    public interface IVisualStudioService
    {
        //string Branch { get; }

        //Project Current { get; }

        

        //IReadOnlyList<Project> Projects { get; set; }

        //IEnumerable<Repository> Repositories { get; set; }

        
    }
}
