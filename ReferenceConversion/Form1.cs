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
        //Load Csproj Files
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


        //Reference Conversion
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
        //Process Csproj File
        private void ProcessCsprojFile(string csprojfile)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(csprojfile);

                bool isChanged = false;

                // �M�z ToolsVersion �M Sdk �ݩ�
                CleanToolsVersionAndSdk(xmlDoc);

                // �ϥ� HashSet �l�ܤw�B�z�L������
                HashSet<string> processedReferences = new HashSet<string>();

                // �ഫ ProjectReference �� Reference
                isChanged |= ConvertProjectReferenceToReference(xmlDoc, processedReferences);

                // �ഫ Reference �� ProjectReference
                isChanged |= ConvertReferenceToProjectReference(xmlDoc, processedReferences);

                // �x�s�ܧ�
                if (isChanged)
                {
                    xmlDoc.Save(csprojfile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�B�z�ɮ׮ɥX�{���~: {ex.Message}", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    // �Y�w�B�z�L�����ثh���L
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
                        processedReferences.Add(referenceName);  // �аO���w�B�z
                        isChanged = true;
                    }
                }
            }

            // �R�� ProjectReference �`�I
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

                    // �Y�w�B�z�L�����ثh���L
                    if (processedReferences.Contains(referenceName)) continue;

                    if (WhitelistManager.IsInAllowlist(referenceName, out _, out string projectGuid))
                    {
                        string baseFolder = Tb_SaveFolder.Text.Trim();
                        if (string.IsNullOrEmpty(baseFolder))
                        {
                            MessageBox.Show("�п�J�M�װ򩳸�Ƨ��A�Ҧp SysTools");
                            return false;
                        }

                        //string relativePath = $@"..\..\..\{baseFolder}\{referenceName}\{referenceName}.csproj";
                        // �p��s�� Include �ݩ�
                        string relativePath = Path.Combine("..", "..", "..", baseFolder, referenceName, $"{referenceName}.csproj");

                        // �ˬd���|�O�_�]�t "Share"�A�p�G�]�t�h�קאּ ShareFunc �l��Ƨ�
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
                        processedReferences.Add(referenceName);  // �аO���w�B�z
                        isChanged = true;
                    }
                }
            }

            // �R�� Reference �`�I
            foreach (XmlNode node in nodesToRemove)
            {
                node.ParentNode.RemoveChild(node);
            }

            return isChanged;
        }  
        //Clean ToolsVersion and Sdk
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
    }
}
