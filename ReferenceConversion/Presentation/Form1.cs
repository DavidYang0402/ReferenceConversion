using Newtonsoft.Json;
using ReferenceConversion.Data;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Infrastructure.ConversionStrategies;
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
        private AllowlistManager allowlistManager;
        private readonly IReferenceConverter _referenceConverter;

        public Form1(IReferenceConverter referenceConverter)
        {
            InitializeComponent();
            allowlistManager = new AllowlistManager();
            Func<string, ISlnModifier> slnModifierFactory = slnPath => new SlnModifier(slnPath);
            _referenceConverter = referenceConverter;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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

        //ComboBox SelectedIndexChanged
        private void Cbx_Project_Allowlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Cbx_Project_Allowlist.SelectedItem != null)
            {
                string selectedProject = Cbx_Project_Allowlist.SelectedItem.ToString();
                allowlistManager.SetCurrentProjectName(selectedProject);
                allowlistManager.DisplayAllowlistForProject(selectedProject, Lb_Allowlist);

                Tb_Ref_Path.Text = allowlistManager.GetProjectDllPath(selectedProject);
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
                Tb_Ref_Path.Clear();

                string? slnDir = selectedFile.DirectoryName;
                LoadCsprojFiles(slnDir);
            }
        }

        //Load Csproj Files
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

        //Process Csproj File
        private void ProcessCsprojFile(string csprojfile, ref bool hasChanges, string slnFilePath, ConversionType conversionType)
        {
            CsprojFileProcessor processor = new CsprojFileProcessor(_referenceConverter);
            bool isChanged = processor.ProcessFile(csprojfile, conversionType, slnFilePath);

            // 儲存變更
            if (isChanged)
            {
                hasChanges = true;         
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
    }
}
