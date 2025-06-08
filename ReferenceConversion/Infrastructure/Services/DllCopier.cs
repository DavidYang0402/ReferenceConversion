using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Shared;

namespace ReferenceConversion.Infrastructure.Services
{
    public class DllCopier : IDllCopier
    {
        public DllCopier() { }

        public void Copy(string slnPath, string refName, string libsTargetDir, string refPath)
        {
            string? slnDir = Path.GetDirectoryName(slnPath);
            if (slnDir == null)
            {
                Logger.LogError("無法取得 sln 目錄");
                return;
            }

            Logger.LogDebug($"專案所在目錄：{slnDir}");

            string firstDir = GetTopLevelDirectoryFromRelativePath(refPath);

            string? rootSearchDir = FindDirectoryUpwards(slnDir, firstDir);

            if (rootSearchDir == null)
            {
                Logger.LogError($"無法從 sln 路徑向上找到 '{firstDir}' 目錄");
                return;
            }
            Logger.LogInfo($"找到 {firstDir} 目錄：{rootSearchDir}");

            string? refDir = Directory.EnumerateDirectories(rootSearchDir, "*", SearchOption.AllDirectories)
                .FirstOrDefault(d => string.Equals(new DirectoryInfo(d).Name, refName, StringComparison.OrdinalIgnoreCase));


            if (refDir == null)
            {
                Logger.LogWarning($"在 {rootSearchDir} 下找不到名為 {refName} 的資料夾，略過");
                return;
            }

            // 找 \bin\Debug\ 資料夾
            string? debugDir = Directory.EnumerateDirectories(refDir, "*", SearchOption.AllDirectories)
                .FirstOrDefault(path =>
                    path.EndsWith(Path.Combine("bin", "Debug"), StringComparison.OrdinalIgnoreCase));

            if (debugDir == null)
            {
                Logger.LogWarning($"在 {refDir} 下找不到 Debug 資料夾，略過");
                return;
            }


            var allDlls = Directory.EnumerateFiles(debugDir, $"{refName}.dll", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .ToList();

            if (allDlls.Count == 0)
            {
                Logger.LogWarning($"在 {debugDir} 找不到任何 {refName}.dll，略過");
                return;
            }

            // 按照「最後寫入時間」排序，選最新的那個
            var latestDll = allDlls.OrderByDescending(f => f.LastWriteTime).First();
            string dllFile = latestDll.FullName;

            Logger.LogInfo($"選擇最新的 {refName}.dll，路徑：{dllFile}，最後修改時間：{latestDll.LastWriteTime}");

            string libsTargetDirFull;

            if (!string.IsNullOrEmpty(libsTargetDir))
            {
                libsTargetDirFull = Path.GetFullPath(
                    Path.IsPathRooted(libsTargetDir)
                        ? libsTargetDir
                        : Path.Combine(slnDir, libsTargetDir)
                );
            }
            else
            {
                // 自動搜尋子目錄中有 .csproj 的資料夾，並尋找其中的 Libs
                string? autoLibsPath = FindLibsInSiblingProject(slnDir);
                if (autoLibsPath == null)
                {
                    Logger.LogError("無法自動尋找 Libs 資料夾，請確認結構或手動指定");
                    return;
                }

                libsTargetDirFull = autoLibsPath;
            }

            try
            {
                Directory.CreateDirectory(libsTargetDirFull);

            }
            catch (Exception ex)
            {
                Logger.LogError($"建立目標資料夾失敗：{ex.Message}");
                return;
            }

            string targetPath = Path.Combine(libsTargetDirFull, Path.GetFileName(dllFile));
            if (File.Exists(targetPath) &&
                File.GetLastWriteTime(targetPath) >= File.GetLastWriteTime(dllFile))
            {
                Logger.LogInfo($"{refName}.dll 已經是最新版本，略過複製");
                return;
            }

            try
            {
                Logger.LogInfo($"複製 {refName}.dll 到 {targetPath}");
                File.Copy(dllFile, targetPath, overwrite: true);
                File.SetLastWriteTime(targetPath, File.GetLastWriteTime(dllFile));
                Logger.LogSuccess($"{refName}.dll 複製成功");
            }
            catch (Exception ex)
            {
                Logger.LogError($"複製失敗：{ex.Message}\n詳細錯誤：{ex}", ex);
            }
        }

        public string GetTopLevelDirectoryFromRelativePath(string allowlistRelativePath)
        {
            // 抓出 allowlist 裡面 path 的第一層目錄
            if (string.IsNullOrWhiteSpace(allowlistRelativePath))
                throw new ArgumentException("refPath 為空或無效", nameof(allowlistRelativePath));

            string[] parts = allowlistRelativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
                throw new InvalidOperationException("無法從 refPath 擷取第一層目錄");

            return parts[0];
        }

        //取出 .csproj 路徑中的專案資料夾路徑
        public string GetProjectDirectoryFromCsprojPath(string allowlistPath)
        {
            if (string.IsNullOrWhiteSpace(allowlistPath))
                throw new ArgumentException("path 為空或無效", nameof(allowlistPath));

            if (!allowlistPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("path 必須是以 .csproj 結尾的專案路徑");

            string directoryPath = Path.GetDirectoryName(allowlistPath);
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new InvalidOperationException("無法從 path 擷取專案資料夾路徑");

            return directoryPath;
        }


        private string? FindLibsInSiblingProject(string slnDir)
        {
            try
            {
                foreach (var subDir in Directory.GetDirectories(slnDir))
                {
                    // 看起來像專案的資料夾（有 .csproj）
                    if (Directory.EnumerateFiles(subDir, "*.csproj").Any())
                    {
                        var libsPath = Path.Combine(subDir, "Libs");
                        if (Directory.Exists(libsPath))
                        {
                            Logger.LogDebug($"自動偵測到 Libs 資料夾：{libsPath}");
                            return libsPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"尋找 Libs 時發生錯誤：{ex.Message}");
            }

            return null;
        }

        private string? FindDirectoryUpwards(string startDir, string targetFolderName)
        {
            var dir = new DirectoryInfo(startDir);

            while (dir != null)
            {
                var match = dir.GetDirectories()
                    .FirstOrDefault(d => string.Equals(d.Name, targetFolderName, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    return match.FullName;
                }

                dir = dir.Parent;
            }

            return null;
        }

    }
}
