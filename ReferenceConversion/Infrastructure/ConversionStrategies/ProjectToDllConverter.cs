using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ReferenceConversion.Applications.Interfaces;
using ReferenceConversion.Data;
using ReferenceConversion.Domain.Enum;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Infrastructure.Services;
using ReferenceConversion.Modifier;

namespace ReferenceConversion.Infrastructure.ConversionStrategies
{
    public class ProjectToDllConverter : IReferenceConversionStrategy
    {
        public ReferenceConversionMode Mode => ReferenceConversionMode.ProjectToDll;

        private readonly IAllowlistManager _allowlistManager;
        private readonly Func<string, ISlnModifier> _slnModifierFactory;

        public ProjectToDllConverter(IAllowlistManager allowlistManager, Func<string, ISlnModifier> slnModifierFactory)
        {
            _allowlistManager = allowlistManager;
            _slnModifierFactory = slnModifierFactory;
        }

        public bool Convert(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath)
        {
            bool isChanged = false;
            var referenceNodes = xmlDoc.GetElementsByTagName("ProjectReference").Cast<XmlNode>().ToList();
            var nodesToRemove = new List<XmlNode>();
            var slnModifier = _slnModifierFactory(slnFilePath);

            foreach (XmlNode node in referenceNodes)
            {
                if (node.Attributes?["Include"] is not XmlAttribute includeAttr) continue;
                string referenceName = Path.GetFileNameWithoutExtension(includeAttr.Value);
                if (string.IsNullOrEmpty(referenceName) || processedReferences.Contains(referenceName)) continue;

                if (_allowlistManager.IsInAllowlist(referenceName, out var project, out var entry))
                {
                    var mainCsproj =  IsMainProject(slnFilePath, project.ProjectName);

                    //Debug.WriteLine($"show current main csproj: {mainCsproj}");

                    var newElement = xmlDoc.CreateElement("Reference");
                    string dllPath = Path.Combine(project.DllPath, $"{referenceName}.dll");

                    newElement.SetAttribute("Include", $"{entry.Name}, Version={entry.Version}, Culture=neutral, processorArchitecture=MSIL");
                    //newElement.SetAttribute("Include", $"{entry.Name}");

                    var specificVersion = xmlDoc.CreateElement("SpecificVersion");
                    specificVersion.InnerText = "False";
                    newElement.AppendChild(specificVersion);

                    var hintPath = xmlDoc.CreateElement("HintPath");
                    hintPath.InnerText = dllPath;
                    newElement.AppendChild(hintPath);

                    slnModifier.RemoveProjectReferenceFromSln(entry.Name, entry.Guid, project.ProjectGuid);

                    node.ParentNode?.AppendChild(newElement);
                    nodesToRemove.Add(node);
                    processedReferences.Add(referenceName);
                    isChanged = true;
                }
            }

            foreach (XmlNode node in nodesToRemove)
                node.ParentNode?.RemoveChild(node);

            return isChanged;
        }


        private string IsMainProject(string slnPath, string proName)
        {

            var slnDir = Path.GetDirectoryName(slnPath);
            var csprojFiles = Directory.EnumerateFiles(slnDir, "*.csproj", searchOption:SearchOption.AllDirectories);

            var mainCsproj = csprojFiles
                .FirstOrDefault(path => 
                Path.GetFileNameWithoutExtension(path).Equals(proName, StringComparison.OrdinalIgnoreCase));

            if(csprojFiles != null)
            {
                Debug.WriteLine($"主專案找到啦！檔案路徑：{mainCsproj}");
            }
            else
            {
                Debug.WriteLine("沒找到主專案!");
            }

            return mainCsproj;
        }

        private void CopyMainDllToLibs(string csprojPath, string refName)
        {
            var csprojDir = Path.GetDirectoryName(csprojPath);
            var csprojFiles = Directory.EnumerateFiles(csprojDir, "*.dll", searchOption: SearchOption.AllDirectories);

            var allowDll = csprojFiles
                .FirstOrDefault(path =>
                Path.GetFileNameWithoutExtension(path).Equals(refName, StringComparison.OrdinalIgnoreCase));

            if (allowDll != null)
            {
                //// 確保 Libs 資料夾存在
                //Directory.CreateDirectory(libsTargetDir);

                //var targetPath = Path.Combine(libsTargetDir, Path.GetFileName(allowDll));

                //// 如果已經存在就覆蓋
                //File.Copy(allowDll, targetPath, overwrite: true);

                //Debug.WriteLine($"✅ 複製成功！{allowDll} → {targetPath}");
            }
            else
            {
                Debug.WriteLine($"⚠️ 找不到符合名稱 {refName} 的 DLL");
            }
        }

    }
}
