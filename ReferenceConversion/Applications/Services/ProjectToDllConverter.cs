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

                    _dllCopier.Copy(slnFilePath, referenceName, project.DllPath);
                }
            }

            foreach (XmlNode node in nodesToRemove)
                node.ParentNode?.RemoveChild(node);

            return isChanged;
        }      
    }
}
