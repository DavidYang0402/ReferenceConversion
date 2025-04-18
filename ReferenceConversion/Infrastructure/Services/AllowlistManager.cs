using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using ReferenceConversion.Data;
using ReferenceConversion.Domain.Interfaces;

namespace ReferenceConversion.Infrastructure.Services
{
    public class AllowlistManager : IAllowlistManager
    {
        private string _currentProjectName = string.Empty;
        private List<Project> _projectAllowlist = new();

        public void LoadProject()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "ReferenceConversion.Data.ProjectAllowList.json";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"找不到嵌入資源: {resourceName}");

            using var reader = new StreamReader(stream);
            string jsonContent = reader.ReadToEnd();

            var allowlistData = JsonConvert.DeserializeObject<AllowlistData>(jsonContent)
                ?? throw new InvalidOperationException("無法解析 Allowlist JSON 資料");

            _projectAllowlist = allowlistData.Projects ?? new List<Project>();
        }

        public void DisplayAllowlistForProject(string projectName, ListBox refList)
        {
            var project = FindProject(projectName);
            if (project == null) return;

            refList.Items.Clear();
            foreach (var item in project.Allowlist)
            {
                refList.Items.Add(item.Name);
            }
        }

        public bool IsInAllowlist(string referenceName, out Project? project, out Allowlist? entry)
        {
            project = FindProject(_currentProjectName);
            if (project is not null)
            {
                entry = project.Allowlist
                    .FirstOrDefault(a => a.Name.Equals(referenceName, StringComparison.OrdinalIgnoreCase));
                return entry is not null;
            }

            entry = null;
            return false;
        }

        public void SetCurrentProjectName(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("專案名稱為空", nameof(projectName));

            _currentProjectName = projectName.Trim();
        }

        public IEnumerable<string> GetProjectNames()
        {
            return _projectAllowlist.Select(p => p.ProjectName).ToList();
        }

        public string? GetProjectDllPath(string projectName)
        {
            return FindProject(projectName)?.DllPath;
        }

        public Project? FindProject(string projectName)
        {
            return _projectAllowlist.FirstOrDefault(p =>
                p.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
