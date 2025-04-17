using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ReferenceConversion.Modifier
{
    public class SlnModifier : ISlnModifier
    {
        private readonly string _slnPath;
        public SlnModifier(string slnPath)
        {
            _slnPath = slnPath;
        }

        // 插入與刪除專案參照
        public void AddProjectReferenceToSln(string projectName, string projectPath, string projectGuid, string refGuid, string parentGuid)
        {
            string slnContent = ReadSlnContent();

            if (!ProjectExistsInSln(slnContent, projectGuid, projectName, projectPath, refGuid))
            {
                int insertIndex = slnContent.LastIndexOf("EndProject");
                if (insertIndex != -1)
                {
                    insertIndex += "EndProject".Length;  // 確保插入點在 EndProject 之後
                    slnContent = slnContent.Insert(insertIndex, CreateProjectEntry(projectGuid, projectName, projectPath, refGuid));
                }
                else
                {
                    Console.WriteLine("[錯誤] 找不到 `EndProject`，可能是 .sln 格式有變更");
                    return;
                }

                if (!ProjectConfigExists(slnContent, refGuid) && !string.IsNullOrEmpty(parentGuid))
                {
                    InsertProjectConfig(ref slnContent, refGuid);
                }

                if (!ProjectConfigExists(slnContent, projectGuid))
                {
                    InsertProjectConfig(ref slnContent, projectGuid);
                }

                if (!string.IsNullOrEmpty(parentGuid) && !NestedProjectExists(slnContent, refGuid, parentGuid))
                {
                    CreateNestedProject(ref slnContent, refGuid, parentGuid);
                }
            }
            else
            {
                Console.WriteLine("[警告] 已經存在該專案或參照，跳過插入");
            }          

            WriteSlnContent(slnContent);
        }
        public void RemoveProjectReferenceFromSln(string projectName, string refGuid, string projectGuid)
        {
            // 讀取 .sln 檔案內容
            string slnContent = ReadSlnContent();

         // 使用正則表達式刪除匹配的專案區塊
            Regex projectRegex = new Regex(RemoveProjectEntry(projectGuid, projectName, refGuid), RegexOptions.IgnoreCase | RegexOptions.Singleline);
            slnContent = projectRegex.Replace(slnContent, "");

            slnContent = RemoveProjectConfigurationIfUnused(slnContent, projectGuid);
            slnContent = RemoveProjectConfigurationIfUnused(slnContent, refGuid);

            RemoveFromNestedProjects(ref slnContent, refGuid);

            // 移除多餘的空白行
            slnContent = Regex.Replace(slnContent, @"^\s*\r?\n", "", RegexOptions.Multiline);

            // 將修改後的內容寫回 .sln 檔案
            WriteSlnContent(slnContent);
        }
        public void CreateNestedProject(ref string slnContent, string childGuid, string parentGuid)
        {
            string nestedEntry = $"\t\t{childGuid} = {parentGuid}\n";
            var nestedRegex = new Regex(@"(GlobalSection\(NestedProjects\).*?)(EndGlobalSection)", RegexOptions.Singleline);

            if (nestedRegex.IsMatch(slnContent))
            {
                slnContent = nestedRegex.Replace(slnContent, m =>
                    m.Groups[1].Value + nestedEntry + m.Groups[2].Value
                );
            }
            else
            {
                string newSection =
                $"\tGlobalSection(NestedProjects) = preSolution\n{nestedEntry}\tEndGlobalSection\n";
                slnContent = Regex.Replace(slnContent, @"(Global\s*)", $"Global\n{newSection}");
            }
        }
        public void RemoveFromNestedProjects(ref string slnContent, string childGuid)
        {
            var nestedRegex = new Regex(@"(GlobalSection\(NestedProjects\).*?EndGlobalSection)", RegexOptions.Singleline);

            if (nestedRegex.IsMatch(slnContent))
            {
                slnContent = nestedRegex.Replace(slnContent, match =>
                {
                    var lines = match.Value.Split('\n')
                                           .Where(line => !line.Contains(childGuid))
                                           .ToList();
                    return string.Join("\n", lines);
                });
            }
        }

        // Sln 檔案操作
        private string ReadSlnContent()
        {
            try
            {
                return File.ReadAllText(_slnPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[錯誤] 無法讀取 .sln 檔案: {ex.Message}");
                throw; // 可選：根據需要重新拋出或處理
            }
        }
        private void WriteSlnContent(string content)
        {
            try
            {
                File.WriteAllText(_slnPath, content, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[錯誤] 無法寫入 .sln 檔案: {ex.Message}");
                throw;
            }
        }

        // 專案檢查
        private bool ProjectExistsInSln(string slnContent, string projectGuid, string projectName, string projectPath,string refGuid)
        {
            //string pattern = $@"Project\([^\)]+\)\s*=\s*""[^""]+"",\s*""[^""]+"",\s*""\{{{Regex.Escape(guid)}\}}""";
            string pattern = $"\nProject(\"{projectGuid}\") = \"{projectName}\", \"{projectPath}\", \"{refGuid}\"\nEndProject";
            return slnContent.Contains(pattern);
        }
        private static bool ProjectConfigExists(string slnContent, string guid)
        {
            return slnContent.Contains($"{guid}.Debug|Any CPU");
        }
        private static bool NestedProjectExists(string slnContent, string childGuid, string parentGuid)
        {
            return slnContent.Contains($"{childGuid} = {parentGuid}");
        }

        // 插入專案配置
        private void InsertProjectConfig(ref string slnContent, string guid)
        {
            const string configSectionPattern = @"(GlobalSection\(ProjectConfigurationPlatforms\).*?EndGlobalSection)";

            slnContent = Regex.Replace(slnContent, configSectionPattern, match =>
            {
                string configEntry = CreateProjectConfig(guid);
                // 插入點為 `EndGlobalSection` 前一行
                string newSection = match.Value.Replace("EndGlobalSection", configEntry + "EndGlobalSection");
                return newSection;
            }, RegexOptions.Singleline);
        }     
        private string CreateProjectEntry(string projectGuid, string projectName, string projectPath, string refGuid)
        {
            return $"\nProject(\"{projectGuid}\") = \"{projectName}\", \"{projectPath}\", \"{refGuid}\"\nEndProject";
        }
        private string CreateProjectConfig(string guid)
        {
            return $@"
                {guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {guid}.Debug|Any CPU.Build.0 = Debug|Any CPU
                {guid}.Release|Any CPU.ActiveCfg = Release|Any CPU
                {guid}.Release|Any CPU.Build.0 = Release|Any CPU
";
        }

        // 刪除專案配置
        private string RemoveProjectEntry(string projectGuid, string projectName, string refGuid) 
        {
            return $@"Project\(""{Regex.Escape(projectGuid)}""\)\s*=\s*""{Regex.Escape(projectName)}""\s*,\s*""[^""]+""\s*,\s*""{Regex.Escape(refGuid)}""[\s\S]*?EndProject";
        }
        private string RemoveProjectConfig(string guid)
        {
            return $@"\s*{Regex.Escape(guid)}\.[^\n]*ActiveCfg\s*=[^\n]*[\s\S]*?\s*{Regex.Escape(guid)}\.[^\n]*Build.0\s*=[^\n]*";
        }
        private string RemoveProjectConfigurationIfUnused(string slnContent, string guid)
        {
            if (!Regex.IsMatch(slnContent, $@"\{{{Regex.Escape(guid)}}}"))
            {
                Regex regex = new Regex(RemoveProjectConfig(guid), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                return regex.Replace(slnContent, "");
            }
            return slnContent;
        }
    }
}
