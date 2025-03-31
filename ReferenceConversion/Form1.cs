using Newtonsoft.Json;
using ReferenceConversion.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;


namespace ReferenceConversion
{
    public partial class Form1 : Form
    {

        private AllowlistManager allowlistManager;
        private ReferenceConverter converter;

        public Form1()
        {
            InitializeComponent();
            allowlistManager = new AllowlistManager();
            converter = new ReferenceConverter(allowlistManager);
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            Tb_SaveFolder.Text = "SysTools";
            allowlistManager.LoadProject();
            LoadProjectNamesToComboBox();

        }

        private void LoadProjectNamesToComboBox()
        {
            Cbx_Project_Allowlist.Items.Clear();
            var projectNames = allowlistManager.GetProjectNames();
            foreach (var projectName in projectNames)
            {
                Cbx_Project_Allowlist.Items.Add(projectName);
            }
            if(Cbx_Project_Allowlist.Items.Count > 0)
            {
                Cbx_Project_Allowlist.SelectedIndex = 0;
            }
        }

        private void Cbx_Project_Allowlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedProject = Cbx_Project_Allowlist.SelectedItem.ToString();
            allowlistManager.SetCurrentProjectName(selectedProject);
            allowlistManager.DisplayAllowlistForProject(selectedProject, Lb_Allowlist);
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
                    LoadCsprojFiles(folderPath);
                }
            }
        }
        //Load Csproj Files
        private void LoadCsprojFiles(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                // 讀取資料夾中的所有 .csproj 檔案
                var csprojFiles = Directory.GetFiles(folderPath, "*.csproj");

                if (csprojFiles.Length > 0)
                {
                    // 清除之前的 ListBox 項目
                    Lb_ShowAllCsproj.Items.Clear();

                    // 將所有 .csproj 檔案加入 ListBox
                    foreach (var file in csprojFiles)
                    {
                        Lb_ShowAllCsproj.Items.Add(Path.GetFileName(file));
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


        //Reference Conversion
        private void Btn_Convert_Click(object sender, EventArgs e)
        {
            string folderPath = Tb_ShowPath.Text;
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show("請選擇有效的資料夾路徑。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 讀取資料夾中的所有 .csproj 檔案
            string[] csprojFiles = Directory.GetFiles(folderPath, "*.csproj");

            if (csprojFiles.Length == 0)
            {
                MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string baseFolder = Tb_SaveFolder.Text.Trim();
            if (string.IsNullOrEmpty(baseFolder))
            {
                MessageBox.Show("請輸入專案基底資料夾，例如 SysTools", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool hasChanges = false;

            foreach (string csprojfile in csprojFiles)
            {
                ProcessCsprojFile(csprojfile, ref hasChanges);
            }

            if (hasChanges)
            {
                MessageBox.Show("轉換完成。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("沒有檔案需要轉換。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        //Process Csproj File
        private void ProcessCsprojFile(string csprojfile, ref bool hasChanges)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(csprojfile);

                bool isChanged = false;

                // 清理 ToolsVersion 和 Sdk 屬性
                CleanToolsVersionAndSdk(xmlDoc);

                // 使用 HashSet 追蹤已處理過的項目
                HashSet<string> processedReferences = new HashSet<string>();

                // 轉換 ProjectReference → Reference
                isChanged |= converter.ConvertProjectReferenceToReference(xmlDoc, processedReferences);

                // 轉換 Reference → ProjectReference
                string saveFolder = Tb_SaveFolder.Text.Trim();
                isChanged |= converter.ConvertReferenceToProjectReference(xmlDoc, processedReferences, saveFolder);

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

        //Clean ToolsVersion and Sdk
        private void CleanToolsVersionAndSdk(XmlDocument xmlDoc)
        {
            // 移除 ToolsVersion 屬性
            XmlAttribute toolsVersionAttribute = xmlDoc.SelectSingleNode("//Project/@ToolsVersion") as XmlAttribute;
            if (toolsVersionAttribute != null)
            {
                toolsVersionAttribute.OwnerElement.Attributes.Remove(toolsVersionAttribute);
            }

            // 移除 Sdk 屬性
            XmlAttribute sdkAttribute = xmlDoc.SelectSingleNode("//Project/@Sdk") as XmlAttribute;
            if (sdkAttribute != null)
            {
                sdkAttribute.OwnerElement.Attributes.Remove(sdkAttribute);
            }
        }


    }
}
