using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tridion.ExternalContentLibrary.V2
{
    public interface IContentLibraryContext : IDisposable
    {
        List<object> GetItems(IEclUri[] eclUris);
    }
}
