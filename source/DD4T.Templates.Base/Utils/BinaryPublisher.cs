﻿using System;
using System.IO;
using System.Xml;
using System.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using DD4T.Templates.Base.Utils;
using DD4T.Templates.Base.Xml;
using System.Text.RegularExpressions;
using DD4T.Templates.Base.Builder;
using Tridion.ExternalContentLibrary.V2;


namespace DD4T.Templates.Base.Utils
{
    public class BinaryPublisher
    {
        protected TemplatingLogger log = TemplatingLogger.GetLogger(typeof(BinaryPublisher));
        TcmUri targetStructureGroupUri = null;
        protected Package package;
        protected Engine engine;
        Template currentTemplate;

        public BinaryPublisher(Package package, Engine engine)
            : this(package, engine, null)
        {
        }

        public BinaryPublisher(Package package, Engine engine, string targetStructureGroup)
        {

            this.package = package;
            this.engine = engine;

            currentTemplate = engine.PublishingContext.ResolvedItem.Template;

            // Determine (optional) structure group parameter

            String targetStructureGroupParam = targetStructureGroup == null ? package.GetValue("sg_PublishBinariesTargetStructureGroup") : targetStructureGroup;
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
            }
        }

        #region Protected Members

        public virtual string PublishBinariesInRichTextField(string xhtml, BuildProperties buildProperties)
        {

            // rich text field is well-formed, except that it does not always have a root element
            // to be sure it can be parsed, we will add a root and remove it afterwards

            TridionXml xml = new TridionXml();
            xml.LoadXml("<tmproot>" + xhtml + "</tmproot>");

            foreach (XmlElement elmt in xml.SelectNodes("//*[@xlink:href[starts-with(string(.),'tcm:')]]", xml.NamespaceManager))
            {
                log.Debug("found node " + elmt.OuterXml);
                XmlAttribute link = (XmlAttribute)elmt.SelectSingleNode("@xlink:href", xml.NamespaceManager);
                log.Debug("about to publish binary with uri " + link.Value);
                string path = PublishMultimediaComponent(link.Value, buildProperties);
                log.Debug("binary will be published to path " + path);

                if (elmt.LocalName.ToLower() == "img")
                {
                    elmt.SetAttribute("src", path);
                }
                else
                {
                    elmt.SetAttribute("href", path);
                }
                elmt.RemoveAttributeNode(link);
            } 
            return xml.DocumentElement.InnerXml;
        }

        public virtual string PublishMultimediaComponent(string uri, BuildProperties buildProperties)
        {
            if (buildProperties.ECLEnabled)
            {
                string url = GetECLUrl(uri);
                if (! string.IsNullOrEmpty(url))
                {
                    return url;
                }
            }
            string itemName = "PublishMultimedia_" + uri;
            TcmUri tcmuri = new TcmUri(uri);
            Item mmItem = package.GetByName(itemName);
            if (mmItem == null)
            {
                mmItem = package.CreateMultimediaItem(tcmuri);
                package.PushItem(itemName, mmItem);
                log.Debug(string.Format("Image {0} ({1}) unique, adding to package", itemName, uri));
                if (!mmItem.Properties.ContainsKey(Item.ItemPropertyPublishedPath))
                {
                    log.Debug(string.Format("Publish Image {0} ({1}).", itemName, uri));
                    PublishItem(mmItem, tcmuri);
                }
            }
            else
            {
                log.Debug(string.Format("Image {0} ({1}) already present in package, not adding again", itemName, tcmuri));
            }
            return GetReferencePath(mmItem, uri);
        }

        protected virtual string GetECLUrl(string uri)
        {
            Component c = (Component)engine.GetObject(uri);
            if (c == null)
            {
                throw new Exception(string.Format("Error loading item with uri {0}", uri));
            }

            log.Debug(System.Threading.Thread.CurrentThread.ManagedThreadId + ": Instantiating a new ECL Session");
            using (IEclSession localSession = SessionFactory.CreateEclSession(engine.GetSession()))
            {
                IEclUri eclUri = localSession.TryGetEclUriFromTcmUri(uri);
                if (eclUri != null) // this is an ECL item
                {
                    log.Debug(System.Threading.Thread.CurrentThread.ManagedThreadId + ": Fetching IContentLibraryContext");
                    using (IContentLibraryContext context = localSession.GetContentLibrary(eclUri))
                    {
                        try
                        {
                            IContentLibraryMultimediaItem item = null;

                            // This is done this way to not have an exception thrown through GetItem, as stated
                            // in the API doc.
                            // The reason to do this, is because if there is an exception, 
                            // the ServiceChannel is going into the aborted state...
                            log.Debug(System.Threading.Thread.CurrentThread.ManagedThreadId + ": Get Items");

                            // GetItems allows up to 20 (depending on config) connections. 
                            // After that any new connection is aborted / not created.

                            var items = context.GetItems(new IEclUri[] { eclUri });

                            if (items != null && items.Count == 1)
                            {
                                item = (IContentLibraryMultimediaItem)items.First();
                                log.Debug(System.Threading.Thread.CurrentThread.ManagedThreadId + ": Item Fetched");
                            }

                            if (item == null)
                            {
                                log.Warning(System.Threading.Thread.CurrentThread.ManagedThreadId + ": Item with ECL URI: " + eclUri + " not found. This MM item is used in: " + c.Id);
                                throw new Exception(string.Format("ECL item not found (ecl uri = {0}, tcm uri = {1}", eclUri, c.Id));
                            }
                            string distributionUrl = item.GetDirectLinkToPublished(null);
                            string result = distributionUrl.ToLower();
                            if (!string.IsNullOrEmpty(result))
                            {
                                log.Debug(System.Threading.Thread.CurrentThread.ManagedThreadId + ": Returning: " + result);
                                return result;
                            }
                        }
                        finally
                        {
                            log.Debug(System.Threading.Thread.CurrentThread.ManagedThreadId + ": Going out of the context using block statement.");
                        }
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Return the reference path for the binary which has just been published. This path is stored in the XML which is published to the broker, and may be used in 
        /// the presentation engine to retrieve the binary. In this implementation, the reference path is the same as the publish path, but the method can be overridden 
        /// to implement other logic. It could (for example) return the path to the binary through a CDN.
        /// </summary>
        /// <param name="item">The templating Item containing the multimedia component (including the publish path)</param>
        /// <param name="uri">The uri of the multimedia component</param>
        /// <returns>The reference path that will be stored in the XML</returns>
        protected virtual string GetReferencePath(Item item, string uri)
        {
            return item.Properties[Item.ItemPropertyPublishedPath];
        }

        protected virtual void PublishItem(Item item, TcmUri itemUri)
        {
            Stream itemStream = null;
            // See if some template set itself as the applied template on this item
            TcmUri appliedTemplateUri = null;
            if (item.Properties.ContainsKey(Item.ItemPropertyTemplateUri))
            {
                appliedTemplateUri = new TcmUri(item.Properties[Item.ItemPropertyTemplateUri]);
            }

            try
            {
                string publishedPath;
                if (targetStructureGroupUri == null)
                {
                    log.Debug("no structure group defined, publishing binary with default settings");
                    Component mmComp = (Component)engine.GetObject(item.Properties[Item.ItemPropertyTcmUri]);
                    // Note: it is dangerous to specify the CT URI as variant ID without a structure group, because it will fail if you publish the same MMC from two or more CTs!
                    // So I removed the variant ID altogether (QS, 20-10-2011)
                    log.Debug(string.Format("publishing mm component {0} without variant id", mmComp.Id));
                    Binary binary = engine.PublishingContext.RenderedItem.AddBinary(mmComp);
                    publishedPath = binary.Url;
                    log.Debug(string.Format("binary is published to url {0}", publishedPath));
                }
                else
                {
                    Component mmComp = (Component)engine.GetObject(item.Properties[Item.ItemPropertyTcmUri]);

                    string fileName = ConstructFileName(mmComp, currentTemplate.Id);
                    StructureGroup targetSG = (StructureGroup)engine.GetObject(targetStructureGroupUri);
                    itemStream = item.GetAsStream();
                    if (itemStream == null)
                    {
                        // All items can be converted to a stream?
                        log.Error(String.Format("Cannot get item '{0}' as stream", itemUri.ToString()));
                    }
                    //byte[] data = new byte[itemStream.Length];
                    //itemStream.Read(data, 0, data.Length);
                    //itemStream.Close();
                    log.Debug(string.Format("publishing mm component {0} to structure group {1} with variant id {2} and filename {3}", mmComp.Id, targetStructureGroupUri.ToString(), currentTemplate.Id, fileName));
                    Binary b = engine.PublishingContext.RenderedItem.AddBinary(item.GetAsStream(), fileName, targetSG, currentTemplate.Id, mmComp, mmComp.BinaryContent.MultimediaType.MimeType);
                    publishedPath = b.Url;
                    //publishedPath = engine.AddBinary(itemUri, appliedTemplateUri, targetStructureGroupUri, data, fileName);
                    log.Debug(string.Format("binary is published to url {0}", publishedPath));
                }
                log.Debug("binary published, published path = " + publishedPath);
                item.Properties[Item.ItemPropertyPublishedPath] = publishedPath;
            }
            finally
            {
                if (itemStream != null) itemStream.Close();
            }
        }

        private static string ConstructFileName(Component mmComp, string variantId)
        {
            Regex re = new Regex(@"^(.*)\.([^\.]+)$");
            string fileName = mmComp.BinaryContent.Filename;
            if (!String.IsNullOrEmpty(fileName))
            {
                fileName = Path.GetFileName(fileName);
            }
            return re.Replace(fileName, string.Format("$1_{0}_{1}.$2", mmComp.Id.ToString().Replace(":", ""), variantId.Replace(":", "")));
        }
        #endregion

    }


}