using System;
using System.Collections.Generic;
using System.Text;
using Dynamic = DD4T.ContentModel;
using TCM = Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using DD4T.ContentModel.Exceptions;
using DD4T.Templates.Base.Utils;

namespace DD4T.Templates.Base.Builder
{
    public class RegionBuilder
    {
#if REGIONS

        public static Dynamic.Region BuildRegion(TCM.Regions.IRegion tcmRegion, Engine engine, BuildManager manager)
        {
            return BuildRegion(tcmRegion, engine, manager, 1, false,false);
        }
        public static Dynamic.Region BuildRegion(TCM.Regions.IRegion tcmRegion, Engine engine, BuildManager manager, int linkLevels, bool resolveWidthAndHeight,bool publishEmptyFields)
        {
            Dynamic.Region r = new Dynamic.Region
                                 {
                                     Name = tcmRegion.RegionName,
                                     Schema = manager.BuildSchema(tcmRegion.RegionSchema),
                                     MetadataFields = new Dynamic.FieldSet()
                                 };
            if (linkLevels > 0)
            {
                try
                {
                    if (tcmRegion.Metadata != null && tcmRegion.RegionSchema != null)
                    {
                        var tcmMetadataFields = new Tridion.ContentManager.ContentManagement.Fields.ItemFields(tcmRegion.Metadata, tcmRegion.RegionSchema);
                        r.MetadataFields = manager.BuildFields(tcmMetadataFields);
                    }
                }
                catch (Exception)
                {
                    // fail silently if there is no metadata schema
                }
            }

            r.ComponentPresentations = new List<Dynamic.ComponentPresentation>();
            foreach (TCM.ComponentPresentation cp in tcmRegion.ComponentPresentations)
            {
                Dynamic.ComponentPresentation dynCp = manager.BuildComponentPresentation(cp, engine, linkLevels - 1, resolveWidthAndHeight);
                r.ComponentPresentations.Add(dynCp);
            }

            manager.AddXpathToFields(r.MetadataFields, "Metadata");

            // adding nested regions
            r.Regions = new List<Dynamic.Region>();
            foreach (TCM.Regions.IRegion nestedTcmRegion in tcmRegion.Regions)
            {
                Dynamic.Region nestedRegion = manager.BuildRegion(nestedTcmRegion, engine);
                r.Regions.Add(nestedRegion);
            }

            return r;
        }
#endif
    }
}
