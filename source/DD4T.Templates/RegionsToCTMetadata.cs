#if REGIONS
using DD4T.Templates.Base;
using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using Tridion.ContentManager.CommunicationManagement.Regions;
using Tridion.ContentManager.ContentManagement.Fields;
using System;
using System.Xml;

namespace DD4T.Templates
{
    [TcmTemplateTitle("Convert regions to CT metadata")]
    public class RegionsToCTMetadata : BasePageTemplate
    {
        public RegionsToCTMetadata() : base(TemplatingLogger.GetLogger(typeof(RegionsToCTMetadata)))
        {
        }

        protected override void TransformPage(Dynamic.Page page)
        {
            var tcmPage = GetTcmPage();
            ProcessRegions(tcmPage.Regions, page, 0);
        }

        private void ProcessRegions(IList<IRegion> regions, Dynamic.Page dd4tPage, int fieldCounter)
        {
            int i = fieldCounter;
            Log.Debug($"ProcessRegions {regions.Count()}, {dd4tPage.ComponentPresentations.Count()}, {fieldCounter}");
            foreach (var region in regions)
            {
                if (region.ComponentPresentations != null && region.ComponentPresentations.Any())
                {
                    foreach (var cp in region.ComponentPresentations)
                    {
                        Dynamic.Component component = Manager.BuildComponent(cp.Component);
                        Dynamic.ComponentTemplate componentTemplate = Manager.BuildComponentTemplate(cp.ComponentTemplate);
                        Dynamic.ComponentPresentation componentPresentation = new Dynamic.ComponentPresentation() { Component = component, ComponentTemplate = componentTemplate, IsDynamic = cp.ComponentTemplate.IsRepositoryPublishable };
                        dd4tPage.ComponentPresentations.Add(componentPresentation);
                        if (componentTemplate.MetadataFields == null)
                        {
                            componentTemplate.MetadataFields = new Dynamic.FieldSet();
                        }
                        if (componentTemplate.MetadataFields.ContainsKey("region"))
                        {
                            componentTemplate.MetadataFields["region"].Values.Clear();
                            componentTemplate.MetadataFields["region"].Values.Add(region.RegionSchema.Title);
                        }
                        else
                        {
                            componentTemplate.MetadataFields.Add("region", new Dynamic.Field()
                            {
                                Name = "region",
                                FieldType = Dynamic.FieldType.Text,
                                Values = new List<string>()
                            {
                                region.RegionName
                            }
                            });
                        }
                        // remove regionView metadata field, because it is no longer possible to manage this information in Tridion 9 with Regions
                        if (componentTemplate.MetadataFields.ContainsKey("regionView"))
                        {
                            componentTemplate.MetadataFields.Remove("regionView");
                        }
                        // copy all other metadata fields from the region to the entity
                        if (region.Metadata != null)
                        {

                            ItemFields regionMetadataFields = new ItemFields(region.Metadata, region.RegionSchema);
                            try
                            {
                                foreach (var regionMeta in regionMetadataFields)
                                {
                                    componentTemplate.MetadataFields.Add(regionMeta.Name, Manager.BuildField(regionMeta, 1));
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Warning("error while trying to copy metadata from region to component template\r\n" + e.Message + "\r\n" + e.StackTrace);
                            }
                        }

                        if (region.Regions != null && region.RegionName.Any())
                        {
                            ProcessRegions(region.Regions, dd4tPage, i);
                        }
                    }
                }
            }
        }
    }
}
#endif