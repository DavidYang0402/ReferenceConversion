using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ReferenceConversion.Modifier;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Applications.Interfaces;
using ReferenceConversion.Domain.Enum;
using ReferenceConversion.Infrastructure.Services;
using ReferenceConversion.Shared;

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

                    string relativePath = Path.Combine(
                        (!isShareUserCtrl)
                          ? Enumerable.Repeat("..", entry.CsprojDepth).ToArray()
                          : Enumerable.Repeat("..", 4).ToArray()
                      );

                    relativePath = Path.Combine(relativePath, entry.Path);

                    var newElement = xmlDoc.CreateElement("ProjectReference");
                    newElement.SetAttribute("Include", relativePath);

                    var projectGuidElement = xmlDoc.CreateElement("Project");
                    projectGuidElement.InnerText = entry.Guid;
                    newElement.AppendChild(projectGuidElement);

                    var nameElement = xmlDoc.CreateElement("Name");
                    nameElement.InnerText = entry.Name;
                    newElement.AppendChild(nameElement);

                    string slnRelativePath = Path.Combine(Enumerable.Repeat("..", entry.SlnDepth).ToArray());

                    slnRelativePath = Path.Combine(slnRelativePath, entry.Path);

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
    }
}
