using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tridion.ExternalContentLibrary.V2
{
    public interface IFieldGroupDefinition
    {
        IList<IFieldDefinition> Fields { get; }
    }
}
