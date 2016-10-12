using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCloud.VisualStudio.Shared
{
    public interface IVisualStudioService
    {
        IServiceProvider ServiceProvider { get; set; }
        string GetActiveRepository();

        string GetSolutionPath();
    }
}
