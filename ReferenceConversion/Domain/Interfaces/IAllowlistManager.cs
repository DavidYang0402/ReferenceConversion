using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReferenceConversion.Data;

namespace ReferenceConversion.Domain.Interfaces
{
    public interface IAllowlistManager
    {
        void LoadProject();
        void DisplayAllowlistForProject(string projectName, ListBox refList);
        bool IsInAllowlist(string referenceName, out Project? project, out Allowlist? entry);
        void SetCurrentProjectName(string projectName);
        IEnumerable<string> GetProjectNames();
        string? GetProjectDllPath(string projectName);
        Project? FindProject(string projectName);
    }
}
