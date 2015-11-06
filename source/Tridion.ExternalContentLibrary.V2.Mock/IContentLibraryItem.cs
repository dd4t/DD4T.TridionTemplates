using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tridion.ExternalContentLibrary.V2
{
    public interface IContentLibraryItem
    {
        IEclUri Id { get; }
        string MetadataXml { get; set; }
        ISchemaDefinition MetadataXmlSchema { get; }
    }
}

