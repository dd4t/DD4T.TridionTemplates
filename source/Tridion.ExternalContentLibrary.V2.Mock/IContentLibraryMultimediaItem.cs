using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tridion.ExternalContentLibrary.V2
{
    public interface IContentLibraryMultimediaItem : IContentLibraryItem, IContentLibraryListItem
    {
        string Filename { get; }
        int? Height { get; }
        string MimeType { get; }
        int? Width { get; }

        string GetDirectLinkToPublished(object o); 
        string GetTemplateFragment(IList<ITemplateAttribute> attributes);
    }
}
