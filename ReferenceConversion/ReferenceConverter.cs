using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ReferenceConversion.Data;

namespace ReferenceConversion
{
    public class ReferenceConverter
    {
        private AllowlistManager _allowlistManager;

        public ReferenceConverter(AllowlistManager allowlistManager)
        {
            _allowlistManager = allowlistManager;
        }

        public bool ConvertProjectReferenceToReference(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath)
        {
            bool isChanged = false;
            XmlNodeList projectReferences = xmlDoc.GetElementsByTagName("ProjectReference");
            List<XmlNode> nodesToRemove = new List<XmlNode>();

            if (projectReferences.Count > 0)
            {
                foreach (XmlNode node in projectReferences)
                {
                    var includeAttr = node.Attributes?["Include"];
                    if (includeAttr == null) continue;

                    string referenceName = Path.GetFileNameWithoutExtension(includeAttr.Value);
                    //string referenceName = Path.GetFileNameWithoutExtension(node.Attributes["Include"].Value);

                    // 若已處理過此項目則跳過
                    if (processedReferences.Contains(referenceName)) continue;

                    if (_allowlistManager.IsInAllowlist(referenceName, out var project, out var entry))
                    {
                        string dllPath = Path.Combine(project.DllPath, $"{referenceName}.dll");

                        XmlElement reference = xmlDoc.CreateElement("Reference");
                        reference.SetAttribute("Include", $"{entry.Name}, Version={entry.Version}, Culture=neutral, processorArchitecture=MSIL");

                        XmlElement specificVersion = xmlDoc.CreateElement("SpecificVersion");
                        specificVersion.InnerText = "False";
                        reference.AppendChild(specificVersion);

                        XmlElement hintPath = xmlDoc.CreateElement("HintPath");
                        hintPath.InnerText = dllPath;
                        //hintPath.InnerText = Path.Combine("App_Data", $"{referenceName}.dll");
                        reference.AppendChild(hintPath);

                        node.ParentNode.AppendChild(reference);
                        nodesToRemove.Add(node);
                        processedReferences.Add(referenceName);  // 標記為已處理
                        isChanged = true;

                        SlnModifier slnModifier = new SlnModifier(slnFilePath);
                        slnModifier.RemoveProjectReferenceFromSln(entry.Name, entry.Guid, project.ProjectGuid);
                    }
                }
            }

            // 刪除 ProjectReference 節點
            foreach (XmlNode node in nodesToRemove)
            {
                node.ParentNode.RemoveChild(node);
            }

            return isChanged;
        }

        public bool ConvertReferenceToProjectReference(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath)
        {
            bool isChanged = false;
            XmlNodeList references = xmlDoc.GetElementsByTagName("Reference");
            List<XmlNode> nodesToRemove = new List<XmlNode>();

            if (references.Count > 0)
            {
                foreach (XmlNode node in references)
                {
                    var includeAttr = node.Attributes?["Include"];
                    if (includeAttr == null) continue;

                    string referenceAttr = includeAttr.Value;
                    if (string.IsNullOrEmpty(referenceAttr)) continue;

                    string referenceName = referenceAttr.Split(',')[0];

                    // 若已處理過此項目則跳過
                    if (processedReferences.Contains(referenceName)) continue;
                   
                    if (_allowlistManager.IsInAllowlist(referenceName, out var project, out var entry))
                    {
                        string relativePath = Path.Combine("..", "..", "..", entry.Path);

                        XmlElement projectReference = xmlDoc.CreateElement("ProjectReference");
                        projectReference.SetAttribute("Include", relativePath);

                        XmlElement projectGuidElement = xmlDoc.CreateElement("Project");
                        projectGuidElement.InnerText = project.ProjectGuid;
                        projectReference.AppendChild(projectGuidElement);

                        XmlElement nameElement = xmlDoc.CreateElement("Name");
                        nameElement.InnerText = entry.Name;
                        projectReference.AppendChild(nameElement);

                        node.ParentNode.AppendChild(projectReference);
                        nodesToRemove.Add(node);
                        processedReferences.Add(entry.Name);  // 標記為已處理
                        isChanged = true;

                        // 提供給 .sln 的路徑
                        string slnRelativePath = Path.Combine("..", "..", entry.Path);                   

                        SlnModifier slnModifier = new SlnModifier(slnFilePath);
                        slnModifier.AddProjectReferenceToSln(entry.Name, slnRelativePath, project.ProjectGuid, entry.Guid, entry.ParentGuid);
                    }
                }
            }

            // 刪除 Reference 節點
            foreach (XmlNode node in nodesToRemove)
            {
                node.ParentNode.RemoveChild(node);
            }

            return isChanged;
        }     
    }
}
