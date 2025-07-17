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

            AutoLoadCommonSolutions();
        }


        // 修改自動載入解決方案方法，直接從應用程式所在目錄開始尋找
        private void AutoLoadCommonSolutions()
        {
            try
            {
                // 從應用程式所在目錄開始搜尋 .sln 檔案
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // 建立解決方案檔案路徑清單
                List<string> solutions = new List<string>();
                
                // 先檢查當前目錄
                solutions.AddRange(Directory.GetFiles(appDir, "*.sln"));
                
                // 如果當前目錄沒有找到，則向上一層尋找
                if (!solutions.Any())
                {
                    string? parentDir = Path.GetDirectoryName(appDir);
                    if (parentDir != null && Directory.Exists(parentDir))
                    {
                        solutions.AddRange(Directory.GetFiles(parentDir, "*.sln"));
                        
                        // 再向上一層尋找 (通常專案目錄結構為 bin/Debug/net8.0/app.exe)
                        string? grandParentDir = Path.GetDirectoryName(parentDir);
                        if (grandParentDir != null && Directory.Exists(grandParentDir))
                        {
                            solutions.AddRange(Directory.GetFiles(grandParentDir, "*.sln"));
                            
                            // 再向上一層
                            string? greatGrandParentDir = Path.GetDirectoryName(grandParentDir);
                            if (greatGrandParentDir != null && Directory.Exists(greatGrandParentDir))
                            {
                                solutions.AddRange(Directory.GetFiles(greatGrandParentDir, "*.sln"));
                            }
                        }
                    }
                }
                
                // 如果還是沒找到，則向下搜尋子目錄
                if (!solutions.Any())
                {
                    solutions.AddRange(FindSolutionFiles(appDir));
                }

                // 如果找到解決方案，則載入第一個
                if (solutions.Any())
                {
                    string slnFile = solutions.First();
                    Tb_ShowPath.Text = slnFile; // 顯示路徑到 TextBox
                    LoadSolutionFile(slnFile);
                }
                else
                {
                    Logger.LogInfo("未找到任何解決方案檔案，請手動選擇。");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"自動載入解決方案時發生錯誤: {ex.Message}");
            }
        }

        // 尋找解決方案檔案的輔助方法
        private List<string> FindSolutionFiles(string directory, int maxDepth = 3)
        {
            List<string> solutions = new List<string>();

            if (maxDepth <= 0 || !Directory.Exists(directory))
                return solutions;

            try
            {
                // 尋找當前目錄中的所有 .sln 檔案
                solutions.AddRange(Directory.GetFiles(directory, "*.sln"));

                // 遞迴搜尋子目錄
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    if (Path.GetFileName(subDir).Equals("trunk", StringComparison.OrdinalIgnoreCase))
                    {
                        // 如果是 trunk 目錄，優先搜尋
                        solutions.AddRange(FindSolutionFiles(subDir, maxDepth - 1));
                    }
                    else
                    {
                        // 其他目錄也搜尋，但降低搜尋深度
                        solutions.AddRange(FindSolutionFiles(subDir, maxDepth - 1));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogToUI?.Invoke($"搜尋目錄 {directory} 時發生錯誤: {ex.Message}");
            }

            return solutions;
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
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Solution Files (*.sln)|*.sln";
                fileDialog.Title = "選擇解決方案檔案";
                fileDialog.InitialDirectory = !string.IsNullOrEmpty(Tb_ShowPath.Text) ? 
                    Path.GetDirectoryName(Tb_ShowPath.Text) 
                     : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if(fileDialog.ShowDialog() == DialogResult.OK)
                {
                    string slnFilePath = fileDialog.FileName;
                    Tb_ShowPath.Text = slnFilePath;
                    
                    LoadSolutionFile(slnFilePath); 
                }
            }
        }

        private void LoadSolutionFile(string slnFilePath)
        {
            try
            {
                if (!File.Exists(slnFilePath))
                {
                    MessageBox.Show("選擇的解決方案檔案不存在。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string? slnDir = Path.GetDirectoryName(slnFilePath);
                if (string.IsNullOrEmpty(slnDir))
                {
                    MessageBox.Show("無法取得解決方案所在目錄。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 清除並設置解決方案檔案
                Cbx_solution_file.DataSource = null;
                var fileInfo = new FileInfo(slnFilePath);
                Cbx_solution_file.DataSource = new List<FileInfo> { fileInfo };
                Cbx_solution_file.DisplayMember = "Name";
                Cbx_solution_file.ValueMember = "FullName";
                Cbx_solution_file.SelectedIndex = 0;

                // 載入該解決方案目錄下的所有 .csproj 檔案
                LoadCsprojFiles(slnDir);

                // 自動選擇匹配的 Allowlist
                SelectMatchingAllowlist(fileInfo.Name);

                Logger.LogInfo($"已載入解決方案: {fileInfo.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入解決方案發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError($"載入解決方案失敗: {ex}");
            }
        }

        // 新增方法: 自動選擇匹配的 Allowlist
        private void SelectMatchingAllowlist(string slnFileName)
        {
            try
            {
                // 從檔案名移除副檔名
                string slnName = Path.GetFileNameWithoutExtension(slnFileName);
                
                // 在所有 allowlist 項目中尋找匹配的
                for (int i = 0; i < Cbx_Project_Allowlist.Items.Count; i++)
                {
                    string projectName = Cbx_Project_Allowlist.Items[i].ToString();
                    
                    // 檢查是否包含（不需要完全相等)
                    if (projectName.Contains(slnName, StringComparison.OrdinalIgnoreCase) || 
                        slnName.Contains(projectName, StringComparison.OrdinalIgnoreCase))
                    {
                        Cbx_Project_Allowlist.SelectedIndex = i;
                        Logger.LogInfo($"自動選擇了匹配的 Allowlist: {projectName}");
                        return;
                    }
                }
                
                // 如果找不到匹配的，使用第一個
                if (Cbx_Project_Allowlist.Items.Count > 0 && Cbx_Project_Allowlist.SelectedIndex < 0)
                {
                    Cbx_Project_Allowlist.SelectedIndex = 0;
                    Logger.LogInfo($"找不到匹配的 Allowlist，使用第一個: {Cbx_Project_Allowlist.Items[0]}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"選擇匹配 Allowlist 時發生錯誤: {ex.Message}");
            }
        }

        // 修改專案定位方法，直接使用解決方案檔案
        public void LocateProjectPathAndShow(string projectName)
        {
            // 如果已經選擇了解決方案，保持不變
            if (Cbx_solution_file.SelectedItem is FileInfo fileInfo)
            {
                return;
            }

            // 否則提示用戶選擇解決方案
            //MessageBox.Show("請先選擇一個解決方案檔案。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Cbx_Project_Allowlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Cbx_Project_Allowlist.SelectedItem != null)
            {
                string selectedProject = Cbx_Project_Allowlist.SelectedItem.ToString();

                _allowlistManager.SetCurrentProjectName(selectedProject);
                _allowlistManager.DisplayAllowlistForProject(selectedProject, Lb_Allowlist);

                // 嘗試找到與所選專案相關的解決方案
                FindAndLoadSolutionForProject(selectedProject);

                Tb_Ref_Path.Text = Path.Combine("..", _allowlistManager.GetProjectDllPath(selectedProject));

                // 設定 UI 屬性
                Lb_Convert_Status.Text = "轉換狀態：無";
                Lb_Convert_Status.ForeColor = Color.Black;
                Lb_Log.Items.Clear();
            }
        }

        // 新增尋找專案相關解決方案的方法
        private void FindAndLoadSolutionForProject(string projectName)
        {
            try
            {
                // 先檢查現有的解決方案是否符合（使用 contains 檢查）
                if (Cbx_solution_file.SelectedItem is FileInfo selectedSln)
                {
                    string slnName = Path.GetFileNameWithoutExtension(selectedSln.Name);
                    if (slnName.Contains(projectName, StringComparison.OrdinalIgnoreCase) || 
                        projectName.Contains(slnName, StringComparison.OrdinalIgnoreCase))
                    {
                        // 現有解決方案符合專案名稱，不需要重新載入
                        return;
                    }
                }

                // 嘗試在 WorkProjects 資料夾中尋找
                string workProjectsPath = @"C:\WorkProjects";
                
                // 首先檢查精確匹配的路徑
                string projectPath = Path.Combine(workProjectsPath, projectName);
                if (Directory.Exists(projectPath))
                {
                    var solutions = FindSolutionFiles(projectPath);
                    if (solutions.Any())
                    {
                        LoadSolutionFile(solutions.First());
                        return;
                    }
                }

                // 如果找不到匹配的專案目錄，嘗試模糊比對
                if (Directory.Exists(workProjectsPath))
                {
                    foreach (var dir in Directory.GetDirectories(workProjectsPath))
                    {
                        string dirName = Path.GetFileName(dir);
                        
                        // 使用 contains 檢查，而非完全相等
                        if (dirName.Contains(projectName, StringComparison.OrdinalIgnoreCase) || 
                            projectName.Contains(dirName, StringComparison.OrdinalIgnoreCase))
                        {
                            var solutions = FindSolutionFiles(dir);
                            if (solutions.Any())
                            {
                                LoadSolutionFile(solutions.First());
                                return;
                            }
                        }
                    }
                }

                Logger.LogInfo($"找不到與 {projectName} 相關的解決方案檔案。");
            }
            catch (Exception ex)
            {
                Logger.LogError($"尋找專案解決方案時發生錯誤: {ex.Message}");
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

        private async void Btn_ToReference_Click(object sender, EventArgs e) => await ExecuteConversion(ReferenceConversionMode.ProjectToDll);

        private async void Btn_ToProjectReference_Click(object sender, EventArgs e) => await ExecuteConversion(ReferenceConversionMode.DllToProject);

        private async Task ExecuteConversion(ReferenceConversionMode mode)
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

                await Task.Run(() => 
                {
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
                    }
                });

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
            if (Lb_Convert_Status.InvokeRequired)
            {
                Lb_Convert_Status.Invoke(new Action(() => ShowStatus(message, isError, useMessageBox)));
            }
            else
            {
                Lb_Convert_Status.Text = message;
                Lb_Convert_Status.ForeColor = isError ? Color.Red : Color.Green;

                if (useMessageBox)
                {
                    var icon = isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
                    MessageBox.Show(message, "訊息", MessageBoxButtons.OK, icon);
                }
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
