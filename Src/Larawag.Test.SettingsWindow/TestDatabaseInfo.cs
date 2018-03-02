using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.Test.SettingsWindow
{
    class TestDatabaseInfo : IDatabaseInfo
    {
        public string Provider { get; set; }
        public string DbVersion { get; set; }
        public string CustomCxString { get; set; }
        public bool EncryptTraffic { get; set; }
        public bool EncryptCustomCxString { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public bool AttachFile { get; set; }
        public string AttachFileName { get; set; }
        public bool UserInstance { get; set; }
        public bool SqlSecurity { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int MaxDatabaseSize { get; set; }

        public bool IsSqlServer => false;

        public bool IsSqlCE => false;

        public IDbConnection GetConnection()
        {
            throw new NotImplementedException();
        }

        public string GetCxString()
        {
            throw new NotImplementedException();
        }

        public string GetDatabaseDescription()
        {
            throw new NotImplementedException();
        }

        public DbProviderFactory GetProviderFactory()
        {
            throw new NotImplementedException();
        }

        public bool IsEquivalent(IDatabaseInfo other)
        {
            throw new NotImplementedException();
        }
    }
}
