using GitLab.VisualStudio.Shared.Controls;
using GitLab.VisualStudio.Shared.Helpers;

namespace GitLab.VisualStudio.Shared
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
}