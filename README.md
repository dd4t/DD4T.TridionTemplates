[![AppVeyor](https://ci.appveyor.com/api/projects/status/github/dd4t/DD4T.TridionTemplates?branch=master&svg=true&passingText=master)](https://ci.appveyor.com/project/DD4T/dd4t-tridiontemplates)

[![AppVeyor](https://ci.appveyor.com/api/projects/status/github/dd4t/DD4T.TridionTemplates?branch=develop&svg=true&passingText=develop)](https://ci.appveyor.com/project/DD4T/dd4t-tridiontemplates)

# DD4T.TridionTemplates
DD4T Tridion Templates

## Release 2.5
- Upgraded to DD4T.Model 2.5
- Support for Regions in Tridion 9 and higher
- Fixed bug with KeyFieldName in Resource schema (33)
- Fixed bug with long binary file names (43)

## Setting Up Your Development Environment
https://github.com/dd4t/DD4T.Core/wiki/9.3-Setting-up-your-development-environment


## Regions in Tridion 9
Before the introduction of page regions in Tridion 9, the concept of 'regions' of component presentations on a page was commonly implemented using a metadata field called 'region' on the metadata of the component template. In that case, the web application would group component presentations together based on this metadata field.

With DD4T 2.5 you can now upgrade your CM to Tridion 9 / 9.1 and start using regions, without having to change your web application. All you need to do is add the new template building block 'Convert regions to CT metadata' to your page templates. It will put a metadata field called region in each of the component templates, and put the name of the region in it.
