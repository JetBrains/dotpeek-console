using System.Reflection;
using JetBrains.ActionManagement;
using JetBrains.Application.PluginSupport;

[assembly: ActionsXml("JetBrains.DotPeek.Plugins.Console.Actions.xml")]

// The following information is displayed in the Plugins dialog
[assembly: PluginTitle("Console")]
[assembly: PluginDescription("dotPeek Console allows execution of C# code inside dotPeek.")]
[assembly: PluginVendor("Maarten Balliauw")]

[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("1.0.0.0")]