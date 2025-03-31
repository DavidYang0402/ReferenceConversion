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
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show("�п�ܦ��Ī���Ƨ����|�C", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ū����Ƨ������Ҧ� .csproj �ɮ�
            string[] csprojFiles = Directory.GetFiles(folderPath, "*.csproj");

            if (csprojFiles.Length == 0)
            {
                MessageBox.Show("��Ƨ����S����� .csproj �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string baseFolder = Tb_SaveFolder.Text.Trim();
            if (string.IsNullOrEmpty(baseFolder))
            {
                MessageBox.Show("�п�J�M�װ򩳸�Ƨ��A�Ҧp SysTools", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool hasChanges = false;

            foreach (string csprojfile in csprojFiles)
            {
                ProcessCsprojFile(csprojfile, ref hasChanges);
            }

            if (hasChanges)
            {
                MessageBox.Show("�ഫ�����C", "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("�S���ɮ׻ݭn�ഫ�C", "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                // �M�z ToolsVersion �M Sdk �ݩ�
                CleanToolsVersionAndSdk(xmlDoc);

                // �ϥ� HashSet �l�ܤw�B�z�L������
                HashSet<string> processedReferences = new HashSet<string>();

                // �ഫ ProjectReference �� Reference
                isChanged |= converter.ConvertProjectReferenceToReference(xmlDoc, processedReferences);

                // �ഫ Reference �� ProjectReference
                string saveFolder = Tb_SaveFolder.Text.Trim();
                isChanged |= converter.ConvertReferenceToProjectReference(xmlDoc, processedReferences, saveFolder);

                // �x�s�ܧ�
                if (isChanged)
                {
                    xmlDoc.Save(csprojfile);
                    hasChanges = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�B�z�ɮ׮ɥX�{���~: {ex.Message}", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
