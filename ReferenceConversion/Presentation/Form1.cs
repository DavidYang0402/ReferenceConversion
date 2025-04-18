using Newtonsoft.Json;
using ReferenceConversion.Domain.Enum;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Infrastructure.ConversionStrategies;
using ReferenceConversion.Infrastructure.Services;
using ReferenceConversion.Modifier;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Formatting = Newtonsoft.Json.Formatting;


namespace ReferenceConversion.Presentation
{
    public partial class Form1 : Form
    {
        private readonly IAllowlistManager _allowlistManager;
        private readonly IStrategyBasedConverter _referenceConverter;
        private readonly CsprojFileProcessor _csprojProcessor;

        public Form1(IStrategyBasedConverter referenceConverter, CsprojFileProcessor csprojProcessor, IAllowlistManager allowlistManager)
        {
            InitializeComponent();
            _referenceConverter = referenceConverter;
            _csprojProcessor = csprojProcessor;
            _allowlistManager = allowlistManager;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _allowlistManager.LoadProject();
            LoadProjectNamesToComboBox();
        }

        private void LoadProjectNamesToComboBox()
        {
            Cbx_Project_Allowlist.Items.Clear();
            var projectNames = _allowlistManager.GetProjectNames();

            foreach (var projectName in projectNames)
            {
                Cbx_Project_Allowlist.Items.Add(projectName);
            }

            if (Cbx_Project_Allowlist.Items.Count > 0)
            {
                Cbx_Project_Allowlist.SelectedIndex = 0;
            }
        }

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

        private void Cbx_Project_Allowlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Cbx_Project_Allowlist.SelectedItem != null)
            {
                string selectedProject = Cbx_Project_Allowlist.SelectedItem.ToString();
                _allowlistManager.SetCurrentProjectName(selectedProject);
                _allowlistManager.DisplayAllowlistForProject(selectedProject, Lb_Allowlist);

                Tb_Ref_Path.Text = _allowlistManager.GetProjectDllPath(selectedProject);
            }
            else
            {
                 MessageBox.Show("請選擇一個專案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Cbx_solution_file_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Cbx_solution_file.SelectedItem is FileInfo selectedFile)
            {
                string? slnDir = selectedFile.DirectoryName;
                LoadCsprojFiles(slnDir);
            }
        }


        private void LoadCsprojFiles(string? folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                MessageBox.Show("資料夾路徑為空或無效。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("指定的資料夾路徑不存在。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var csprojFiles = Directory.EnumerateFiles(folderPath, "*.csproj", SearchOption.AllDirectories);

            if (!csprojFiles.Any())
            {
                MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 清除之前的 ListBox 項目
            Lb_ShowAllCsproj.Items.Clear();

            // 將所有 .csproj 檔案加入 ListBox
            foreach (var file in csprojFiles)
            {
                string relativePath = Path.GetRelativePath(folderPath, file);
                Lb_ShowAllCsproj.Items.Add(relativePath);
            }            
        }

        private void ProcessCsprojFile(string csprojfile, ref bool hasChanges, string slnFilePath, ReferenceConversionMode mode)
        {
            bool isChanged = _csprojProcessor.ProcessFile(csprojfile, mode, slnFilePath);
            if (isChanged)
                hasChanges = true;
        }

        private void Btn_ToReference_Click(object sender, EventArgs e) => ExecuteConversion(ReferenceConversionMode.ProjectToDll);

        private void Btn_ToProjectReference_Click(object sender, EventArgs e) => ExecuteConversion(ReferenceConversionMode.DllToProject);

        private void ExecuteConversion(ReferenceConversionMode mode)
        {
            if (Cbx_solution_file.SelectedItem is FileInfo selected)
            {
                var slnPath = selected.FullName;
                var dir = Path.GetDirectoryName(slnPath)!;
                var csprojFiles = Directory.EnumerateFiles(dir, "*.csproj", SearchOption.AllDirectories);
                if (!csprojFiles.Any())
                {
                    MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                bool hasChanges = false;
                foreach (var file in csprojFiles)
                    ProcessCsprojFile(file, ref hasChanges, slnPath, mode);

                var msg = hasChanges
                    ? "轉換完成。"
                    : "沒有檔案需要轉換。";
                MessageBox.Show(msg, "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("請先選擇 .sln 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
