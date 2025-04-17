using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConversion.Applications.Interfaces
{
    public interface IReferenceConvertionService
    {
        Task ConvertAllToReference(string solutionPath);
        Task ConvertAllToProjectReference(string solutionPath);
    }
}
