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
            MessageBox.Show(message, Strings.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public void Error(string message)
        {
            MessageBox.Show(message, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void Information(string message)
        {
            MessageBox.Show(message, Strings.Information, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Warning(string message)
        {
            MessageBox.Show(message, Strings.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}