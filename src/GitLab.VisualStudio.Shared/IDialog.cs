using System;

namespace GitLab.VisualStudio.Shared
{
    public interface IDialog
    {
        void Close();

        void Information(string message);

        void Warning(string message);

        void Error(string message);

        void Confirm(string message, Action<bool?> callback);
    }
}