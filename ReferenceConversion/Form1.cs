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

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(csprojfile);

            bool isChanged = false;

            XmlNodeList projectreferences = xmlDoc.GetElementsByTagName("ProjectReference");
            XmlNodeList references = xmlDoc.GetElementsByTagName("Reference");

        }
    }
}
