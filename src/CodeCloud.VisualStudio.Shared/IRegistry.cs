using CodeCloud.VisualStudio.Shared.Controls;
using CodeCloud.VisualStudio.Shared.Helpers;
using System.Collections.Generic;

namespace CodeCloud.VisualStudio.Shared
{
    public class Repository : Bindable
    {
        public string Name { get; set; }
        public string Path { get; set; }

        private bool _isActived;
        public bool IsActived
        {
            get { return _isActived; }
            set { SetProperty(ref _isActived, value); }
        }

        public Octicon Icon { get; set; }
    }
    public interface IRegistry
    {
        IReadOnlyList<Repository> GetKnownRepositories();
    }
}
