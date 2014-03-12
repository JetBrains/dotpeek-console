#if DP10
using System.Windows.Media.Imaging;
using JetBrains.DotPeek.AssemblyExplorer;
using JetBrains.DotPeek.ExplorerNodesModel.Nodes;
#else
using JetBrains.ReSharper.Features.Browsing.AssemblyExplorer;
using JetBrains.ReSharper.Features.Browsing.AssemblyExplorer.ExplorerNodesModel.Core;
using JetBrains.ReSharper.Features.Browsing.AssemblyExplorer.ExplorerNodesModel.Nodes;
using JetBrains.UI.Icons;
#endif
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.DotPeek.Plugins.Console.ToolWindow;
using JetBrains.IDE.TreeBrowser;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ProjectModel.Model2.Assemblies.Interfaces;

namespace JetBrains.DotPeek.Plugins.Console.Handlers
{
    [ActionHandler("Console.ShowConsole", "Console.OpenAssemblyInConsole")]
    public class ShowConsoleWindowHandler
        : IActionHandler
    {
        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return context.GetData<ISolution>(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION) != null;
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            ISolution solution = context.GetData<ISolution>(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION);
            if (solution == null)
            {
                return;
            }

            var assemblyExplorerManager = SolutionEx.GetComponent<IAssemblyExplorerManager>(solution);
            var assemblyExplorer = assemblyExplorerManager.Opened;
            if (assemblyExplorer == null)
            {
                return;
            }

#if DP10
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream iconStream = asm.GetManifestResourceStream("JetBrains.DotPeek.Plugins.Console.Console.png");
            var decoder = new PngBitmapDecoder(iconStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            var icon = decoder.Frames[0];

            //var icon = new BitmapImage(new Uri(@"pack://application:,,,/JetBrains.DotPeek.Plugins.Console.1.0;Console.png", UriKind.RelativeOrAbsolute));
            //Assembly asm = Assembly.GetExecutingAssembly();
            //Stream iconStream = asm.GetManifestResourceStream("Console.png");
            //BitmapImage bitmap = new BitmapImage();
            //bitmap.BeginInit();
            //bitmap.StreamSource = iconStream;
            //bitmap.EndInit();
            //icon.Source = bitmap;
#else
            IThemedIconManager themedIconManager = SolutionEx.GetComponent<IThemedIconManager>(solution);
            var icon = themedIconManager.GetIcon<ConsoleThemedIcons.Console>().CurrentImageSource;
#endif

            var console = new ConsoleToolWindow(assemblyExplorerManager);
            var consoleWindow = new Window
            {
                Title = "Console",
                Icon = icon,
                Content = console,
                Width = 640,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            consoleWindow.Show();

            // Do we have an assembly node selected somewhere? If so, load the assembly in the console
            var data = context.GetData(TreeModelBrowser.TREE_MODEL_NODES);
            if (data != null)
            {
                var node = data.FirstOrDefault();
                if (node != null)
                {
                    IAssemblyFile assemblyFile = null;
                    IAssemblyFileNode assemblyFileNode = node.DataValue as IAssemblyFileNode;
                    if (assemblyFileNode != null)
                    {
                        assemblyFile = ExplorerNodeEx.GetAssemblyFile(assemblyFileNode);
                    }
                    else
                    {
                        AssemblyReferenceNode assemblyReferenceNode = node.DataValue as AssemblyReferenceNode;
                        if (assemblyReferenceNode != null)
                        {
#if DP10
                            IAssembly assemblyResolveResult = ModuleReferencesResolveStoreEx.ResolveResult(assemblyReferenceNode.Reference);
#else
                            IAssembly assemblyResolveResult = ModuleReferencesResolveStoreEx.GetModuleToAssemblyResolveResult(assemblyReferenceNode.Reference);
#endif
                            if (assemblyResolveResult != null)
                            {
                                assemblyFile = Enumerable.FirstOrDefault(assemblyResolveResult.GetFiles());
                            }
                        }
                    }

                    AssemblyInfoCache component = SolutionEx.TryGetComponent<AssemblyInfoCache>(solution);
#if DP10
                    if (component != null && assemblyFile.Location.ExistsFile)
                    {
                        console.LoadAssemblies(new[] { assemblyFile.Location.FullPath });
                    }
#else
                    if (component != null && assemblyFile.Location.ExistsFile && !AssemblyExplorerUtil.AssemblyIsBroken(assemblyFile.Location, component))
                    {
                        console.LoadAssemblies(new[] {assemblyFile.Location.FullPath});
                    }
#endif
                }
            }
        }
    }
}
