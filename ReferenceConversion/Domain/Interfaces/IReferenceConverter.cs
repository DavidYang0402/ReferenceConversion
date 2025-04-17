using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReferenceConversion.Domain.Interfaces
{
    public interface IReferenceConverter
    {
        bool ConvertProjectReferenceToReference(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath);
        bool ConvertReferenceToProjectReference(XmlDocument xmlDoc, HashSet<string> processedReferences, string slnFilePath);
    }
}
