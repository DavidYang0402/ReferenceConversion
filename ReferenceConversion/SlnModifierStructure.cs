using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace ReferenceConversion
{
    public class SlnModifierStructure
    {
        private string _slnPath;
        private SolutionFile _slnFile;

        public SlnModifierStructure(string slnPath)
        {
            _slnPath = slnPath;
            MSBuildLocator.RegisterDefaults();
            _slnFile = SolutionFile.Parse(_slnPath);
        }

        public void AddProjectReference(string projectName, string projectPath, string projectGuid, string refGuid)
        {
            //檢查專案是否已存在 .sln
            var project = _slnFile.ProjectsInOrder.FirstOrDefault(p => p.ProjectGuid == refGuid);
        }
    }
}
