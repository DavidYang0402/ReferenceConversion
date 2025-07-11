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
using ReferenceConversion.Shared;

namespace ReferenceConversion.Applications.Services
{
    public class ProjectToDllConverter : IReferenceConversionStrategy
    {
        public ReferenceConversionMode Mode => ReferenceConversionMode.ProjectToDll;

        private readonly IAllowlistManager _allowlistManager;
        private readonly Func<string, ISlnModifier> _slnModifierFactory;
        private readonly IDllCopier _dllCopier;

        public ProjectToDllConverter(IAllowlistManager allowlistManager, Func<string, ISlnModifier> slnModifierFactory, IDllCopier dllCopier)
        {
            _allowlistManager = allowlistManager;
            _slnModifierFactory = slnModifierFactory;
            _dllCopier = dllCopier;
        }

        public bool Convert(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath, string csprojPath)
        {
            bool isChanged = false;
            var referenceNodes = xmlDoc.GetElementsByTagName("ProjectReference").Cast<XmlNode>().ToList();
            var nodesToRemove = new List<XmlNode>();
            var slnModifier = _slnModifierFactory(slnFilePath);

            Logger.LogInfo($"開始處理 SLN 檔案: {slnFilePath}, 進行轉成專案參考");

            Logger.LogSeparator();

            foreach (XmlNode node in referenceNodes)
            {
                if (node.Attributes?["Include"] is not XmlAttribute includeAttr) continue;
                string referenceName = Path.GetFileNameWithoutExtension(includeAttr.Value);
                if (string.IsNullOrEmpty(referenceName) || processedReferences.Contains(referenceName)) continue;

                Logger.LogInfo($"處理 ProjectReference: {referenceName}");

                if (_allowlistManager.IsInAllowlist(referenceName, out var project, out var entry))
                {
                    Logger.LogInfo($"找到允許的專案: {entry.Name}, 將轉換為 DLL");

                    var newElement = xmlDoc.CreateElement("Reference");

                    string dllPath = Path.Combine("..", project.DllPath, $"{referenceName}.dll");

                    string csprojProjectName = Path.GetFileNameWithoutExtension(csprojPath);
                    if (csprojProjectName == "ShareUserCtrl")
                    {
                        dllPath = Path.Combine("..", "..", entry.Path);
                    }


                    newElement.SetAttribute("Include", $"{entry.Name}, Version={entry.Version}, Culture=neutral, processorArchitecture=MSIL");

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

                    // Log DLL 複製
                    Logger.LogInfo($"開始複製 DLL: {referenceName}.dll 到 {dllPath}");
                    _dllCopier.Copy(slnFilePath, referenceName, project.DllPath, entry.Path);

                    Logger.LogSeparator();
                }
            }
            Logger.LogInfo($"結束處理 SLN 檔案: {slnFilePath}"); 
            Logger.LogSeparator();

            foreach (XmlNode node in nodesToRemove)
                node.ParentNode?.RemoveChild(node);

            return isChanged;
        }      
    }
}
