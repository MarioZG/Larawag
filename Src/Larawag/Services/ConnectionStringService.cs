using LINQPad.Extensibility.DataContext;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.Services
{
    class ConnectionStringService : IConnectionStringService
    {
        private const string PasswordPrefix = "CRMDriver-";

        public string GetConnectionString(IConnectionInfo connectionInfo)
        {
            var connString = connectionInfo.DatabaseInfo.CustomCxString;
            var password = LINQPad.Util.GetPassword(PasswordPrefix + connectionInfo.DatabaseInfo.UserName);
            connString += $" Password ={ password};";
            return connString;
        }

        public void SetPasword(string username, string password)
        {
            LINQPad.Util.SetPassword(PasswordPrefix + username, password);
        }

        public void ApplyDBInfoInfoFromClientService(CrmServiceClient crmSvc, IDatabaseInfo dbInfo) 
        {
            var username = crmSvc.OrganizationServiceProxy.ClientCredentials.UserName.UserName;
            var password = crmSvc.OrganizationServiceProxy.ClientCredentials.UserName.Password;
            var url = crmSvc.ConnectedOrgPublishedEndpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication];
            var authType = crmSvc.ActiveAuthenticationType;

            this.SetPasword(username, password);

            dbInfo.UserName = username;
            dbInfo.EncryptCustomCxString = true;
            dbInfo.Server = url;

            dbInfo.CustomCxString = $"Url={url};  Username={username}; AuthType={authType};";
        }

        public bool AreConnectionsEquivalent(IConnectionInfo c1, IConnectionInfo c2)
        {
            var areEqual = c1.DatabaseInfo.UserName.ToLower() == c2.DatabaseInfo.UserName.ToLower()
                && c1.DatabaseInfo.Server.ToLower() == c2.DatabaseInfo.Server.ToLower();
            return areEqual;
        }
    }
}
