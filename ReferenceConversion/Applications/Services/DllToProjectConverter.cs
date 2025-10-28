using ReferenceConversion.Applications.Interfaces;
using ReferenceConversion.Data;
using ReferenceConversion.Domain.Enum;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Infrastructure.Services;
using ReferenceConversion.Modifier;
using ReferenceConversion.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReferenceConversion.Applications.Services
{
    public class DllToProjectConverter : IReferenceConversionStrategy
    {
        public ReferenceConversionMode Mode => ReferenceConversionMode.DllToProject;

        private readonly IAllowlistManager _allowlistManager;
        private readonly Func<string, ISlnModifier> _slnModifierFactory;

        public DllToProjectConverter(IAllowlistManager allowlistManager, Func<string, ISlnModifier> slnModifierFactory)
        {
            _allowlistManager = allowlistManager;
            _slnModifierFactory = slnModifierFactory;
        }

        public bool Convert(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath, string csprojPath)
        {
            bool isChanged = false;
            var referenceNodes = xmlDoc.GetElementsByTagName("Reference");
            var nodesToRemove = new List<XmlNode>();
            var slnModifier = _slnModifierFactory(slnFilePath);

            Logger.LogInfo($"開始處理 SLN 檔案: {slnFilePath}, 進行轉成專案參考");

            string csprojProName = Path.GetFileNameWithoutExtension(csprojPath);
            bool isShareUserCtrl = string.Equals(csprojProName, "ShareUserCtrl", StringComparison.Ordinal);

            for (int i = 0; i < referenceNodes.Count; i++)
            {
                XmlNode? node = referenceNodes[i];
                XmlAttribute includeAttr = node.Attributes?["Include"];
                if(includeAttr is null) continue;

                string referenceName = includeAttr.Value.Split(',')[0];
                if (string.IsNullOrEmpty(referenceName) || processedReferences.Contains(referenceName)) continue;

                Logger.LogInfo($"處理 ProjectReference: {referenceName}");

                if (_allowlistManager.IsInAllowlist(referenceName, out var project, out var entry))
                {
                    Logger.LogInfo($"找到允許的專案: {entry.Name}, 將轉換為 DLL");

                    // NOTE: Old code VER.1
                    //string relativePath = Path.Combine(
                    //    (!isShareUserCtrl)
                    //      ? Enumerable.Repeat("..", entry.CsprojDepth).ToArray()
                    //      : Enumerable.Repeat("..", 4).ToArray()
                    //  );

                    //relativePath = Path.Combine(relativePath, entry.Path);

                    int csprojDepth = !isShareUserCtrl ? entry.CsprojDepth : 4;

                    // NOTE: Old code VER.2
                    //string relativePath = BuildRelativePath(csprojDepth, entry.Path);

                    if (!TryResolveProjectPaths(slnFilePath, csprojPath, entry, csprojDepth, out var relativePath, out var slnRelativePath))
                    {
                        Logger.LogWarning($"找不到 {entry.Name} 對應的 .csproj 路徑，略過轉換。");
                        continue;
                    }

                    var newElement = xmlDoc.CreateElement("ProjectReference");
                    newElement.SetAttribute("Include", relativePath);

                    var projectGuidElement = xmlDoc.CreateElement("Project");
                    projectGuidElement.InnerText = entry.Guid;
                    newElement.AppendChild(projectGuidElement);

                    var nameElement = xmlDoc.CreateElement("Name");
                    nameElement.InnerText = entry.Name;
                    newElement.AppendChild(nameElement);

                    // NOTE: Old code VER1
                    //string slnRelativePath = Path.Combine(Enumerable.Repeat("..", entry.SlnDepth).ToArray());

                    //slnRelativePath = Path.Combine(slnRelativePath, entry.Path);

                    // NOTE: Old code VER2
                    //string slnRelativePath = BuildRelativePath(entry.SlnDepth, entry.Path);

                    slnModifier.AddProjectReferenceToSln(entry.Name, slnRelativePath, project.ProjectGuid, entry.Guid, entry.ParentGuid);

                    node.ParentNode?.AppendChild(newElement);
                    nodesToRemove.Add(node);
                    processedReferences.Add(referenceName);
                    isChanged = true;

                    Logger.LogSeparator();
                }
            }
            Logger.LogInfo($"結束處理 SLN 檔案: {slnFilePath}");
            Logger.LogSeparator();

            foreach (XmlNode node in nodesToRemove)
                node.ParentNode?.RemoveChild(node);

            return isChanged;
        }

        // NOTE: NEW function
        private static bool TryResolveProjectPaths(
            string slnFilePath,
            string csprojPath,
            ReferenceItem entry,
            int csprojDepth,
            out string relativePath,
            out string slnRelativePath)
        {
            string? allowlistPath = string.IsNullOrWhiteSpace(entry.Path) ? null : entry.Path;

            if (allowlistPath is not null)
            {
                relativePath = BuildRelativePath(csprojDepth, allowlistPath);
                slnRelativePath = BuildRelativePath(entry.SlnDepth, allowlistPath);
                return true;
            }

            string? slnDir = Path.GetDirectoryName(slnFilePath);
            string? csprojDir = Path.GetDirectoryName(csprojPath);
            if (string.IsNullOrWhiteSpace(slnDir) || string.IsNullOrWhiteSpace(csprojDir))
            {
                relativePath = string.Empty;
                slnRelativePath = string.Empty;
                return false;
            }

            string? discoveredProject = LocateProjectFromSolution(slnDir, slnFilePath, entry)
                ?? LocateProjectByName(slnDir, entry.Name);
            if (discoveredProject is null)
            {
                relativePath = string.Empty;
                slnRelativePath = string.Empty;
                return false;
            }

            relativePath = NormalizePath(Path.GetRelativePath(csprojDir, discoveredProject));
            slnRelativePath = NormalizePath(Path.GetRelativePath(slnDir, discoveredProject));
            return true;
        }

        private static string? LocateProjectFromSolution(string slnDir, string slnFilePath, ReferenceItem entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Guid) || !File.Exists(slnFilePath))
            {
                return null;
            }

            try
            {
                string normalizedTargetGuid = NormalizeGuid(entry.Guid);

                foreach (string line in File.ReadLines(slnFilePath))
                {
                    string trimmed = line.TrimStart();
                    if (!trimmed.StartsWith("Project(", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var quotedSegments = ExtractQuotedSegments(trimmed);
                    if (quotedSegments.Count < 4)
                    {
                        continue;
                    }

                    string candidateGuid = quotedSegments[3];
                    if (!string.Equals(NormalizeGuid(candidateGuid), normalizedTargetGuid, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string pathSegment = quotedSegments[2];
                    string fullPath = Path.GetFullPath(Path.Combine(slnDir, pathSegment));

                    if (!File.Exists(fullPath))
                    {
                        Logger.LogWarning($"在 {slnFilePath} 找到 GUID {entry.Guid} 但 {fullPath} 不存在，仍使用解決方案中的路徑進行轉換。");
                    }

                    return fullPath;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"解析 {slnFilePath} 以尋找 {entry.Name}({entry.Guid}) 時發生錯誤: {ex.Message}", ex);
            }

            return null;
        }

        private static string NormalizePath(string path)
        {
            return path;
        }

        private static IReadOnlyList<string> ExtractQuotedSegments(string line)
        {
            var segments = new List<string>();
            int index = 0;
            while (index < line.Length)
            {
                int start = line.IndexOf('"', index);
                if (start == -1)
                {
                    break;
                }

                int end = line.IndexOf('"', start + 1);
                if (end == -1)
                {
                    break;
                }

                segments.Add(line.Substring(start + 1, end - start - 1));
                index = end + 1;
            }

            return segments;
        }

        private static string NormalizeGuid(string guid)
        {
            return guid.Trim().TrimStart('{').TrimEnd('}');
        }

        private static string? LocateProjectByName(string slnDir, string projectName)
        {
            try
            {
                return Directory.EnumerateFiles(slnDir, $"{projectName}.csproj", SearchOption.AllDirectories)
                    .OrderBy(p => p.Length)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.LogError($"搜尋 {projectName}.csproj 時發生錯誤: {ex.Message}", ex);
                return null;
            }
        }

        private static string BuildRelativePath(int depth, string targetPath)
        {
            var segments = new List<string>();

            if (depth > 0)
            {
                segments.AddRange(Enumerable.Repeat("..", depth));
            }

            if (!string.IsNullOrWhiteSpace(targetPath))
            {
                segments.Add(targetPath);
            }

            string combined = Path.Combine(segments.ToArray());
            return combined;
        }
    }
}
