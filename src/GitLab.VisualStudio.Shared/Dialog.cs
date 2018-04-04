using System;
using System.Windows;
using System.Windows.Controls;

namespace GitLab.VisualStudio.Shared
{
    public class Dialog : ContentControl, IDialog
    {
        public event Action Closed;

        public void Close()
        {
            Closed?.Invoke();
        }

        public void Confirm(string message, Action<bool?> callback)
        {
            MessageBox.Show(message, "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public void Error(string message)
        {
            MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void Information(string message)
        {
            MessageBox.Show(message, "消息", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Warning(string message)
        {
            MessageBox.Show(message, "报警", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}