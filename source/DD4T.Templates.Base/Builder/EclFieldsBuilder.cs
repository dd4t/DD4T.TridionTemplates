using System;
using System.Collections.Generic;
using System.Xml;
using DD4T.ContentModel;
using Tridion.ContentManager.Templating;
using Tridion.ExternalContentLibrary.V2;

namespace DD4T.Templates.Base.Builder
{
    internal static class EclFieldsBuilder
    {
        private static readonly TemplatingLogger _log = TemplatingLogger.GetLogger(typeof(EclFieldsBuilder));

        internal static IFieldSet BuildExternalMetadataFieldSet(IContentLibraryItem eclItem)
        {
            string externalMetadataXml = eclItem.MetadataXml;
            if (string.IsNullOrEmpty(externalMetadataXml))
            {
                // No external metadata available; nothing to do.
                return null;
            }

            ISchemaDefinition externalMetadataSchema = eclItem.MetadataXmlSchema;
            if (externalMetadataSchema == null)
            {
                _log.Warning(string.Format("ECL Item '{0}' has external metadata, but no schema defining it.", eclItem.Id));
                return null;
            }

            try
            {
                XmlDocument externalMetadataDoc = new XmlDocument();
                externalMetadataDoc.LoadXml(externalMetadataXml);
                IFieldSet result = CreateExternalMetadataFieldSet(externalMetadataSchema.Fields, externalMetadataDoc.DocumentElement);

                _log.Debug(string.Format("ECL Item '{0}' has external metadata: {1}", eclItem.Id, string.Join(", ", result.Keys)));
                return result;
            }
            catch (Exception ex)
            {
                _log.Error("An error occurred while parsing the external metadata for ECL Item " + eclItem.Id);
                _log.Error(ex.Message);
                return null;
            }
        }

        private static FieldSet CreateExternalMetadataFieldSet(IEnumerable<IFieldDefinition> eclFieldDefinitions, XmlElement parentElement)
        {
            FieldSet fieldSet = new FieldSet();
            foreach (IFieldDefinition eclFieldDefinition in eclFieldDefinitions)
            {
                XmlNodeList fieldElements = parentElement.SelectNodes(string.Format("*[local-name()='{0}']", eclFieldDefinition.XmlElementName));
                if (fieldElements.Count == 0)
                {
                    // Don't generate a DD4T Field for ECL field without values.
                    continue;
                }

                Field field = new Field { Name = eclFieldDefinition.XmlElementName };
                foreach (XmlElement fieldElement in fieldElements)
                {
                    if (eclFieldDefinition is INumberFieldDefinition)
                    {
                        field.NumericValues.Add(Convert.ToDouble(fieldElement.InnerText));
                        field.FieldType = FieldType.Number;
                    }
                    else if (eclFieldDefinition is IDateFieldDefinition)
                    {
                        field.DateTimeValues.Add(Convert.ToDateTime(fieldElement.InnerText));
                        field.FieldType = FieldType.Date;
                    }
                    else if (eclFieldDefinition is IFieldGroupDefinition)
                    {
                        if (field.EmbeddedValues == null)
                        {
                            field.EmbeddedValues = new List<FieldSet>();
                        }
                        IEnumerable<IFieldDefinition> embeddedFieldDefinitions = ((IFieldGroupDefinition) eclFieldDefinition).Fields;
                        field.EmbeddedValues.Add(CreateExternalMetadataFieldSet(embeddedFieldDefinitions, fieldElement));
                        field.FieldType = FieldType.Embedded;
                    }
                    else
                    {
                        field.Values.Add(fieldElement.InnerText);
                        if (eclFieldDefinition is IMultiLineTextFieldDefinition)
                        {
                            field.FieldType = FieldType.MultiLineText;
                        }
                        else if (eclFieldDefinition is IXhtmlFieldDefinition)
                        {
                            field.FieldType = FieldType.Xhtml;
                        }
                        else
                        {
                            field.FieldType = FieldType.Text;
                        }
                    }
                }

                fieldSet.Add(eclFieldDefinition.XmlElementName, field);
            }

            return fieldSet;
        }
    }
}
