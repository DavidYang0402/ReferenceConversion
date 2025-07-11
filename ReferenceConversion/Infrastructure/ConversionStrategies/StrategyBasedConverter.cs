using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ReferenceConversion.Data;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Domain.Enum;
using ReferenceConversion.Modifier;
using ReferenceConversion.Applications.Interfaces;

namespace ReferenceConversion.Infrastructure.ConversionStrategies
{
    public class StrategyBasedConverter : IStrategyBasedConverter
    {
        private readonly Dictionary<ReferenceConversionMode, IReferenceConversionStrategy> _strategies;

        public StrategyBasedConverter(IEnumerable<IReferenceConversionStrategy> strategies)
        {
            _strategies = new Dictionary<ReferenceConversionMode, IReferenceConversionStrategy>();

            foreach(var strategy in strategies)
            {
                _strategies[strategy.Mode] = strategy;
            }
        }

        public bool ConvertProjectReferenceToReference(XmlDocument xmlDoc, HashSet<string> processed, string slnFilePath, string csprojPath)
        {
            return _strategies[ReferenceConversionMode.ProjectToDll].Convert(xmlDoc, processed, slnFilePath, csprojPath);
        }

        public bool ConvertReferenceToProjectReference(XmlDocument xmlDoc, HashSet<string> processed, string slnFilePath, string csprojPath)
        {
            return _strategies[ReferenceConversionMode.DllToProject].Convert(xmlDoc, processed, slnFilePath, csprojPath);
        }
    }
}
