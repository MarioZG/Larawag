using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.Services
{
    public class CompilerService : ICompilerService
    {
        public async Task<object> CompileCode(string filename, string outputFileName)
        {
            // https://support.microsoft.com/en-us/help/304655/how-to-programmatically-compile-code-using-c-compiler
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
#pragma warning disable CS0618 // Type or member is obsolete
            ICodeCompiler icc = codeProvider.CreateCompiler();
#pragma warning restore CS0618 // Type or member is obsolete

            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = Path.GetDirectoryName(filename) + "\\CrmContext.dll";
            parameters.ReferencedAssemblies.Add(Path.GetDirectoryName(filename)+"\\Microsoft.Xrm.Sdk.dll");
            parameters.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");

            CompilerResults results = await Task.Run(() => icc.CompileAssemblyFromFile(parameters, filename));
            return results.Errors.Count == 0;
        }
    }
}
