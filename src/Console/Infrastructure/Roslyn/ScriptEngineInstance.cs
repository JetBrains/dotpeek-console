using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Documents;
using Roslyn.Compilers;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace JetBrains.DotPeek.Plugins.Console.Infrastructure.Roslyn
{
    public class ScriptEngineInstance
    {
        protected List<string> References { get; private set; }
        protected Session Session { get; private set; }

        public ScriptEngineInstance()
        {
            var engine = new ScriptEngine();

            References = new List<string>();
            foreach (var metadataReference in engine.GetReferences())
            {
                References.Add(metadataReference.Display);
            }

            Session = engine.CreateSession();
        }

        public virtual ScriptResult Execute(string code)
        {
            var result = new ScriptResult();
            try
            {
                var submission = Session.CompileSubmission<object>(code);
                try
                {
                    result.ReturnValue = submission.Execute();
                }
                catch (Exception ex)
                {
                    result.ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                }
            }
            catch (Exception ex)
            {
                result.CompileExceptionInfo = ExceptionDispatchInfo.Capture(ex);
            }

            return result;
        }

        public void AddReferences(string[] referencePaths)
        {
            var referencePathsToAdd = referencePaths.Except(References);
            foreach (var referencePath in referencePathsToAdd)
            {
                var referenceFilename = Path.GetFileName(referencePath);
                if (References.All(existingReferenceFilename => Path.GetFileName(existingReferenceFilename) != referenceFilename))
                {
                    Session.AddReference(referencePath);
                    References.Add(referencePath);
                }
            }
        }
    }
}