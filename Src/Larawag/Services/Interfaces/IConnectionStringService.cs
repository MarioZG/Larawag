using LINQPad.Extensibility.DataContext;

namespace Larawag.Services
{
    public interface IConnectionStringService
    {
        string GetConnectionString(IConnectionInfo connectionInfo);

        void SetPasword(string username, string password);
    }
}