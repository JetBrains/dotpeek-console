using JetBrains.DotPeek.Plugins.Console.Infrastructure.Roslyn;
#if DP10
using JetBrains.DotPeek.AssemblyExplorer;
#elif DP11 || DP12
using JetBrains.ReSharper.Features.Browsing.AssemblyExplorer;
#endif
using JetBrains.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Brushes = System.Windows.Media.Brushes;

namespace JetBrains.DotPeek.Plugins.Console.ToolWindow
{
    /// <summary>
    /// Interaction logic for ConsoleToolWindow.xaml
    /// </summary>
    public partial class ConsoleToolWindow
        : UserControl
    {
        private IAssemblyExplorerManager assemblyExplorerManager;
        private ScriptEngineInstance engine;
        private Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>(); 

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            MaxDepth = 4
        };

        public ConsoleToolWindow(IAssemblyExplorerManager assemblyExplorerManager)
        {
            InitializeComponent();

            Console.Focus();

            this.assemblyExplorerManager = assemblyExplorerManager;
            engine = new ScriptEngineInstance();

            commands.Add("help", new ConsoleCommand
            {
                Name = "help",
                Description = "Shows the current list of commands.",
                Execute = (control, s) =>
                {
                    control.WriteLine();
                    control.WriteLine("Available commands:");
                    foreach (var command in commands.Values)
                    {
                        control.WriteLine("  " + command.Name);
                        control.WriteLine("    " + command.Description);
                    }
                    control.WriteLine();
                }
            });
            commands.Add("cls", new ConsoleCommand
            {
                Name = "cls",
                Description = "Cleanup the console.",
                Execute = (control, s) =>
                {
                    control.Clear();
                }
            });
            // TODO
            // commands.Add("reference", new ConsoleCommand
            //{
            //    Name = "reference",
            //    Description = "References an assembly by path.",
            //    Execute = (control, s) =>
            //    {
            //        var referenceName = s.Substring(10).Replace("\"", "").TrimEnd(';');

            //        engine.AddReferences(new[] {referenceName});
            //        control.WriteLine(string.Format("Added assembly reference {0}.", referenceName));

            //        if (File.Exists(referenceName))
            //        {
            //            this.assemblyExplorerManager.AddItemsByPath(new[] { new FileSystemPath(referenceName) });
            //        }
            //    }
            //});

            Console.WriteLine("dotPeek C# console - Powered by Roslyn and inspired on ScriptCS");
            Console.WriteLine();
            Console.Write("> ");
            Console.MoveCaretToEnd();

            Console.CommandEntered += (sender, args) =>
            {
                try
                {
                    ConsoleCommand command;
                    if (!string.IsNullOrEmpty(args.Command) &&
                        commands.TryGetValue(args.Command.Split(' ').First().ToLowerInvariant(), out command))
                    {
                        command.Execute(Console, args.Command);
                    }
                    else if (args.Command != string.Empty)
                    {
                        var result = engine.Execute(args.Command);

                        if (result.CompileExceptionInfo != null)
                        {
                            LogError(result.CompileExceptionInfo.SourceException.Message);
                        }
                        if (result.ExecuteExceptionInfo != null)
                        {
                            LogError(result.CompileExceptionInfo.SourceException.Message);
                        }
                        if (result.ReturnValue != null)
                        {
                            Console.CurrentForeground = Brushes.Yellow;
                            Console.WriteLine(JsonConvert.SerializeObject(result.ReturnValue, Formatting.Indented, SerializerSettings));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                }

                Console.ResetColor();
                Console.Write("> ");
                Console.MoveCaretToEnd();
            };

            Console.AutoCompletionRequested += (sender, args) =>
            {
                // Default implementation: be smart with history
                var autoCompletionResult = Console.History.FirstOrDefault(s => s.ToLowerInvariant().StartsWith(args.Command.ToLowerInvariant()));
                if (!string.IsNullOrEmpty(autoCompletionResult))
                {
                    Console.AutoCompletionFinished(autoCompletionResult, true);
                }
            };
        }

        public void LoadAssemblies(IEnumerable<string> assemblyLocations)
        {
            engine.AddReferences(assemblyLocations.ToArray());
            foreach (var assemblyLocation in assemblyLocations)
            {
                LogInformation("Added reference to " + assemblyLocation);
            }

            Console.ResetColor();
            Console.Write("> ");
            Console.MoveCaretToEnd();
        }

        public void LogInformation(string message, params string[] values)
        {
            Console.CurrentForeground = Brushes.Gray;
            Console.WriteLine(string.Format(message, values));
            Console.ResetColor();
        }

        public void LogWarning(string message, params string[] values)
        {
            Console.CurrentForeground = Brushes.Yellow;
            Console.WriteLine(string.Format(message, values));
            Console.ResetColor();
        }

        public void LogError(string message, params string[] values)
        {
            Console.CurrentForeground = Brushes.Red;
            Console.WriteLine(string.Format(message, values));
            Console.ResetColor();
        }
    }
}
