using Newtonsoft.Json;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;


namespace ReferenceConversion
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
            if(string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show("請選擇有效的資料夾路徑。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 讀取資料夾中的所有 .csproj 檔案
            string[] csprojFiles = Directory.GetFiles(folderPath, "*.csproj");

            if(csprojFiles.Length == 0)
            {
                MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach(string csprojfile in csprojFiles)
            {
                ProcessCsprojFile(csprojfile);
            }

            MessageBox.Show("轉換完成。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        //Process Csproj File
        private void ProcessCsprojFile(string csprojfile)
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
                isChanged |= ConvertProjectReferenceToReference(xmlDoc, processedReferences);

                // 轉換 Reference → ProjectReference
                isChanged |= ConvertReferenceToProjectReference(xmlDoc, processedReferences);

                // 儲存變更
                if (isChanged)
                {
                    xmlDoc.Save(csprojfile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"處理檔案時出現錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Convert ProjectReference to Reference
        private bool ConvertProjectReferenceToReference(XmlDocument xmlDoc, HashSet<string> processedReferences)
        {
            bool isChanged = false;
            XmlNodeList projectReferences = xmlDoc.GetElementsByTagName("ProjectReference");
            List<XmlNode> nodesToRemove = new List<XmlNode>();

            if (projectReferences.Count > 0)
            {
                foreach (XmlNode node in projectReferences)
                {
                    string referenceName = Path.GetFileNameWithoutExtension(node.Attributes["Include"].Value);

                    // 若已處理過此項目則跳過
                    if (processedReferences.Contains(referenceName)) continue;

                    if (WhitelistManager.IsInAllowlist(referenceName, out string version, out string guid))
                    {
                        XmlElement reference = xmlDoc.CreateElement("Reference");
                        reference.SetAttribute("Include", $"{referenceName}, Version={version}, Culture=neutral, processorArchitecture=MSIL");

                        XmlElement specificVersion = xmlDoc.CreateElement("SpecificVersion");
                        specificVersion.InnerText = "False";
                        reference.AppendChild(specificVersion);

                        XmlElement hintPath = xmlDoc.CreateElement("HintPath");
                        hintPath.InnerText = Path.Combine("App_Data", $"{referenceName}.dll");
                        reference.AppendChild(hintPath);

                        node.ParentNode.AppendChild(reference);
                        nodesToRemove.Add(node);
                        processedReferences.Add(referenceName);  // 標記為已處理
                        isChanged = true;
                    }
                }
            }

            // 刪除 ProjectReference 節點
            foreach (XmlNode node in nodesToRemove)
            {
                node.ParentNode.RemoveChild(node);
            }

            return isChanged;
        }
        // Convert Reference to ProjectReference
        private bool ConvertReferenceToProjectReference(XmlDocument xmlDoc, HashSet<string> processedReferences)
        {
            bool isChanged = false;
            XmlNodeList references = xmlDoc.GetElementsByTagName("Reference");
            List<XmlNode> nodesToRemove = new List<XmlNode>();

            if (references.Count > 0)
            {
                foreach (XmlNode node in references)
                {
                    string referenceAttr = node.Attributes["Include"]?.Value;
                    if (string.IsNullOrEmpty(referenceAttr)) continue;

                    string referenceName = referenceAttr.Split(',')[0];

                    // 若已處理過此項目則跳過
                    if (processedReferences.Contains(referenceName)) continue;

                    if (WhitelistManager.IsInAllowlist(referenceName, out _, out string projectGuid))
                    {
                        string baseFolder = Tb_SaveFolder.Text.Trim();
                        if (string.IsNullOrEmpty(baseFolder))
                        {
                            MessageBox.Show("請輸入專案基底資料夾，例如 SysTools");
                            return false;
                        }

                        //string relativePath = $@"..\..\..\{baseFolder}\{referenceName}\{referenceName}.csproj";
                        // 計算新的 Include 屬性
                        string relativePath = Path.Combine("..", "..", "..", baseFolder, referenceName, $"{referenceName}.csproj");

                        // 檢查路徑是否包含 "Share"，如果包含則修改為 ShareFunc 子資料夾
                        if (relativePath.Contains("Share"))
                        {
                            relativePath = Path.Combine("..", "..", "..", baseFolder, "ShareFunc\\ShareFunc\\", referenceName, $"{referenceName}.csproj");
                        }

                        XmlElement projectReference = xmlDoc.CreateElement("ProjectReference");
                        projectReference.SetAttribute("Include", relativePath);

                        XmlElement projectGuidElement = xmlDoc.CreateElement("Project");
                        projectGuidElement.InnerText = projectGuid;
                        projectReference.AppendChild(projectGuidElement);

                        XmlElement nameElement = xmlDoc.CreateElement("Name");
                        nameElement.InnerText = referenceName;
                        projectReference.AppendChild(nameElement);

                        node.ParentNode.AppendChild(projectReference);
                        nodesToRemove.Add(node);
                        processedReferences.Add(referenceName);  // 標記為已處理
                        isChanged = true;
                    }
                }
            }

            // 刪除 Reference 節點
            foreach (XmlNode node in nodesToRemove)
            {
                node.ParentNode.RemoveChild(node);
            }

            return isChanged;
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
