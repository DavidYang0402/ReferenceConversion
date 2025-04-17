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
                // �]�w��l���|�]�i�H��ܰO��ϥΪ̤W����ܪ���Ƨ��^
                folderDialog.SelectedPath = Tb_ShowPath.Text;

                // ��ܸ�Ƨ���ܵ���
                DialogResult result = folderDialog.ShowDialog();

                // �p�G�ϥΪ̿�ܤF��Ƨ�
                if (result == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    Tb_ShowPath.Text = folderPath;  // ��ܿ�ܪ���Ƨ����|

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
                MessageBox.Show($"Ū����Ƨ��ɵo�Ϳ��~�G{ex.Message}", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                 MessageBox.Show("�п�ܤ@�ӱM�סC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("��Ƨ����|���ũεL�ġC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("���w����Ƨ����|���s�b�C", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var csprojFiles = Directory.EnumerateFiles(folderPath, "*.csproj", SearchOption.AllDirectories);

            if (!csprojFiles.Any())
            {
                MessageBox.Show("��Ƨ����S����� .csproj �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // �M�����e�� ListBox ����
            Lb_ShowAllCsproj.Items.Clear();

            // �N�Ҧ� .csproj �ɮץ[�J ListBox
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

            // �x�s�ܧ�
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
    }
}
