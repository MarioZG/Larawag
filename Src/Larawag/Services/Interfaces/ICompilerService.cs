using System.CodeDom.Compiler;
using System.Threading.Tasks;

namespace Larawag.Services
{
    public interface ICompilerService
    {
        Task<CompilerErrorCollection> CompileCode(string filename, string outputFileName, string sdkDllPath);
    }
}