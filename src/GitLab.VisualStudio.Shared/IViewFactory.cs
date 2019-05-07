using GitLab.VisualStudio.Shared.Models;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Controls;

namespace GitLab.VisualStudio.Shared
{
    public interface IViewFactory
    {
        T GetView<T>(ViewTypes type) where T : Control;
        CloneDialogResult ShowCloneDialog(IProgress<ServiceProgressData> downloadProgress);
    }
}