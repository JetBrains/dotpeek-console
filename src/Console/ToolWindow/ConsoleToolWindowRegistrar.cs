#if DP10
using JetBrains.DotPeek.AssemblyExplorer;
#else
#endif
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.UI.Extensions;
using JetBrains.UI.ToolWindowManagement;

namespace JetBrains.DotPeek.Plugins.Console.ToolWindow
{
    // TODO: make this into a tool window
    //[SolutionComponent]
    //public class ConsoleToolWindowRegistrar
    //{
    //    private readonly Lifetime _lifetime;
    //    private readonly ISolution _solution;
    //    private readonly ToolWindowClass _toolWindowClass;

    //    public ConsoleToolWindowRegistrar(Lifetime lifetime,  ToolWindowManager toolWindowManager, ISolution solution, ConsoleToolWindowDescriptor toolWindowDescriptor) // IActionManager actionManager, IActionBarManager actionBarManager, IShortcutManager shortcutManager, ConsoleToolWindowDescriptor toolWindowDescriptor, ISolution solution, IUIApplication environment)
    //    {
    //        this._lifetime = lifetime;
    //        _solution = solution;

    //        _toolWindowClass = toolWindowManager.Classes[(ToolWindowDescriptor)toolWindowDescriptor];
    //        _toolWindowClass.RegisterEmptyContent(lifetime, lt =>
    //         {
    //             var window = new ConsoleWindowControl();
    //             return window.BindToLifetime(lt);
    //         });
    //    }
    //}
}