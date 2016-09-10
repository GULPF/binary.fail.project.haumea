using System.Reflection;
using System.Runtime.CompilerServices;
#if __ANDROID__
using Android.App;
#endif

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle("haumea.desktopgl")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Oscar Nihlgård")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Allows test code to access internal classes/properties.
// See http://stackoverflow.com/questions/358196/
// for some discussion regarding this.
// Also, https://lostechies.com/derickbailey/2014/01/03/semantics-modules-and-testing-why-and-how-i-test-internal-components-not-private-methods/
[assembly: InternalsVisibleTo("unittests")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("1.0.0")]

// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

