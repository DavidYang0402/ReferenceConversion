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
                // �]�w��l���|�]�i�H��ܰO��ϥΪ̤W����ܪ���Ƨ��^
                folderDialog.SelectedPath = Tb_ShowPath.Text;

                // ��ܸ�Ƨ���ܵ���
                DialogResult result = folderDialog.ShowDialog();

                // �p�G�ϥΪ̿�ܤF��Ƨ�
                if (result == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    Tb_ShowPath.Text = folderPath;  // ��ܿ�ܪ���Ƨ����|

                    // �I�s��k�Ӹ��J��Ƨ����� .csproj �ɮ�
                    LoadCsprojFiles(folderPath);
                }
            }
        }
        private void LoadCsprojFiles(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                // Ū����Ƨ������Ҧ� .csproj �ɮ�
                var csprojFiles = Directory.GetFiles(folderPath, "*.csproj");

                if (csprojFiles.Length > 0)
                {
                    // �M�����e�� ListBox ����
                    Lb_ShowAllCsproj.Items.Clear();

                    // �N�Ҧ� .csproj �ɮץ[�J ListBox
                    foreach (var file in csprojFiles)
                    {
                        Lb_ShowAllCsproj.Items.Add(Path.GetFileName(file));
                    }
                }
                else
                {
                    MessageBox.Show("��Ƨ����S����� .csproj �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("���w����Ƨ����|�L�ġC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("�п�ܦ��Ī���Ƨ����|�C", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ū����Ƨ������Ҧ� .csproj �ɮ�
            string[] csprojFiles = Directory.GetFiles(folderPath, "*.csproj");

            if(csprojFiles.Length == 0)
            {
                MessageBox.Show("��Ƨ����S����� .csproj �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach(string csprojfile in csprojFiles)
            {
                ProcessCsprojFile(csprojfile);
            }

            MessageBox.Show("�ഫ�����C", "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ProcessCsprojFile(string csprojfile)
        {

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(csprojfile);

                bool isChanged = false;

                // �M�z ToolsVersion �M Sdk �ݩ�
                CleanToolsVersionAndSdk(xmlDoc);

                // �ഫ ProjectReference �� Reference
                XmlNodeList projectReferences = xmlDoc.GetElementsByTagName("ProjectReference");
                XmlNodeList references = xmlDoc.GetElementsByTagName("Reference");

                // �����ݭn�R���� ProjectReference �`�I
                List<XmlNode> nodesToRemove = new List<XmlNode>();

                if (projectReferences.Count > 0)
                {
                    foreach (XmlNode node in projectReferences)
                    {
                        string referenceName = Path.GetFileNameWithoutExtension(node.Attributes["Include"].Value);
                        Console.WriteLine($"�B�z ProjectReference: {referenceName}");

                        // �ˬd�զW�椤�O�_���ӰѦҦW��
                        if (IsInAllowlist(referenceName, out string version))
                        {
                            // �Ы� Reference �`�I�A�ó]�m����
                            XmlElement reference = xmlDoc.CreateElement("Reference");
                            reference.SetAttribute("Include", $"{referenceName}, Version={version}, Culture=neutral, processorArchitecture=MSIL");

                            XmlElement specificVersion = xmlDoc.CreateElement("SpecificVersion");
                            specificVersion.InnerText = "False";
                            reference.AppendChild(specificVersion);

                            XmlElement hintPath = xmlDoc.CreateElement("HintPath");
                            string hintPathValue = Path.Combine("App_Data", $"{referenceName}.dll");
                            hintPath.InnerText = hintPathValue;
                            reference.AppendChild(hintPath);

                            // ��s�Ыت� Reference �`�I�[����`�I
                            node.ParentNode.AppendChild(reference);
                            nodesToRemove.Add(node);  // �O���ݭn�R���� ProjectReference �`�I
                            isChanged = true;
                        }
                    }

                    // �R�������쪺 ProjectReference �`�I
                    foreach (XmlNode node in nodesToRemove)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }

                if (isChanged)
                {
                    try
                    {
                        // �O�s�ഫ�᪺ .csproj �ɮ�
                        xmlDoc.Save(csprojfile);
                        Console.WriteLine($"�ɮפw�O�s: {csprojfile}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"�O�s�ɮ׮ɥX�{���~: {ex.Message}", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�B�z�ɮ׮ɥX�{���~: {ex.Message}", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsInAllowlist(string referenceName, out string version)
        {
            version = string.Empty;
            string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            string allowlistPath = Path.Combine(projectRoot, "Data", "AllowList.json");

            Console.WriteLine($"AllowList.json ���|: {allowlistPath}");

            if (!File.Exists(allowlistPath))
            {
                throw new FileNotFoundException($"�䤣�� AllowList.json�A�нT�{���|�O�_���T: {allowlistPath}");
            }

            string json = File.ReadAllText(allowlistPath);
            dynamic allowlist = JsonConvert.DeserializeObject(json);

            // �u�ˬd name �O�_�b�զW�椤�A�æ^�ǹ����� version
            foreach (var item in allowlist.whitelist)
            {
                if (item.name == referenceName)
                {
                    version = item.version;
                    return true;
                }
            }

            // �p�G�զW�椤�S���������W�١A��^ false
            return false;
        }

        private void CleanToolsVersionAndSdk(XmlDocument xmlDoc)
        {
            // ���� ToolsVersion �ݩ�
            XmlAttribute toolsVersionAttribute = xmlDoc.SelectSingleNode("//Project/@ToolsVersion") as XmlAttribute;
            if (toolsVersionAttribute != null)
            {
                toolsVersionAttribute.OwnerElement.Attributes.Remove(toolsVersionAttribute);
            }

            // ���� Sdk �ݩ�
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
