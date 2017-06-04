using DD4T.Templates.Base.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamic = DD4T.ContentModel;
using Tridion.ContentManager.Templating;
using System.Text.RegularExpressions;
using Tridion.ContentManager.ContentManagement;
using System.IO;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using DD4T.Templates.Base.Utils;

namespace DD4T.Templates.Base.Providers
{
    public abstract class BaseBinaryPathProvider : IBinaryPathProvider
    {
        protected TemplatingLogger log = TemplatingLogger.GetLogger(typeof(BaseBinaryPathProvider));
        private Engine engine;
        private Package package;
        private TcmUri targetStructureGroupUri;
        private bool stripTcmUrisFromBinaryUrls;

        public BaseBinaryPathProvider(Engine engine, Package package)
        {
            this.engine = engine;
            this.package = package;

            String targetStructureGroupParam = package.GetValue("sg_PublishBinariesTargetStructureGroup");
            if (targetStructureGroupParam != null)
            {
                if (!TcmUri.IsValid(targetStructureGroupParam))
                {
                    log.Error(String.Format("TargetStructureGroup '{0}' is not a valid TCMURI. Exiting template.", targetStructureGroupParam));
                    return;
                }

                Publication publication = TridionUtils.GetPublicationFromContext(package, engine);
                TcmUri localTargetStructureGroupTcmUri = TridionUtils.GetLocalUri(new TcmUri(publication.Id), new TcmUri(targetStructureGroupParam));
                targetStructureGroupUri = new TcmUri(localTargetStructureGroupTcmUri);
                log.Debug($"targetStructureGroupUri = {targetStructureGroupUri.ToString()}");
            }

            String stripTcmUrisFromBinaryUrlsParam = package.GetValue("stripTcmUrisFromBinaryUrls");
            if (stripTcmUrisFromBinaryUrlsParam != null)
            {
                stripTcmUrisFromBinaryUrls = stripTcmUrisFromBinaryUrlsParam.ToLower() == "yes" || stripTcmUrisFromBinaryUrlsParam.ToLower() == "true";
            }
            log.Debug($"stripTcmUrisFromBinaryUrls = {stripTcmUrisFromBinaryUrls}");

        }
        public virtual string ConstructFileName(Component mmComp, string variantId, bool stripTcmUrisFromBinaryUrls, string targetStructureGroupUri)
        {
            log.Debug($"called ConstructPath with {stripTcmUrisFromBinaryUrls} and {targetStructureGroupUri}");
            Regex re = new Regex(@"^(.*)\.([^\.]+)$");
            string fileName = mmComp.BinaryContent.Filename;
            if (!String.IsNullOrEmpty(fileName))
            {
                fileName = Path.GetFileName(fileName);
            }
            if (stripTcmUrisFromBinaryUrls)
            {
                log.Debug("about to return " + fileName);
                return fileName;
            }
            return re.Replace(fileName, string.Format("$1_{0}_{1}.$2", mmComp.Id.ToString().Replace(":", ""), variantId.Replace(":", "")));

        }

        public virtual bool GetStripTcmUrisFromBinaryUrls(Component component)
        {
            return stripTcmUrisFromBinaryUrls;
        }


        public virtual TcmUri GetTargetStructureGroupUri(Dynamic.Component component)
        {
            return targetStructureGroupUri;
        }

        public virtual TcmUri GetTargetStructureGroupUri(Component component)
        {
            return targetStructureGroupUri;
        }
    }
}
