using LINQPad.Extensibility.DataContext;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.CrmConnectControl.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
            //var password = LINQPad.Util.GetPassword(PasswordPrefix + connectionInfo.DatabaseInfo.UserName);
            //connString += $"Password='{password.Replace("&", "&amp;")}';";
            return connString;
        }

        public void SetPasword(string username, string password)
        {
            LINQPad.Util.SetPassword(PasswordPrefix + username, password);
        }

        public void ApplyDBInfoInfoFromClientService(CrmServiceClient crmSvc, IDatabaseInfo dbInfo) 
        {
            string username = "", password = "";
            var url = crmSvc.ConnectedOrgPublishedEndpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication];
            var authType = crmSvc.ActiveAuthenticationType;
            var orgName = crmSvc.ConnectedOrgFriendlyName;
            
            if (crmSvc.ActiveAuthenticationType == AuthenticationType.Office365)
            {
                username = crmSvc.OrganizationServiceProxy.ClientCredentials.UserName.UserName;
                //password = crmSvc.OrganizationServiceProxy.ClientCredentials.UserName.Password;

                dbInfo.CustomCxString = $"Url={url};  Username={username}; AuthType={crmSvc.ActiveAuthenticationType};";
            }
            else if (crmSvc.ActiveAuthenticationType == AuthenticationType.OAuth)
            {
                var cred = CredentialManager.ReadCredentials("ShowConnectionDialog for Larawag_Default");
                username = cred.UserName;
                IntPtr bstr = Marshal.SecureStringToBSTR(cred.Password);
                //password = Marshal.PtrToStringBSTR(bstr);

                dbInfo.CustomCxString = $"AuthType=OAuth;Username={username}; Url={url};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d; RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto;";
            }

            //this.SetPasword(username, password);

            dbInfo.Provider = "Larawag.DynamicsCRM";
            dbInfo.UserName = username;
            dbInfo.EncryptCustomCxString = true;
            dbInfo.Server = url;
            dbInfo.Database = orgName;




        }

        public bool AreConnectionsEquivalent(IConnectionInfo c1, IConnectionInfo c2)
        {
            var areEqual = c1.DatabaseInfo.UserName?.ToLower() == c2.DatabaseInfo.UserName?.ToLower()
                && c1.DatabaseInfo.Database?.ToLower() == c2.DatabaseInfo.Database?.ToLower()
                && c1.DatabaseInfo.Server?.ToLower() == c2.DatabaseInfo.Server?.ToLower();
            return areEqual;
        }

        public bool IsConnectionProvided(IDatabaseInfo databaseInfo)
        {
            return !String.IsNullOrWhiteSpace(databaseInfo?.UserName)
                && !String.IsNullOrWhiteSpace(databaseInfo?.Database)
                && !String.IsNullOrWhiteSpace(databaseInfo?.Server);
        }
    }
}
