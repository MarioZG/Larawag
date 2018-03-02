using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.Test.SettingsWindow
{
    public class TestCustomTypeInfo : ICustomTypeInfo
    {
        public string CustomAssemblyPath { get; set; }
        public string CustomTypeName { get; set; }
        public string CustomMetadataPath { get; set; }

        public string GetAbsoluteCustomAssemblyPath()
        {
            throw new NotImplementedException();
        }

        public string GetCustomTypeDescription()
        {
            throw new NotImplementedException();
        }

        public string[] GetCustomTypesInAssembly()
        {
            throw new NotImplementedException();
        }

        public string[] GetCustomTypesInAssembly(string baseTypeName)
        {
            throw new NotImplementedException();
        }

        public bool IsEquivalent(ICustomTypeInfo other)
        {
            throw new NotImplementedException();
        }
    }
}
