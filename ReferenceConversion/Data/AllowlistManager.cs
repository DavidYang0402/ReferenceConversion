using System;
using System.Collections.Generic;
using System.Linq;
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
            // 取得應用程式根目錄 (發佈後的位置)
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // 構建 Data 資料夾中的 AllowList.json 路徑
            string jsonFilePath = Path.Combine(appDirectory, "Data", "ProjectAllowList.json");

            if (File.Exists(jsonFilePath))
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                var allowlistData = JsonConvert.DeserializeObject<AllowlistData>(jsonContent);

               projectAllowlist = allowlistData.Projects;
            }
            else
            {
                MessageBox.Show("找不到 ProjectAllowList.json 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        public bool IsInAllowlist(string referenceName, out string version, out string guid)
        {
            version = string.Empty;
            guid = string.Empty;

            var selectedProject = projectAllowlist.FirstOrDefault(p => p.ProjectName.Equals(curProjectName, StringComparison.OrdinalIgnoreCase));
            if (selectedProject != null)
            {
                var entry = selectedProject.Allowlist.FirstOrDefault(w => w.Name.Equals(referenceName, StringComparison.OrdinalIgnoreCase));
                if (entry != null)
                {
                    version = entry.Version;
                    guid = entry.Guid;
                    return true;
                }
            }

            return false;
        }

        // 設定當前選擇的專案名稱
        public void SetCurrentProjectName(string projectName)
        {
            curProjectName = projectName;
        }

        // 取得專案名稱清單
        public List<string> GetProjectNames()
        {
            return projectAllowlist.Select(p => p.ProjectName).ToList();
        }
    }
}
