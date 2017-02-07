using System;
using System.Collections.Generic;

namespace CodeCloud.VisualStudio.Shared
{
    public class RepositoryInfo
    {
        public string Branch { get; set; }
        public string Path { get; set; }
    }

    public interface IVisualStudioService
    {
        string Branch { get; }

        event Action SolutionChanged;

        Project Current { get; }

        

        bool IsCodeCloudProject { get; }

        IReadOnlyList<Project> Projects { get; set; }

        IEnumerable<Repository> Repositories { get; set; }

        IServiceProvider ServiceProvider { get; set; }

        
    }
}
