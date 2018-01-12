using System.Threading.Tasks;

namespace Larawag.Services
{
    public interface IOrganizationServiceContextGenerator
    {
        Task<object> GenerateCode(string connectionString, string outFile);
    }
}