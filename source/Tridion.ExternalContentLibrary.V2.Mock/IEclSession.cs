﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tridion.ExternalContentLibrary.V2
{
    public interface IEclSession : IDisposable
    {
        IEclUri TryGetEclUriFromTcmUri(string uri);
        IContentLibraryContext GetContentLibrary(IEclUri eclUri);
    }
}
