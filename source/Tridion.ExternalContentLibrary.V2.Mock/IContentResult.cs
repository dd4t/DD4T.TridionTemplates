using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tridion.ExternalContentLibrary.V2
{
    public interface IContentResult
    {
        string ContentType { get; set; }
        System.IO.Stream Stream { get; set; }
    }
}
