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

            //MessageBox.Show($"��� {fileInfos.Count} �ӸѨM����ɮסC", "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Ū����Ƨ��ɵo�Ϳ��~�G{ex.Message}", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // �]�m slnGuid �� projectGuid
                slnGuid = projectGuid;
            }
            else
            {
                MessageBox.Show("�䤣��������M�� GUID�C", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // Ū����Ƨ������Ҧ� .csproj �ɮ�
                var csprojFiles = Directory.EnumerateFiles(folderPath, "*.csproj", SearchOption.AllDirectories);

                if (csprojFiles.Any())
                {
                    // �M�����e�� ListBox ����
                    Lb_ShowAllCsproj.Items.Clear();

                    // �N�Ҧ� .csproj �ɮץ[�J ListBox
                    foreach (var file in csprojFiles)
                    {
                        string relativePath = Path.GetRelativePath(folderPath, file);
                        Lb_ShowAllCsproj.Items.Add(relativePath);
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

        //Process Csproj File
        private void ProcessCsprojFile(string csprojfile, ref bool hasChanges, string slnFilePath, ConversionType conversionType)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(csprojfile);

                bool isChanged = false;

                // �ϥ� HashSet �l�ܤw�B�z�L������
                HashSet<string> processedReferences = new HashSet<string>();

                switch (conversionType)
                {
                    case ConversionType.ToReference:
                        //MessageBox.Show($"�B�z�� 2 :{csprojfile}");
                        string? dllFolderPath = string.IsNullOrWhiteSpace(Tb_Ref_Path.Text) ? null : Tb_Ref_Path.Text;
                        isChanged |= converter.ConvertProjectReferenceToReference(xmlDoc, processedReferences, slnFilePath, slnGuid, dllFolderPath);
                        break;

                    case ConversionType.ToProjectReference:
                        // �ഫ Reference �� ProjectReference
                        isChanged |= converter.ConvertReferenceToProjectReference(xmlDoc, processedReferences, slnFilePath, slnGuid);
                        break;
                }

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

        public enum ConversionType
        {
            ToReference,
            ToProjectReference
        }

        private void Btn_ToReference_Click(object sender, EventArgs e)
        {
            // �q ComboBox ����� .sln �ɮר��o���|
            if (Cbx_solution_file.SelectedItem is FileInfo selectedSlnFile)
            {
                string slnFilePath = selectedSlnFile.FullName;
                string slnDirectory = Path.GetDirectoryName(slnFilePath)!;

                var csprojFiles = Directory.EnumerateFiles(slnDirectory, "*.csproj", SearchOption.AllDirectories);
                if (!csprojFiles.Any())
                {
                    MessageBox.Show("��Ƨ����S����� .csproj �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool hasChanges = false;

                foreach (string csprojfile in csprojFiles)
                {
                    ProcessCsprojFile(csprojfile, ref hasChanges, slnFilePath, ConversionType.ToReference);
                }

                if (hasChanges)
                {
                    MessageBox.Show("�ഫ�� ProjectReference �����C", "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("�S���ɮ׻ݭn�ഫ�� ProjectReference�C", "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("�Х���ܤ@�� .sln �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("��Ƨ����S����� .csproj �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool hasChanges = false;

                foreach (string csprojfile in csprojFiles)
                {
                    ProcessCsprojFile(csprojfile, ref hasChanges, slnFilePath, ConversionType.ToProjectReference);
                }

                if (hasChanges)
                {
                    MessageBox.Show("�ഫ�� ProjectReference �����C", "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("�S���ɮ׻ݭn�ഫ�� ProjectReference�C", "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("�Х���ܤ@�� .sln �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Bt_Dll_Rel_Path_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // ��ܸ�Ƨ���ܵ���
                DialogResult result = folderDialog.ShowDialog();

                // �p�G�ϥΪ̿�ܤF��Ƨ�
                if (result == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    Tb_Ref_Path.Text = folderPath;  // ��ܿ�ܪ���Ƨ����|
                }
            }
        }

        private void Bt_Clean_DllRef_Path_Click(object sender, EventArgs e)
        {
            Tb_Ref_Path.Clear();
        }
    }
}
