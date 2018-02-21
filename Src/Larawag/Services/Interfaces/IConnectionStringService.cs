using LINQPad.Extensibility.DataContext;
using Microsoft.Xrm.Tooling.Connector;

namespace Larawag.Services
{
    public interface IConnectionStringService
    {
        string GetConnectionString(IConnectionInfo connectionInfo);

        void SetPasword(string username, string password);

        void ApplyDBInfoInfoFromClientService(CrmServiceClient crmSvc, IDatabaseInfo dbInfo);

        bool AreConnectionsEquivalent(IConnectionInfo c1, IConnectionInfo c2);
    }
}