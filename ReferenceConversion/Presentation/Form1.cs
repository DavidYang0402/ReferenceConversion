using ReferenceConversion.Domain.Enum;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Infrastructure.ConversionStrategies;
using ReferenceConversion.Shared;


namespace ReferenceConversion.Presentation
{
    public partial class Form1 : Form
    {
        private readonly IAllowlistManager _allowlistManager;
        private readonly CsprojFileProcessor _csprojProcessor;

        public Form1(CsprojFileProcessor csprojProcessor, IAllowlistManager allowlistManager)
        {
            InitializeComponent();
            _csprojProcessor = csprojProcessor;
            _allowlistManager = allowlistManager;

            Pnl_Log.Visible = false;
            Btn_Clear_Log.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = 550;
            Lb_Log_Path.Text = 
                $"Log 檔存放路徑: {Path.Combine(@"C:\Reference_Covert_Logs", $"log_{DateTime.Now:yyyyMMdd}.txt")}";

            _allowlistManager.LoadProject();
            LoadProjectNamesToComboBox();

            Logger.LogToUI = msg =>
            {
                if (InvokeRequired)
                    Invoke(() => Lb_Log.Items.Add(msg));
                else
                    Lb_Log.Items.Add(msg);
            };

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
                // 設定初始路徑（可以選擇記住使用者上次選擇的資料夾）
                folderDialog.SelectedPath = Tb_ShowPath.Text;

                // 顯示資料夾選擇視窗
                DialogResult result = folderDialog.ShowDialog();

                // 如果使用者選擇了資料夾
                if (result == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    Tb_ShowPath.Text = folderPath;  // 顯示選擇的資料夾路徑

                    LoadSolutionToComboBox(folderPath);
                }
            }
        }

        private void LoadSolutionToComboBox(string baseDirectory)
        {
            string? trunkPath = null;

            // 情況1: 選擇的路徑本身就是trunk
            if (Path.GetFileName(baseDirectory).Equals("trunk", StringComparison.OrdinalIgnoreCase))
            {
                trunkPath = baseDirectory;
            }
            // 情況2: 在選擇的路徑下直接尋找trunk
            else
            {
                trunkPath = Directory.EnumerateDirectories(baseDirectory)
                                    .FirstOrDefault(d => Path.GetFileName(d).Equals("trunk", StringComparison.OrdinalIgnoreCase));

                // 情況3: 更深層次尋找trunk (例如在WorkProjects下找  XXX專案名/trunk)
                if (trunkPath == null)
                {
                    try
                    {
                        // 只深入搜尋一層子目錄，避免搜尋過多
                        foreach (var dir in Directory.EnumerateDirectories(baseDirectory))
                        {
                            var subTrunkDir = Directory.EnumerateDirectories(dir)
                                                    .FirstOrDefault(d => Path.GetFileName(d).Equals("trunk", StringComparison.OrdinalIgnoreCase));
                            if (subTrunkDir != null)
                            {
                                trunkPath = subTrunkDir;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogToUI?.Invoke($"搜尋子目錄時發生錯誤: {ex.Message}");
                    }
                }
            }

            if (string.IsNullOrEmpty(trunkPath))
            {
                MessageBox.Show("找不到 trunk 資料夾，請確認目錄結構是否正確。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cbx_solution_file.DataSource = null;
                return;
            }

            var slnPath = FindSolutionFilesInDirectoryAndSubfolders(trunkPath);
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
                MessageBox.Show($"讀取資料夾時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                LocateProjectPathAndShow(selectedProject);
                Tb_Ref_Path.Text = Path.Combine("..", _allowlistManager.GetProjectDllPath(selectedProject));

                //Set UI Properties
                Lb_Convert_Status.Text = "轉換狀態：無";
                Lb_Convert_Status.ForeColor = Color.Black;
                Lb_Log.Items.Clear();
            }
            else
            {
                MessageBox.Show("請選擇一個專案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        //取 WorkProjects: 自動尋找 WorkProjects，會依序從 C:\，搜尋所有曹
        public void LocateProjectPathAndShow(string projectName)
        {
            string? foundPath = null;

            //foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
            //{
            //    string possiblePath = Path.Combine(drive.RootDirectory.FullName, "WorkProjects", projectName);

            //    if (Directory.Exists(possiblePath))
            //    {
            //        foundPath = possiblePath;
            //        break;
            //    }
            //}

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
            {
                try
                {
                    string basePath = Path.Combine(drive.RootDirectory.FullName, "WorkProjects");
                    string exactPath = Path.Combine(basePath, projectName);

                    //精確匹配
                    if (Directory.Exists(exactPath))
                    {
                        foundPath = exactPath;
                        break;
                    }

                    //沒找到 => 開始模糊匹配
                    if (Directory.Exists(basePath))
                    {
                        var similarDirs = Directory.GetDirectories(basePath)
                            .Where(dir => Path.GetFileName(dir).Contains(projectName, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (similarDirs.Any())
                        {
                            foundPath = similarDirs.First(); // 你也可以列出所有結果讓使用者選
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 防止某些磁碟沒權限或爆炸
                    Console.WriteLine($"Error accessing {drive.Name}: {ex.Message}");
                }
            }

            if (foundPath != null)
            {
                Tb_ShowPath.Text = foundPath;
                LoadSolutionToComboBox(foundPath);
            }
            else
            {
                Tb_ShowPath.Text = "找不到 WorkProjects 專案";
            }
        }


        private void LoadCsprojFiles(string? folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                MessageBox.Show("資料夾路徑為空或無效。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("指定的資料夾路徑不存在。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var csprojFiles = Directory.EnumerateFiles(folderPath, "*.csproj", SearchOption.AllDirectories);

            if (!csprojFiles.Any())
            {
                MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 清除之前的 ListBox 項目
            Lb_ShowAllCsproj.Items.Clear();

            // 將所有 .csproj 檔案加入 ListBox
            foreach (var file in csprojFiles)
            {
                string fullPath = Path.GetFullPath(file);
                Lb_ShowAllCsproj.Items.Add(fullPath);
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
            try
            {

                if (Cbx_solution_file.SelectedItem is not FileInfo selected)
                {
                    ShowStatus("請先選擇一個解決方案檔案。", isError: true, useMessageBox: true);
                    return;
                }

                var slnPath = selected.FullName;
                var dir = Path.GetDirectoryName(slnPath)!;

                // 顯示操作開始
                ShowStatus($"開始轉換，模式：{(mode == ReferenceConversionMode.ProjectToDll ? "專案轉DLL參考" : "DLL轉專案參考")}", isError: false);
                Application.DoEvents(); // 讓UI更新

                var csprojFiles = Directory.EnumerateFiles(dir, "*.csproj", SearchOption.AllDirectories).ToList();
                if (!csprojFiles.Any())
                {
                    MessageBox.Show("資料夾中沒有找到 .csproj 檔案。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool hasChanges = false;
                int totalFiles = csprojFiles.Count;
                int processedCount = 0;
                int changedCount = 0;

                // 處理每個csproj檔案
                foreach (var file in csprojFiles)
                {
                    processedCount++;
                    ShowStatus($"正在處理 ({processedCount}/{totalFiles}): {Path.GetFileName(file)}");

                    bool fileChanged = false;
                    ProcessCsprojFile(file, ref fileChanged, slnPath, mode);

                    if (fileChanged)
                    {
                        changedCount++;
                        hasChanges = true;
                    }

                    Application.DoEvents(); // 讓UI能夠更新
                }

                var msg = hasChanges
                    ? (mode == ReferenceConversionMode.ProjectToDll)
                        ? "已完成轉換為 檔案參考"
                        : "已完成轉換為 專案參考"
                    : "沒有檔案需要轉換。";

                ShowStatus(msg, isError: !hasChanges);
            } catch (Exception ex)
            {
                ShowStatus($"轉換過程發生錯誤: {ex.Message}", isError: true, useMessageBox: true);
                Logger.LogToUI?.Invoke($"錯誤詳情: {ex}");
            }
        }

        private void ShowStatus(string message, bool isError = false, bool useMessageBox = false)
        {
            Lb_Convert_Status.Text = message;
            Lb_Convert_Status.ForeColor = isError ? Color.Red : Color.Green;

            if (useMessageBox)
            {
                var icon = isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
                MessageBox.Show(message, "訊息", MessageBoxButtons.OK, icon);
            }
        }

        private void Btn_ToggleLog_Click(object sender, EventArgs e)
        {
            Logger.IsEnabled = !Logger.IsEnabled;
            Lb_Log_Status.Text = Logger.IsEnabled ? "Log: 開啟" : "Log: 關閉";

            Pnl_Log.Visible = !Pnl_Log.Visible;
            int logWidth = Pnl_Log.Width;

            if (Pnl_Log.Visible)
            {
                this.Width = 1380;
                Btn_Clear_Log.Visible = true;
            }
            else
            {
                Lb_Log.Items.Clear();
                this.Width = 550;
                Btn_Clear_Log.Visible = false;
            }

        }

        private void Btn_Clear_Log_Click(object sender, EventArgs e)
        {
            Lb_Log.Items.Clear();
        }
    }
}
