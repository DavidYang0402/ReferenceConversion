using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ReferenceConversion.Domain.Enum;

namespace ReferenceConversion.Applications.Interfaces
{
    public interface IReferenceConversionStrategy
    {
        ReferenceConversionMode Mode { get; }
        bool Convert(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath)
            => Convert(xmlDoc, processedReferences, slnFilePath, null);
        bool Convert(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath, string? csprojPath);
    }
}
