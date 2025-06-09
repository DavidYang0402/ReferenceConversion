using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReferenceConversion.Domain.Interfaces
{
    public interface IStrategyBasedConverter
    {
        bool ConvertProjectReferenceToReference(XmlDocument xmlDoc, HashSet<string> processed, string slnFilePath);
        bool ConvertReferenceToProjectReference(XmlDocument xmlDoc, HashSet<string> processed, string slnFilePath, string csprojPath);
    }
}
