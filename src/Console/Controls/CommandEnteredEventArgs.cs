using System;

namespace JetBrains.DotPeek.Plugins.Console.Controls
{
    public class CommandEnteredEventArgs
        : EventArgs
    {
        public string Command { get; set; }
    }
}