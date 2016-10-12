using CodeCloud.VisualStudio.Shared.Controls;
using System.Collections.Generic;

namespace CodeCloud.VisualStudio.Shared
{
    public class Repository
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsActived { get; set; }
        public Octicon Icon { get; set; }
    }
    public interface IRegistry
    {
        IReadOnlyList<Repository> GetKnownRepositories();
    }
}
