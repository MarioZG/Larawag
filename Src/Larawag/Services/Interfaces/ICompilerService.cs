using System.Threading.Tasks;

namespace Larawag.Services
{
    public interface ICompilerService
    {
        Task<bool> CompileCode(string filename, string outputFileName);
    }
}