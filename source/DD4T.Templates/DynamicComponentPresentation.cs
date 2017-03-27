using DD4T.Templates.Base;
using DD4T.Templates.Base.Utils;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;

namespace DD4T.Templates
{
    /// <summary>
    /// Generates a DD4T data model based for the current component presentation
    /// </summary>
    /// <remarks>
    /// This is a new feature in DD4T 2.0. If your web application is running on earlier versions 
    /// of DD4T, do NOT use this template, but use Generate dynamic component instead.
    /// </remarks>
    [TcmTemplateTitle("Generate dynamic component presentation")]
    [TcmTemplateParameterSchema("resource:DD4T.Templates.Resources.Schemas.Dynamic Delivery Parameters.xsd")]
    public partial class DynamicComponentPresentation : BaseComponentTemplate
    {

        private BinaryPublisher _binaryPublisher = null;
        protected BinaryPublisher BinaryPublisher
        {
            get
            {
                if (_binaryPublisher == null)
                {
                    _binaryPublisher = new BinaryPublisher(Package, Engine);
                }
                return _binaryPublisher;
            }
        }

        public DynamicComponentPresentation() : base(TemplatingLogger.GetLogger(typeof(DynamicComponentPresentation))) 
        { 
               ComponentPresentationRenderStyle = ComponentPresentationRenderStyle.ComponentPresentation;
        }

        #region DynamicDeliveryTransformer Members
        protected override void TransformComponent(Dynamic.Component component)
        {
            Log.Debug(string.Format("Started TransformComponent for component {0}", component.Id));
            // persist the ComponentPresentationRenderStyle in the package so that the next TBB in the chain is able to read it
            if (Package != null)
            {
                Item renderStyle = Package.CreateStringItem(ContentType.Text, ComponentPresentationRenderStyle.ToString());
                Package.PushItem("render-style", renderStyle);
            }

            // call helper function to publish all relevant multimedia components
            // Note: this template only published mm components that are found in the metadata of the page
            // MM components that are used as component presentation, or that are linked from a component that is 
            // used as a component presentation, will be published from the component template
            // (e.g. by adding 'Publish binaries for components' to that CT)

            PublishAllBinaries(component);

        }
        #endregion

        #region private
      

        protected void PublishAllBinaries(Dynamic.FieldSet fieldSet)
        {
            foreach (Dynamic.Field field in fieldSet.Values)
            {
                if (field.FieldType == Dynamic.FieldType.ComponentLink || field.FieldType == Dynamic.FieldType.MultiMediaLink)
                {
                    foreach (Dynamic.Component linkedComponent in field.LinkedComponentValues)
                    {
                        PublishAllBinaries(linkedComponent);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Embedded)
                {
                    foreach (Dynamic.FieldSet embeddedFields in field.EmbeddedValues)
                    {
                        PublishAllBinaries(embeddedFields);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Xhtml)
                {
                    for (int i = 0; i < field.Values.Count; i++)
                    {
                        string xhtml = field.Values[i];
                        field.Values[i] = BinaryPublisher.PublishBinariesInRichTextField(xhtml, Manager.BuildProperties);
                    }
                }
            }
        }


        private void PublishAllBinaries(Dynamic.Component component)
        {
            if (component.ComponentType.Equals(Dynamic.ComponentType.Multimedia))
            {
                BinaryPublisher.PublishMultimediaComponent(component, Manager.BuildProperties);
            }
            foreach (var field in component.Fields.Values)
            {
                if (field.FieldType == Dynamic.FieldType.ComponentLink || field.FieldType == Dynamic.FieldType.MultiMediaLink)
                {
                    foreach (Dynamic.IComponent linkedComponent in field.LinkedComponentValues)
                    {
                        PublishAllBinaries(linkedComponent as Dynamic.Component);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Embedded)
                {
                    foreach (Dynamic.FieldSet embeddedFields in field.EmbeddedValues)
                    {
                        PublishAllBinaries(embeddedFields);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Xhtml)
                {
                    for (int i = 0; i < field.Values.Count; i++)
                    {
                        string xhtml = field.Values[i];
                        field.Values[i] = BinaryPublisher.PublishBinariesInRichTextField(xhtml, Manager.BuildProperties);
                    }
                }
            }
            foreach (var field in component.MetadataFields.Values)
            {
                if (field.FieldType == Dynamic.FieldType.ComponentLink || field.FieldType == Dynamic.FieldType.MultiMediaLink)
                {
                    foreach (Dynamic.Component linkedComponent in field.LinkedComponentValues)
                    {
                        PublishAllBinaries(linkedComponent);
                    }
                }
                if (field.FieldType == Dynamic.FieldType.Xhtml)
                {
                    for (int i = 0; i < field.Values.Count; i++)
                    {
                        string xhtml = field.Values[i];
                        field.Values[i] = BinaryPublisher.PublishBinariesInRichTextField(xhtml, Manager.BuildProperties);
                    }
                }
            }
        }

        #endregion
    }
}
