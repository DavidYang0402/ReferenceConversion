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

        public bool Convert(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath)
        {
            bool isChanged = false;
            var referenceNodes = xmlDoc.GetElementsByTagName("Reference").Cast<XmlNode>().ToList();
            var nodesToRemove = new List<XmlNode>();
            var slnModifier = _slnModifierFactory(slnFilePath);

            foreach (XmlNode node in referenceNodes)
            {
                if (node.Attributes?["Include"] is not XmlAttribute includeAttr) continue;
                string referenceName = includeAttr.Value.Split(',')[0];
                if (string.IsNullOrEmpty(referenceName) || processedReferences.Contains(referenceName)) continue;

                if (_allowlistManager.IsInAllowlist(referenceName, out var project, out var entry))
                {
                    string relativePath = Path.Combine("..", "..", "..", entry.Path);
                    var newElement = xmlDoc.CreateElement("ProjectReference");
                    newElement.SetAttribute("Include", relativePath);

                    var projectGuidElement = xmlDoc.CreateElement("Project");
                    projectGuidElement.InnerText = project.ProjectGuid;
                    newElement.AppendChild(projectGuidElement);

                    var nameElement = xmlDoc.CreateElement("Name");
                    nameElement.InnerText = entry.Name;
                    newElement.AppendChild(nameElement);

                    string slnRelativePath = Path.Combine("..", "..", entry.Path);
                    slnModifier.AddProjectReferenceToSln(entry.Name, slnRelativePath, project.ProjectGuid, entry.Guid, entry.ParentGuid);

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
