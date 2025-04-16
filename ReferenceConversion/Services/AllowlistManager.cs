using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReferenceConversion.Data
{
    public class AllowlistManager
    {
        private string curProjectName;
        private List<Project> projectAllowlist;
        

        public AllowlistManager()
        {
            projectAllowlist = new List<Project>();
            curProjectName = string.Empty;
        }

        public void LoadProject()
        {
            // 取得應用程式的命名空間和嵌入資源名稱
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ReferenceConversion.Data.ProjectAllowList.json";  // 假設嵌入的資源名稱為這個

            // 讀取嵌入的 JSON 檔案
            // 讀取嵌入的 JSON 檔案
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                // 檢查 stream 是否為 null
                if (stream == null)
                {
                    throw new InvalidOperationException($"找不到嵌入資源: {resourceName}");
                }

                using (var reader = new StreamReader(stream))
                {
                    string jsonContent = reader.ReadToEnd();
                    var allowlistData = JsonConvert.DeserializeObject<AllowlistData>(jsonContent);
                    projectAllowlist = allowlistData.Projects;
                }
            }
        }

        public void DisplayAllowlistForProject(string projectName, ListBox refList)
        {
            var project = projectAllowlist.FirstOrDefault(p => p.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase));

            if (project != null)
            {
                refList.Items.Clear();
                foreach (var item in project.Allowlist)
                {
                    refList.Items.Add(item.Name);
                }
            }
        }
        // 判斷是否在 Allowlist 中
        public bool IsInAllowlist(string referenceName, out Project? project, out Allowlist? entry)
        {
            project = projectAllowlist.FirstOrDefault(p => p.ProjectName.Equals(curProjectName, StringComparison.OrdinalIgnoreCase));
            if (project == null)
            {
                entry = null;
                return false;
            }

            entry = project.Allowlist.FirstOrDefault(a => a.Name.Equals(referenceName, StringComparison.OrdinalIgnoreCase));
            return entry != null;
        }


        // 設定當前選擇的專案名稱
        public void SetCurrentProjectName(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentException("專案名稱為空", nameof(projectName));
            }

            curProjectName = projectName;
        }

        // 取得專案名稱清單
        public List<string> GetProjectNames()
        {
            return projectAllowlist.Select(p => p.ProjectName).ToList();
        }

        public string? GetProjectDllPath(string projectName)
        {
            var project = projectAllowlist.FirstOrDefault(p => p.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase));
            return project?.DllPath;
        }

    }
}
