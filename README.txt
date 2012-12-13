Project Dependencies
--------------------------------------------------------------

The following SDKs should be installed in the development environment prior to building the source:

- If you don't already have NuGet installed go to http://nuget.org and install the NuGet Visual Studio extension.

- Once NuGet is installed install the Silverlight for Windows Phone Toolkit package (ID: SilverlightToolkitWP).

- Install "Windows Phone toolkit" package (ID: WPtoolkit).


--------------------------------------------------------------
RELEASE INSTRUCTIONS

- Upload the English resx file to GlotPress http://translate.wordpress.org/projects/wordpress-for-windows-phone 
- Ping translators on http://make.wordpress.org/polyglots/
- Download the translated strings from GlotPress, rename each files with the correct name.
- Open each file, do a small edit, and re-save it.
- Verify that there're not instances of :
<value></value>
- Change the configuration to Release and build for Win7


Building for Windows Phone 8
--------------------------------------------------------------

Build the app for Windows Phone 8 to support different size screens.

- In the project properties editor >  Application tab, change the Target Windows OS Version to 8.0.  This will update project files and building for earlier versions will no longer be an option. DO NOT commit the updated project files after doing this. :)

- In the project properties editor > Build tab, add WINPHONE8 to the conditional compliation symbols. This is to correct the layout of the Blog Panorama due to differences between the 7.1 and 8.0 SDKs.
