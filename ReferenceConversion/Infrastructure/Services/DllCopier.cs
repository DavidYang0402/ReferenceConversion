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

            var firstDir = GetShareCoreRootFromAllowlistPath(refPath);

            string? shareCoreDir = FindDirectoryUpwards(slnDir, firstDir);
            Debug.WriteLine($"................: {shareCoreDir}");

            if (shareCoreDir == null)
            {
                Logger.LogError($"無法從 sln 路徑向上找到 '{firstDir}' 目錄");
                return;
            }

            Logger.LogInfo($"找到 ShareCore 目錄：{shareCoreDir}");

            string? dllFile = Directory.EnumerateFiles(shareCoreDir, $"{refName}.dll", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (dllFile == null)
            {
                Logger.LogWarning($"找不到 {refName} 的 DLL（{refName}.dll），略過");
                return;
            }

            Logger.LogDebug($"DLL 路徑：{dllFile}");

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
            try
            {
                Logger.LogInfo($"複製 {refName}.dll 到 {targetPath}");
                File.Copy(dllFile, targetPath, overwrite: true);
                Logger.LogSuccess($"{refName}.dll 複製成功\n");
            }
            catch (Exception ex)
            {
                Logger.LogError($"複製失敗：{ex.Message}\n", ex);
            }
        }

        public string? GetShareCoreRootFromAllowlistPath(string allowlistRelativePath)
        {
            // 抓出 allowlist 裡面 path 的第一層目錄
            string firstDir = allowlistRelativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];

            Debug.WriteLine($"................: {firstDir}");

            return firstDir;
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
                if (dir.GetDirectories(targetFolderName).Any())
                {
                    return Path.Combine(dir.FullName, targetFolderName);
                }
                dir = dir.Parent;
            }

            return null;
        }

    }
}
