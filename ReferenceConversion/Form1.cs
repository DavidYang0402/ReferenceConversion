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

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(csprojfile);

            bool isChanged = false;

            XmlNodeList projectreferences = xmlDoc.GetElementsByTagName("ProjectReference");
            XmlNodeList references = xmlDoc.GetElementsByTagName("Reference");

        }
    }
}
