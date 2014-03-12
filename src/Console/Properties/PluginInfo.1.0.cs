using System.Reflection;
using JetBrains.ActionManagement;
using JetBrains.Application.PluginSupport;

[assembly: ActionsXml("JetBrains.DotPeek.Plugins.Console.Actions.1.0.xml")]

// The following information is displayed in the Plugins dialog
[assembly: PluginTitle("Console")]
[assembly: PluginDescription("dotPeek Console allows execution of C# code inside dotPeek. It supports dotPeek 1.0 and 1.1.")]
[assembly: PluginVendor("Maarten Balliauw")]

[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("1.0.0.0")]