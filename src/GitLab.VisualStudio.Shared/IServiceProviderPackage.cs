using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLab.VisualStudio.Shared
{
    public interface IServiceProviderPackage : IServiceProvider, Microsoft.VisualStudio.Shell.IAsyncServiceProvider
    {
    }
}
