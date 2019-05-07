using System.Windows.Controls;

namespace GitLab.VisualStudio.Shared
{
    public interface IViewFactory
    {
        T GetView<T>(ViewTypes type) where T : Control;
        void ShowCloneDialog(string name, string url);
    }
}