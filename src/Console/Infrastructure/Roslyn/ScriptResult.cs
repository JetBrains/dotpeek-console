using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace JetBrains.DotPeek.Plugins.Console.Infrastructure.Roslyn
{
    public class ScriptResult
    {
        public object ReturnValue { get; set; }
        public ExceptionDispatchInfo ExecuteExceptionInfo { get; set; }
        public ExceptionDispatchInfo CompileExceptionInfo { get; set; }
    }
}
