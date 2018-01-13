using LINQPad.Extensibility.DataContext;
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
    }
}
