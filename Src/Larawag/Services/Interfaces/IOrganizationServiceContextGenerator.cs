using System.Diagnostics;
using System.Threading.Tasks;

namespace Larawag.Services
{
    public interface IOrganizationServiceContextGenerator
    {
        event DataReceivedEventHandler ErrorDataReceived;
        event DataReceivedEventHandler OutputDataReceived;
        Task<bool> GenerateCode(string connectionString, string outFile);
    }
}