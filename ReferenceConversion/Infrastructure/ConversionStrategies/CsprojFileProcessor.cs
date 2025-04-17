using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ReferenceConversion.Domain.Interfaces;
using static ReferenceConversion.Presentation.Form1;

namespace ReferenceConversion.Infrastructure.ConversionStrategies
{
    public class CsprojFileProcessor
    {
        private readonly IReferenceConverter _converter;

        public CsprojFileProcessor(IReferenceConverter converter)
        {
            _converter = converter;
        }

        public bool ProcessFile(string csprojFile, ConversionType conversionType, string slnFilePath) 
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(csprojFile);

                bool isChanged = false;
                HashSet<string> processedReferences = new HashSet<string>();

                switch (conversionType)
                {
                    case ConversionType.ToReference:
                        isChanged |= _converter.ConvertProjectReferenceToReference(xmlDoc, processedReferences, slnFilePath);
                        break;
                    case ConversionType.ToProjectReference:
                        isChanged |= _converter.ConvertReferenceToProjectReference(xmlDoc, processedReferences, slnFilePath);
                        break;
                }

                if (isChanged)
                {
                    xmlDoc.Save(csprojFile);
                }

                return isChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing file {csprojFile}: {ex.Message}");
                return false;
            }
        }
    }   
}
