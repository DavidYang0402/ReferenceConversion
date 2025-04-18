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
                 MessageBox.Show("�п�ܤ@�ӱM�סC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("��Ƨ����S����� .csproj �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                bool hasChanges = false;
                foreach (var file in csprojFiles)
                    ProcessCsprojFile(file, ref hasChanges, slnPath, mode);

                var msg = hasChanges
                    ? "�ഫ�����C"
                    : "�S���ɮ׻ݭn�ഫ�C";
                MessageBox.Show(msg, "�T��", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("�Х���� .sln �ɮסC", "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
