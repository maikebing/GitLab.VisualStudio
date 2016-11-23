using System;

namespace CodeCloud.VisualStudio.Shared
{
    public class RepositoryInfo
    {
        public string Branch { get; set; }
        public string Path { get; set; }
    }

    public interface IVisualStudioService
    {
        IServiceProvider ServiceProvider { get; set; }
        RepositoryInfo GetActiveRepository();

        string GetSolutionPath();
    }
}
