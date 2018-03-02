using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Larawag.Test.SettingsWindow
{
    class TestConnectionInfo : IConnectionInfo
    {
        public IDatabaseInfo DatabaseInfo { get; set; }

        public ICustomTypeInfo CustomTypeInfo { get; set; }

        public IDynamicSchemaOptions DynamicSchemaOptions => throw new NotImplementedException();

        public string DisplayName { get; set; }
        public string AppConfigPath { get; set; }
        public bool Persist { get; set; }
        public bool IsProduction { get; set; }
        public XElement DriverData { get; set; }

        public IDictionary<string, object> SessionData => throw new NotImplementedException();

        public string Decrypt(string data)
        {
            throw new NotImplementedException();
        }

        public string Encrypt(string data)
        {
            throw new NotImplementedException();
        }
    }
}
