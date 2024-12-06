# DD4T.TridionTemplates
DD4T Tridion Templates

## Release 2.6
- Upgraded to DD4T.Model 2.6.1
- Dropped support for Tridion 9.1 and older (minimum version is now Tridion 9.5)
- Upgraded to .NET Framework 4.8
- Simplified the build process (you can now simply )
- It is now possible to configure a max deserialization depth

### Overriding the maximum deserialization depth
Since version 13.0.1, Newtonsoft uses a maximum depth when deserializing an object into Json. This value is 64 by default. In some cases, this may not be enough for the DD4T data structure, especially when you use a high linklevel value.
You can now override the maximum depth with a template parameter called 'MaxSerializationDepth'. If you're using multiple DD4T TBBs in a single template, use this parameter on the first one. 

## Release 2.5
- Support for Regions in Tridion 9 and higher
- Fixed bug with KeyFieldName in Resource schema (33)
- Fixed bug with long binary file names (43)

## Setting Up Your Development Environment
You need to add dependencies to the Tridion "TOM.NET" DLLs. Since these DLLs are part of commercial software, we cannot include them and they cannot be found on Nuget.  There are 2 ways around this:
- Create a Nuget package called SDLTridionSites9.5-CM, version 9.5.1 containing the required DLLs (see list below). You can either upload this package to a private package repository, or simply put it in a folder that you set up as a local Nuget repo.
- Just copy the DLLs to a folder and create direct dependencies on them (without Nuget). If you take this approach, you must remove the reference to SDLTridionSites9.5-CM from the packages.config files (in DD4T.Templates and DD4T.Templates.Base).

Either way, you need to include the following DLLs:

- Tridion.Common.dll
- Tridion.ContentManager.Common.dll
- Tridion.ContentManager.dll
- Tridion.ContentManager.Publishing.dll
- Tridion.ContentManager.Queuing.dll
- Tridion.ContentManager.TemplateTypes.dll
- Tridion.ContentManager.Templating.dll
- Tridion.ContentManager.TypeRegistration.dll
- Tridion.ExternalContentLibrary.Contract.V3.dll
- Tridion.ExternalContentLibrary.dll
- Tridion.ExternalContentLibrary.Provider.V3.Base.dll
- Tridion.ExternalContentLibrary.V2.dll
- Tridion.Logging.dll

They can be found in the bin\Client folder in any Tridion CM installation.

After successfully building, you will find the final result in tools\template-installer. You can use the install-templates.bat file in that folder to upload the templates to your own Tridion instance.

## Regions in Tridion 9
Before the introduction of page regions in Tridion 9, the concept of 'regions' of component presentations on a page was commonly implemented using a metadata field called 'region' on the metadata of the component template. In that case, the web application would group component presentations together based on this metadata field.

With DD4T 2.5 you can now upgrade your CM to Tridion 9 / 9.1 and start using regions, without having to change your web application. All you need to do is add the new template building block 'Convert regions to CT metadata' to your page templates. It will put a metadata field called region in each of the component templates, and put the name of the region in it.
