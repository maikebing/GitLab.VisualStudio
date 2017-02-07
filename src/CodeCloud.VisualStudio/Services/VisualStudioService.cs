using CodeCloud.VisualStudio.Shared;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace CodeCloud.VisualStudio.Services
{
    [Export(typeof(IVisualStudioService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VisualStudioService : IVisualStudioService
    {
        [Import]
        private IGitService _git;

        [Import]
        private ITeamExplorerServices _tes;

        public int IGitExt { get; private set; }
        public string Branch { get; private set; }
        public Project Current { get; private set; }

        public bool IsCodeCloudProject { get; private set; }

        public IReadOnlyList<Project> Projects { get; set; }

        public IEnumerable<Repository> Repositories { get; set; }

        private IServiceProvider _serviceProvider;

        public event Action SolutionChanged;

        public IServiceProvider ServiceProvider
        {
            get
            {
                return _serviceProvider;
            }
            set
            {
                _serviceProvider = value;

                IsCodeCloudProject = CheckIsCodeCloudProject();
            }
        }

        private bool CheckIsCodeCloudProject()
        {
            var repo = _tes.GetActiveRepository();
            if (repo == null || Projects == null)
            {
                return false;
            }

            var path = repo.Path;
            var url = _git.GetRemote(path);

            foreach (var project in Projects)
            {
                if (string.Equals(project.Url, url, StringComparison.OrdinalIgnoreCase))
                {
                    Current = project;
                    Branch = repo.Branch;

                    SolutionChanged?.Invoke();

                    return true;
                }
            }

            return false;
        }
    }
}
