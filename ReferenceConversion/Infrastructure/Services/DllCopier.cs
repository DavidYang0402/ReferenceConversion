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

        public void Copy(string slnPath, string proName, string refName, string libsTargetDir)
        {
            string? slnDir = Path.GetDirectoryName(slnPath);
            Logger.LogDebug($"專案路徑：{slnDir}");

            var csprojFiles = Directory.EnumerateFiles(slnDir, "*.csproj", SearchOption.AllDirectories);
            Logger.LogDebug($"🔍 尋找專案檔：共 {csprojFiles.Count()} 個檔案");

            var mainCsproj = csprojFiles
                .FirstOrDefault(path =>
                    Path.GetFileNameWithoutExtension(path).Contains(proName, StringComparison.OrdinalIgnoreCase));
            Logger.LogDebug($"主專案檔案：{mainCsproj}");

            if (mainCsproj == null)
            {
                Logger.LogError($"❌ 找不到主專案 '{proName}'");
                return;
            }

            Logger.LogInfo("🚀 開始複製主專案底下的 DLL");

            var csprojDir = Path.GetDirectoryName(mainCsproj)!;
            Logger.LogDebug($"📁 專案目錄：{csprojDir}");

            var dllFiles = Directory.EnumerateFiles(csprojDir, "*.dll", SearchOption.AllDirectories).ToList();
            Logger.LogDebug($"🔍 尋找 DLL：共 {dllFiles.Count} 個檔案");
            foreach (var file in dllFiles)
            {
                Logger.LogDebug($"   └─ {file}");
            }

            var allowDll = dllFiles
                .FirstOrDefault(path =>
                    Path.GetFileNameWithoutExtension(path).Equals(refName, StringComparison.OrdinalIgnoreCase));
            Logger.LogDebug($"符合的 DLL：{allowDll}");

            if (allowDll != null)
            {
                string libsTargetDirFull = Path.GetFullPath(
                    Path.IsPathRooted(libsTargetDir)
                        ? libsTargetDir
                        : Path.Combine(slnDir!, libsTargetDir)
                );

                try
                {
                    Directory.CreateDirectory(libsTargetDirFull);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"❌ 建立資料夾失敗：{ex.Message}");
                    return;
                }

                Logger.LogDebug($"🔧 DllPath 是 {(Path.IsPathRooted(libsTargetDir) ? "絕對路徑" : "相對路徑")}：{libsTargetDir}");
                Logger.LogDebug($"📦 最終複製目標路徑：{libsTargetDirFull}");

                var targetPath = Path.Combine(libsTargetDirFull, Path.GetFileName(allowDll));

                Logger.LogInfo($"✅ 符合的 DLL：{allowDll}");
                Logger.LogInfo($"📦 複製到：{targetPath}");

                File.Copy(allowDll, targetPath, overwrite: true);
                Logger.LogInfo("🎉 複製成功！");
            }
            else
            {
                Logger.LogError($"❌ 找不到名稱為 '{refName}' 的 DLL");
            }
        }

    }
}
