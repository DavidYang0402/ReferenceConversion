using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ReferenceConversion.Applications.Interfaces;
using ReferenceConversion.Data;
using ReferenceConversion.Domain.Enum;
using ReferenceConversion.Modifier;

namespace ReferenceConversion.Infrastructure.ConversionStrategies
{
    public class ProjectToDllConverter : IReferenceConversionStrategy
    {
        public ReferenceConversionMode Mode => ReferenceConversionMode.ProjectToDll;

        private readonly AllowlistManager _allowlistManager;
        private readonly Func<string, ISlnModifier> _slnModifierFactory;

        public ProjectToDllConverter(AllowlistManager allowlistManager, Func<string, ISlnModifier> slnModifierFactory)
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
                    var newElement = xmlDoc.CreateElement("Reference");
                    string dllPath = Path.Combine(project.DllPath, $"{referenceName}.dll");

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
                }
            }

            foreach (XmlNode node in nodesToRemove)
                node.ParentNode?.RemoveChild(node);

            return isChanged;
        }
    }
}
