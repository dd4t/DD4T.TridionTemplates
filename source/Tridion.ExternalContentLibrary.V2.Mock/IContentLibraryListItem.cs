using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tridion.ExternalContentLibrary.V2
{
    public interface IContentLibraryListItem
    {
        bool CanGetUploadMultimediaItemsUrl { get; }
        bool CanSearch { get; }
        string DisplayTypeId { get; }
        string IconIdentifier { get; }
        bool IsThumbnailAvailable { get; }
        DateTime? Modified { get; }
        string ThumbnailETag { get; }
        string Title { get; set; }
    }
}
