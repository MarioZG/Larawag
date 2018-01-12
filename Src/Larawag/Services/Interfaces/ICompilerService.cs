using System.Threading.Tasks;

namespace Larawag.Services
{
    public interface ICompilerService
    {
        Task<object> CompileCode(string filename, string outputFileName);
    }
}