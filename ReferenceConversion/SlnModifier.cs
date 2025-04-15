using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ReferenceConversion
{
    public class SlnModifier
    {
        string _slnPath;
        public SlnModifier(string slnPath)
        {
            _slnPath = slnPath;
        }

        public void AddProjectReferenceToSln(string projectName, string projectPath, string projectGuid, string refGuid, string parentGuid)
        {
            string slnContent = File.ReadAllText(_slnPath);

            if (!slnContent.Contains(refGuid))
            {
                int insertIndex = slnContent.LastIndexOf("EndProject");
                if (insertIndex != -1)
                {
                    insertIndex += "EndProject".Length;  // 確保插入點在 EndProject 之後
                    string projectEntry = $"\nProject(\"{projectGuid}\") = \"{projectName}\", \"{projectPath}\", \"{refGuid}\"\nEndProject";
                    slnContent = slnContent.Insert(insertIndex, projectEntry);
                }
                else
                {
                    Console.WriteLine("[錯誤] 找不到 `EndProject`，可能是 .sln 格式有變更");
                    return;
                }
            }
            else
            {
                Console.WriteLine("[警告] 已經存在該專案或參照，跳過插入");
            }

            if (!slnContent.Contains($"{refGuid}.Debug|Any CPU") && !string.IsNullOrEmpty(parentGuid))
            {
                string projectConfig = $@"
                {refGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {refGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
                {refGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
                {refGuid}.Release|Any CPU.Build.0 = Release|Any CPU
";

                slnContent = Regex.Replace(slnContent, @"(GlobalSection\(ProjectConfigurationPlatforms\).*?EndGlobalSection)", match =>
                {
                    return match.Groups[1].Value.Insert(match.Groups[1].Value.LastIndexOf("EndGlobalSection"), projectConfig);
                }, RegexOptions.Singleline);
            }

            if (!slnContent.Contains($"{projectGuid}.Debug|Any CPU"))
            {
                string projectConfig = $@"
                {projectGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {projectGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
                {projectGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
                {projectGuid}.Release|Any CPU.Build.0 = Release|Any CPU
";

                slnContent = Regex.Replace(slnContent, @"(GlobalSection\(ProjectConfigurationPlatforms\).*?EndGlobalSection)", match =>
                {
                    return match.Groups[1].Value.Insert(match.Groups[1].Value.LastIndexOf("EndGlobalSection"), projectConfig);
                }, RegexOptions.Singleline);
            }

            if (!string.IsNullOrEmpty(parentGuid) && !slnContent.Contains($"{refGuid} = {parentGuid}"))
            {
                AddNestedProject(ref slnContent, refGuid, parentGuid);
            }

            File.WriteAllText(_slnPath, slnContent, Encoding.UTF8);
        }

        public void RemoveProjectReferenceFromSln(string projectName, string refGuid, string projectGuid)
        {
            // 讀取 .sln 檔案內容
            string slnContent = File.ReadAllText(_slnPath);

            // 用正則表達式匹配該項目及其區塊
            string projectPattern = $@"Project\(""{Regex.Escape(projectGuid)}""\)\s*=\s*""{Regex.Escape(projectName)}""\s*,\s*""[^""]+""\s*,\s*""{Regex.Escape(refGuid)}""[\s\S]*?EndProject";

            // 使用正則表達式刪除匹配的專案區塊
            Regex projectRegex = new Regex(projectPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            slnContent = projectRegex.Replace(slnContent, "");


            if (!Regex.IsMatch(slnContent, $@"\{{{Regex.Escape(projectGuid)}}}"))
            {
                // 如果該 ProjectGuid 不再被其他地方使用，則刪除 ProjectConfigurationPlatforms 配置
                string pattern = $@"\s*{Regex.Escape(projectGuid)}\.[^\n]*ActiveCfg\s*=[^\n]*[\s\S]*?\s*{Regex.Escape(projectGuid)}\.[^\n]*Build.0\s*=[^\n]*";

                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                slnContent = regex.Replace(slnContent, "");
            }

            if (!Regex.IsMatch(slnContent, $@"\{{{Regex.Escape(refGuid)}}}"))
            {
                // 如果該 ProjectGuid 不再被其他地方使用，則刪除 ProjectConfigurationPlatforms 配置
                string pattern = $@"\s*{Regex.Escape(refGuid)}\.[^\n]*ActiveCfg\s*=[^\n]*[\s\S]*?\s*{Regex.Escape(refGuid)}\.[^\n]*Build.0\s*=[^\n]*";

                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                slnContent = regex.Replace(slnContent, "");
            }

            RemoveFromNestedProjects(ref slnContent, refGuid);

            // 移除多餘的空白行
            slnContent = Regex.Replace(slnContent, @"^\s*\r?\n", "", RegexOptions.Multiline);

            // 將修改後的內容寫回 .sln 檔案
            File.WriteAllText(_slnPath, slnContent, Encoding.UTF8);           
        }

        public void AddNestedProject(ref string slnContent, string childGuid, string parentGuid)
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

    }
}
