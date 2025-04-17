using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConversion.Modifier
{
    public interface ISlnModifier
    {
        void AddProjectReferenceToSln(string projectName, string projectPath, string projectGuid, string refGuid, string parentGuid);
        void RemoveProjectReferenceFromSln(string projectName, string refGuid, string projectGuid);
    }
}
