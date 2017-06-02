using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Dynamic = DD4T.ContentModel;
using Tridion.ContentManager.Templating;

namespace DD4T.Templates.Base.Contracts
{
    public interface IBinaryPathProvider
    {
        bool GetStripTcmUrisFromBinaryUrls(Component component);
        TcmUri GetTargetStructureGroupUri(Component component);
        TcmUri GetTargetStructureGroupUri(Dynamic.Component component);
        string ConstructPath(Component mmComp, string variantId, bool stripTcmUrisFromBinaryUrls, string targetStructureGroupUri);
    }
}
