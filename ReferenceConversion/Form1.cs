using Newtonsoft.Json;
using ReferenceConversion.Data;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Formatting = Newtonsoft.Json.Formatting;


namespace ReferenceConversion
{
    public partial class Form1 : Form
    {

        private AllowlistManager allowlistManager;
        private ReferenceConverter converter;
        private string slnGuid;

        public Form1()
        {
            InitializeComponent();
            allowlistManager = new AllowlistManager();
            converter = new ReferenceConverter(allowlistManager);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Tb_SaveFolder.Text = "SysTools";
            allowlistManager.LoadProject();
            LoadProjectNamesToComboBox();

        }

        //Load Project Names to ComboBox
        private void LoadProjectNamesToComboBox()
        {
            Cbx_Project_Allowlist.Items.Clear();
            var projectNames = allowlistManager.GetProjectNames();
            foreach (var projectName in projectNames)
            {
                Cbx_Project_Allowlist.Items.Add(projectName);
            }
            if (Cbx_Project_Allowlist.Items.Count > 0)
            {
                Cbx_Project_Allowlist.SelectedIndex = 0;
            }
        }

        //Get Folder
        private void Btn_GetFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // 設定初始路徑（可以選擇記住使用者上次選擇的資料夾）
                folderDialog.SelectedPath = Tb_ShowPath.Text;

                // 顯示資料夾選擇視窗
                DialogResult result = folderDialog.ShowDialog();

                // 如果使用者選擇了資料夾
                if (result == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    Tb_ShowPath.Text = folderPath;  // 顯示選擇的資料夾路徑

                    // 呼叫方法來載入資料夾中的 .csproj 檔案
                    LoadSolutionToComboBox(folderPath);
                }
            }
        }

        private void LoadSolutionToComboBox(string directory)
        {
            var slnPath = FindSolutionFilesInDirectoryAndSubfolders(directory);
            var fileInfos = slnPath.Select(path => new FileInfo(path)).ToList();

            Cbx_solution_file.DataSource = fileInfos;
            Cbx_solution_file.DisplayMember = "Name";
            Cbx_solution_file.ValueMember = "FullName";

            //MessageBox.Show($"找到 {fileInfos.Count} 個解決方案檔案。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //MessageBox.Show($"{Cbx_solution_file.DataSource}\n{Cbx_solution_file.DisplayMember}\n{Cbx_solution_file.ValueMember}");

            if (fileInfos.Count == 1)
            {
                Cbx_solution_file.SelectedIndex = 0;
            }
        }

        private static List<string> FindSolutionFilesInDirectoryAndSubfolders(string directory)
        {
            try
            {
                return Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"讀取資料夾時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }
        }

        //ComboBox SelectedIndexChanged
        private void Cbx_Project_Allowlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedProject = Cbx_Project_Allowlist.SelectedItem.ToString();
            allowlistManager.SetCurrentProjectName(selectedProject);
            allowlistManager.DisplayAllowlistForProject(selectedProject, Lb_Allowlist);

            string projectGuid = allowlistManager.GetProjectGuid(selectedProject);

            //MessageBox.Show(projectGuid);

            if (!string.IsNullOrEmpty(projectGuid))
            {
                // 設置 slnGuid 為 projectGuid
                slnGuid = projectGuid;
            }
            else
            {
                MessageBox.Show("找不到對應的專案 GUID。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Cbx_solution_file_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Cbx_solution_file.SelectedItem is FileInfo selectedFile)
            {
                Tb_Ref_Path.Clear();

                var slnDir = selectedFile.DirectoryName;
                LoadCsprojFiles(slnDir);
            }
        }

        //Load Csproj Files
        private void LoadCsprojFiles(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                // 讀取資料夾中的所有 .csproj 檔案
                var csprojFiles = Directory.EnumerateFiles(folderPath, "*.csproj", SearchOption.AllDirectories);

                if (csprojFiles.Any())
                {
                    // 清除之前的 ListBox 項目
                    Lb_ShowAllCsproj.Items.Clear();

                    // 將所有 .csproj 檔案加入 ListBox
                    foreach (var file in csprojFiles)
                    {
                        string relativePath = Path.GetRelativePath(folderPath, file);
                        Lb_ShowAllCsproj.Items.Add(relativePath);
                    }
                }
                else
                {
                    MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("指定的資料夾路徑無效。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Process Csproj File
        private void ProcessCsprojFile(string csprojfile, ref bool hasChanges, string slnFilePath, ConversionType conversionType)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(csprojfile);

                bool isChanged = false;

                // 使用 HashSet 追蹤已處理過的項目
                HashSet<string> processedReferences = new HashSet<string>();

                switch (conversionType)
                {
                    case ConversionType.ToReference:
                        //MessageBox.Show($"處理中 2 :{csprojfile}");
                        string? dllFolderPath = string.IsNullOrWhiteSpace(Tb_Ref_Path.Text) ? null : Tb_Ref_Path.Text;
                        isChanged |= converter.ConvertProjectReferenceToReference(xmlDoc, processedReferences, slnFilePath, slnGuid, dllFolderPath);
                        break;

                    case ConversionType.ToProjectReference:
                        // 轉換 Reference → ProjectReference
                        isChanged |= converter.ConvertReferenceToProjectReference(xmlDoc, processedReferences, slnFilePath, slnGuid);
                        break;
                }

                // 儲存變更
                if (isChanged)
                {
                    xmlDoc.Save(csprojfile);
                    hasChanges = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"處理檔案時出現錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public enum ConversionType
        {
            ToReference,
            ToProjectReference
        }

        private void Btn_ToReference_Click(object sender, EventArgs e)
        {
            // 從 ComboBox 選取的 .sln 檔案取得路徑
            if (Cbx_solution_file.SelectedItem is FileInfo selectedSlnFile)
            {
                string slnFilePath = selectedSlnFile.FullName;
                string slnDirectory = Path.GetDirectoryName(slnFilePath)!;

                var csprojFiles = Directory.EnumerateFiles(slnDirectory, "*.csproj", SearchOption.AllDirectories);
                if (!csprojFiles.Any())
                {
                    MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool hasChanges = false;

                foreach (string csprojfile in csprojFiles)
                {
                    ProcessCsprojFile(csprojfile, ref hasChanges, slnFilePath, ConversionType.ToReference);
                }

                if (hasChanges)
                {
                    MessageBox.Show("轉換為 ProjectReference 完成。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("沒有檔案需要轉換為 ProjectReference。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("請先選擇一個 .sln 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Btn_ToProjectReference_Click(object sender, EventArgs e)
        {
            if (Cbx_solution_file.SelectedItem is FileInfo selectedSlnFile)
            {
                string slnFilePath = selectedSlnFile.FullName;
                string slnDirectory = Path.GetDirectoryName(slnFilePath)!;

                var csprojFiles = Directory.EnumerateFiles(slnDirectory, "*.csproj", SearchOption.AllDirectories);
                if (!csprojFiles.Any())
                {
                    MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool hasChanges = false;

                foreach (string csprojfile in csprojFiles)
                {
                    ProcessCsprojFile(csprojfile, ref hasChanges, slnFilePath, ConversionType.ToProjectReference);
                }

                if (hasChanges)
                {
                    MessageBox.Show("轉換為 ProjectReference 完成。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("沒有檔案需要轉換為 ProjectReference。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("請先選擇一個 .sln 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Bt_Dll_Rel_Path_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // 顯示資料夾選擇視窗
                DialogResult result = folderDialog.ShowDialog();

                // 如果使用者選擇了資料夾
                if (result == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    Tb_Ref_Path.Text = folderPath;  // 顯示選擇的資料夾路徑
                }
            }
        }

        private void Bt_Clean_DllRef_Path_Click(object sender, EventArgs e)
        {
            Tb_Ref_Path.Clear();
        }
    }
}
