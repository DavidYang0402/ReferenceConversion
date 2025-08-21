using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConversion.Infrastructure.Services
{
    public class DllCopier : IDllCopier
    {
        public DllCopier() { }

        public void Copy(string slnPath, string refName, string libsTargetDir, string refPath, string proName)
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

            string projectSubDir = GetProjectDirectoryFromCsprojPath(refPath);
            string fullDLLDir = (Path.GetRelativePath(firstDir, projectSubDir) == ".")
                ? rootSearchDir
                : Path.Combine(rootSearchDir, Path.GetRelativePath(firstDir, projectSubDir));

            // 找 \bin\Debug\ 資料夾
            string? debugDir = Directory.EnumerateDirectories(fullDLLDir, "*", SearchOption.AllDirectories)
                .FirstOrDefault(path =>
                    path.EndsWith(Path.Combine("bin", "Debug"), StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith("bin", StringComparison.OrdinalIgnoreCase));

            if (debugDir == null)
            {
               Logger.LogWarning($"在 {fullDLLDir} 下找不到 Debug 資料夾，略過");
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

            string libsTargetDirFull = "";
            if (!string.IsNullOrEmpty(libsTargetDir))
            {
                string? autoLibsPath = FindLibsInProjectOrSiblings(slnDir, libsTargetDir, proName);
                if (autoLibsPath == null)
                {
                    Logger.LogError("無法定位目標 Libs 資料夾（已依序檢查：自身、同名專案、其他專案）");
                    return;
                }
                libsTargetDirFull = autoLibsPath;
            }

            if (libsTargetDirFull == null) {
                Logger.LogError("目標 Libs 資料夾路徑無效");
                return;
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
                //File.SetLastWriteTime(targetPath, File.GetLastWriteTime(dllFile));
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

            string directoryPath = Path.GetDirectoryName(allowlistPath);
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new InvalidOperationException("無法從 path 擷取專案資料夾路徑");

            return directoryPath;
        }

        private string? FindLibsInProjectOrSiblings(string slnDir, string libsSpec, string proName)
        {
            string? targetFolder = ExtractSecondSegment(libsSpec);
            if (string.IsNullOrWhiteSpace(targetFolder))
                return null;

            try
            {
                string? baseDir = null;
                string mainCsproj = Path.Combine(slnDir, $"{proName}.csproj");
                if (File.Exists(mainCsproj))
                {
                    Logger.LogDebug($"在解決方案根目錄找到 {proName}.csproj");
                    baseDir = slnDir;
                }
                else
                {
                    Logger.LogDebug($"解決方案根目錄沒有 {proName}.csproj，開始搜尋 {proName} 目錄");

                    var candidateDirs = Directory.EnumerateDirectories(slnDir, proName, SearchOption.AllDirectories)
                        .OrderBy(p => p.Length);

                    foreach (var dir in candidateDirs)
                    {
                        var csproj = Path.Combine(dir, $"{proName}.csproj");
                        if (File.Exists(csproj))
                        {
                            baseDir = dir;
                            Logger.LogDebug($"找到符合的專案目錄：{dir} (含 {proName}.csproj)");
                            break;
                        }
                    }
                }

                if (baseDir == null)
                {
                    Logger.LogWarning($"找不到符合的專案 (缺少 {proName}.csproj)");
                    return null;
                }

                //確認 / 建立 targetFolder
                var libsDir = Path.Combine(baseDir, targetFolder);
                if (!Directory.Exists(libsDir))
                {
                    try
                    {
                        Directory.CreateDirectory(libsDir);
                        Logger.LogInfo($"建立目標 Libs 目錄：{libsDir}");
                    }
                    catch (Exception ce)
                    {
                        Logger.LogError($"建立目錄失敗：{libsDir}，錯誤：{ce.Message}");
                        return null;
                    }
                }
                else
                {
                    Logger.LogDebug($"使用既有目錄：{libsDir}");
                }

                return libsDir;
            }
            catch (Exception ex)
            {
                Logger.LogError($"尋找 Libs 發生例外：{ex.Message}");
            }

            return null;
        }

        private static string? ExtractSecondSegment(string pathLike)
        {
            if (string.IsNullOrWhiteSpace(pathLike))
                return null;

            var parts = pathLike
                .Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
                return parts[1];          // 第二段
            if (parts.Length == 1)
                return parts[0];          // 只有一段時取第一段
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
