using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConversion.Domain.Interfaces
{
    public interface IDllCopier
    {
        void Copy(string slnPath, string proName, string refName, string libsTargetDir);
    }
}
