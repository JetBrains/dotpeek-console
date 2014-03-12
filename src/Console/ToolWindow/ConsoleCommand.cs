using System;
using JetBrains.DotPeek.Plugins.Console.Controls;

namespace JetBrains.DotPeek.Plugins.Console.ToolWindow
{
    public class ConsoleCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Action<ConsoleControl, string> Execute { get; set; }
    }
}