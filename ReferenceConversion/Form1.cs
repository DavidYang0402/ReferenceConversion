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

        private void Lb_ShowAllCsproj_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

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

        private void ProcessCsprojFile(string csprojfile)
        {

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(csprojfile);

                bool isChanged = false;

                // 清理 ToolsVersion 和 Sdk 屬性
                CleanToolsVersionAndSdk(xmlDoc);

                // 轉換 ProjectReference 為 Reference
                XmlNodeList projectReferences = xmlDoc.GetElementsByTagName("ProjectReference");
                XmlNodeList references = xmlDoc.GetElementsByTagName("Reference");

                // 收集需要刪除的 ProjectReference 節點
                List<XmlNode> nodesToRemove = new List<XmlNode>();

                if (projectReferences.Count > 0)
                {
                    foreach (XmlNode node in projectReferences)
                    {
                        string referenceName = Path.GetFileNameWithoutExtension(node.Attributes["Include"].Value);
                        Console.WriteLine($"處理 ProjectReference: {referenceName}");

                        // 檢查白名單中是否有該參考名稱
                        if (IsInAllowlist(referenceName, out string version))
                        {
                            // 創建 Reference 節點，並設置版本
                            XmlElement reference = xmlDoc.CreateElement("Reference");
                            reference.SetAttribute("Include", $"{referenceName}, Version={version}, Culture=neutral, processorArchitecture=MSIL");

                            XmlElement specificVersion = xmlDoc.CreateElement("SpecificVersion");
                            specificVersion.InnerText = "False";
                            reference.AppendChild(specificVersion);

                            XmlElement hintPath = xmlDoc.CreateElement("HintPath");
                            string hintPathValue = Path.Combine("App_Data", $"{referenceName}.dll");
                            hintPath.InnerText = hintPathValue;
                            reference.AppendChild(hintPath);

                            // 把新創建的 Reference 節點加到父節點
                            node.ParentNode.AppendChild(reference);
                            nodesToRemove.Add(node);  // 記錄需要刪除的 ProjectReference 節點
                            isChanged = true;
                        }
                    }

                    // 刪除收集到的 ProjectReference 節點
                    foreach (XmlNode node in nodesToRemove)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }

                if (isChanged)
                {
                    try
                    {
                        // 保存轉換後的 .csproj 檔案
                        xmlDoc.Save(csprojfile);
                        Console.WriteLine($"檔案已保存: {csprojfile}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存檔案時出現錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"處理檔案時出現錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsInAllowlist(string referenceName, out string version)
        {
            version = string.Empty;
            string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            string allowlistPath = Path.Combine(projectRoot, "Data", "AllowList.json");

            Console.WriteLine($"AllowList.json 路徑: {allowlistPath}");

            if (!File.Exists(allowlistPath))
            {
                throw new FileNotFoundException($"找不到 AllowList.json，請確認路徑是否正確: {allowlistPath}");
            }

            string json = File.ReadAllText(allowlistPath);
            dynamic allowlist = JsonConvert.DeserializeObject(json);

            // 只檢查 name 是否在白名單中，並回傳對應的 version
            foreach (var item in allowlist.whitelist)
            {
                if (item.name == referenceName)
                {
                    version = item.version;
                    return true;
                }
            }

            // 如果白名單中沒有找到對應名稱，返回 false
            return false;
        }

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

        private string FindCsprojFile(string baseDir, string referenceName)
        {
            string[] csprojFiles = Directory.GetFiles(baseDir, "*.csproj", SearchOption.AllDirectories);

            foreach (string csproj in csprojFiles)
            {
                if (Path.GetFileNameWithoutExtension(csproj) == referenceName)
                {
                    return csproj;
                }
            }

            return null;
        }
    }
}
