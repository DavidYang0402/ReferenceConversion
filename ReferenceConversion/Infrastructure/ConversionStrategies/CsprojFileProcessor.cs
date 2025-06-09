using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ReferenceConversion.Domain.Enum;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConversion.Shared;
using static ReferenceConversion.Presentation.Form1;

namespace ReferenceConversion.Infrastructure.ConversionStrategies
{
    public class CsprojFileProcessor
    {
        private readonly IStrategyBasedConverter _converter;

        public CsprojFileProcessor(IStrategyBasedConverter converter)
        {
            _converter = converter;
        }

        public bool ProcessFile(string csprojFile, ReferenceConversionMode mode, string slnFilePath) 
        {
            try
            {
                // 讀檔並檢查開頭
                string content = File.ReadAllText(csprojFile);
                var trimmed = content.TrimStart();
                if (string.IsNullOrWhiteSpace(trimmed) || !trimmed.StartsWith("<"))
                {
                    Logger.LogInfo($"[跳過] 檔案不是有效 XML: {csprojFile}");
                    return false;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(csprojFile);

                bool isChanged = false;
                var processed = new HashSet<string>();
                //HashSet<string> processedReferences = new HashSet<string>();

                switch (mode)
                {
                    case ReferenceConversionMode.ProjectToDll:
                        isChanged |= _converter.ConvertProjectReferenceToReference(xmlDoc, processed, slnFilePath);
                        break;
                    case ReferenceConversionMode.DllToProject:
                        isChanged |= _converter.ConvertReferenceToProjectReference(xmlDoc, processed, slnFilePath, csprojFile);
                        break;
                }

                if (isChanged) xmlDoc.Save(csprojFile);

                return isChanged;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error processing file {csprojFile}: {ex.Message}");
                return false;
            }
        }
    }   
}
