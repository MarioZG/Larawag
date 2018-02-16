using LINQPad.Extensibility.DataContext;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Larawag.Services
{
    public interface IOrganizationServiceContextGenerator
    {
        event DataReceivedEventHandler ErrorDataReceived;
        event DataReceivedEventHandler OutputDataReceived;
        Task<bool> GenerateCode(string connectionString, string outFile);
        string GetWorkingFolder(IDatabaseInfo dbInfo);
    }
}