using System;
using System.IO;
using System.Drawing;
using Tridion.ContentManager.Templating;
using TCM = Tridion.ContentManager.ContentManagement;
using DD4T.ContentModel;
using DD4T.Templates.Base.Utils;
using Dynamic = DD4T.ContentModel;

namespace DD4T.Templates.Base.Builder
{
   public class ComponentBuilder
   {
      private static TemplatingLogger log = TemplatingLogger.GetLogger(typeof(ComponentBuilder));
      public static Dynamic.Component BuildComponent(TCM.Component tcmComponent, BuildManager manager)
      {
          return BuildComponent(tcmComponent, manager.BuildProperties.LinkLevels, manager);
      }

      public static Dynamic.Component BuildComponent(TCM.Component tcmComponent, int currentLinkLevel, BuildManager manager)
      {
          log.Debug(string.Format("start BuildComponent with component {0} ({1}) and link level {2}", tcmComponent.Title, tcmComponent.Id, currentLinkLevel));
          Dynamic.Component c = new Dynamic.Component();
          c.Title = tcmComponent.Title;
          c.Id = tcmComponent.Id.ToString();
          c.RevisionDate = tcmComponent.RevisionDate;

          c.Version = tcmComponent.Version;
          c.Schema = manager.BuildSchema(tcmComponent.Schema);
          
          c.ComponentType = (ComponentType)Enum.Parse(typeof(ComponentType), tcmComponent.ComponentType.ToString());

          if (tcmComponent.ComponentType.Equals(TCM.ComponentType.Multimedia))
          {
              Multimedia multimedia = new Multimedia();
              multimedia.MimeType = tcmComponent.BinaryContent.MultimediaType.MimeType;
              multimedia.Size = tcmComponent.BinaryContent.Size;
              multimedia.FileName = tcmComponent.BinaryContent.Filename;

              string extension = System.IO.Path.GetExtension(multimedia.FileName);
              if (string.IsNullOrEmpty(extension))
              {
                  multimedia.FileExtension = "";
              }
              else
              {
                  // remove leading dot from extension because microsoft returns this as ".gif"
                  multimedia.FileExtension = extension.Substring(1);
              }

              if (manager.BuildProperties.ResolveWidthAndHeight)
              {
                  try
                  {
                      MemoryStream memstream = new MemoryStream();
                      tcmComponent.BinaryContent.WriteToStream(memstream);
                      Image image = Image.FromStream(memstream);
                      memstream.Close();

                      multimedia.Width = image.Size.Width;
                      multimedia.Height = image.Size.Height;
                  }
                  catch (Exception e)
                  {
                      log.Warning(string.Format("error retrieving width and height of image: is component with ID {0} really an image? Error message: {1}", c.Id, e.Message));
                      multimedia.Width = 0;
                      multimedia.Height = 0;
                  }
              }
              else
              {
                  multimedia.Width = 0;
                  multimedia.Height = 0;
              }
              c.Multimedia = multimedia;
              c.Multimedia.Url = manager.PublishMultimediaComponent(c);
          }
          else
          {
              c.Multimedia = null;
          }
          c.Fields = new Dynamic.FieldSet();
          c.MetadataFields = new Dynamic.FieldSet();
          if (currentLinkLevel > 0)
          {
              if (tcmComponent.Content != null)
              {
                  TCM.Fields.ItemFields tcmFields = new TCM.Fields.ItemFields(tcmComponent.Content, tcmComponent.Schema);
                  c.Fields = manager.BuildFields(tcmFields, currentLinkLevel);
              }
              
              if (tcmComponent.Metadata != null)
              {
                  TCM.Fields.ItemFields tcmMetadataFields = new TCM.Fields.ItemFields(tcmComponent.Metadata, tcmComponent.MetadataSchema);
                  c.MetadataFields = manager.BuildFields(tcmMetadataFields, currentLinkLevel);
              }
          }
          if (!manager.BuildProperties.OmitContextPublications)
          {
              c.Publication = manager.BuildPublication(tcmComponent.ContextRepository);
          }
          if (!manager.BuildProperties.OmitOwningPublications)
          {
              c.OwningPublication = manager.BuildPublication(tcmComponent.OwningRepository);
          }
          if (!manager.BuildProperties.OmitFolders)
          {
              TCM.Folder folder = (TCM.Folder)tcmComponent.OrganizationalItem;
              c.Folder = manager.BuildOrganizationalItem(folder);
          }
          if (!manager.BuildProperties.OmitCategories)
          {
              c.Categories = manager.BuildCategories(tcmComponent);
          }
          manager.AddXpathToFields(c.Fields, "tcm:Content/custom:" + tcmComponent.Schema.RootElementName); 
          manager.AddXpathToFields(c.MetadataFields, "tcm:Metadata/custom:Metadata");
          return c;
      }
	}
}
