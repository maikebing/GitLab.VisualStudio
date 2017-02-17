using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
