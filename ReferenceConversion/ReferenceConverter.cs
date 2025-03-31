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

        public bool ConvertProjectReferenceToReference(XmlDocument xmlDoc, HashSet<string> processedReferences)
        {
            bool isChanged = false;
            XmlNodeList projectReferences = xmlDoc.GetElementsByTagName("ProjectReference");
            List<XmlNode> nodesToRemove = new List<XmlNode>();

            if (projectReferences.Count > 0)
            {
                foreach (XmlNode node in projectReferences)
                {
                    string referenceName = Path.GetFileNameWithoutExtension(node.Attributes["Include"].Value);

                    // 若已處理過此項目則跳過
                    if (processedReferences.Contains(referenceName)) continue;

                    if (_allowlistManager.IsInAllowlist(referenceName, out string version, out string guid))
                    {
                        XmlElement reference = xmlDoc.CreateElement("Reference");
                        reference.SetAttribute("Include", $"{referenceName}, Version={version}, Culture=neutral, processorArchitecture=MSIL");

                        XmlElement specificVersion = xmlDoc.CreateElement("SpecificVersion");
                        specificVersion.InnerText = "False";
                        reference.AppendChild(specificVersion);

                        XmlElement hintPath = xmlDoc.CreateElement("HintPath");
                        hintPath.InnerText = Path.Combine("App_Data", $"{referenceName}.dll");
                        reference.AppendChild(hintPath);

                        node.ParentNode.AppendChild(reference);
                        nodesToRemove.Add(node);
                        processedReferences.Add(referenceName);  // 標記為已處理
                        isChanged = true;
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

        public bool ConvertReferenceToProjectReference(XmlDocument xmlDoc, HashSet<string> processedReferences, string saveFolder)
        {
            bool isChanged = false;
            XmlNodeList references = xmlDoc.GetElementsByTagName("Reference");
            List<XmlNode> nodesToRemove = new List<XmlNode>();

            if (references.Count > 0)
            {
                foreach (XmlNode node in references)
                {
                    string referenceAttr = node.Attributes["Include"]?.Value;
                    if (string.IsNullOrEmpty(referenceAttr)) continue;

                    string referenceName = referenceAttr.Split(',')[0];

                    // 若已處理過此項目則跳過
                    if (processedReferences.Contains(referenceName)) continue;

                    if (_allowlistManager.IsInAllowlist(referenceName, out _, out string projectGuid))
                    {
                        string baseFolder = saveFolder;
                        //如果 baseFolder 是空的，顯示錯誤訊息並結束方法
                        if (string.IsNullOrEmpty(baseFolder))
                        {
                            baseFolder = "SysTools";
                        }

                        // 計算新的 Include 屬性
                        string relativePath = Path.Combine("..", "..", "..", baseFolder, referenceName, $"{referenceName}.csproj");

                        // 檢查路徑是否包含 "Share"，如果包含則修改為 ShareFunc 子資料夾
                        if (relativePath.Contains("Share"))
                        {
                            relativePath = Path.Combine("..", "..", "..", baseFolder, "ShareFunc\\ShareFunc\\", referenceName, $"{referenceName}.csproj");
                        }

                        XmlElement projectReference = xmlDoc.CreateElement("ProjectReference");
                        projectReference.SetAttribute("Include", relativePath);

                        XmlElement projectGuidElement = xmlDoc.CreateElement("Project");
                        projectGuidElement.InnerText = projectGuid;
                        projectReference.AppendChild(projectGuidElement);

                        XmlElement nameElement = xmlDoc.CreateElement("Name");
                        nameElement.InnerText = referenceName;
                        projectReference.AppendChild(nameElement);

                        node.ParentNode.AppendChild(projectReference);
                        nodesToRemove.Add(node);
                        processedReferences.Add(referenceName);  // 標記為已處理
                        isChanged = true;
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
