using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Larawag.Services
{
    public interface ICompilerService
    {
        Task<ImmutableArray<Diagnostic>> CompileCode(string filename, string outputFileName, string logFile);
    }
}