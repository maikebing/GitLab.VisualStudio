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

        public IReadOnlyList<Project> Projects { get; set; }

        public IEnumerable<Repository> Repositories { get; set; }
    }
}
