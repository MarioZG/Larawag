using Larawag.Services;
using Larawag.Utils.Commands;
using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Larawag.EarlyBoundStaticDriver.ViewModels
{
    public class LibrarySelectorViewModel
    {
        public ICommand CommandGenerateDll { get; private set; }
        public ICommand CommandSelectDll { get; private set; }
        public ICommand CommandSelectClass { get; private set; }

        IOrganizationServiceContextGenerator contextGenerator;
        ICompilerService compilerService;
        internal IConnectionInfo ConnectionInfo { get; set; }


        public LibrarySelectorViewModel(IOrganizationServiceContextGenerator contextGenerator, ICompilerService compilerService)
        {
            CommandGenerateDll =  new RealyAsyncCommand<object>(GenerateDllClicked);
            CommandSelectDll = new RealyAsyncCommand<object>(SelectDllClicked);
            CommandSelectClass = new RealyAsyncCommand<object>(SelectClassClicked);
            this.contextGenerator = contextGenerator;
            this.compilerService = compilerService;
        }

        private Task<object> SelectDllClicked(object arg)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Choose custom assembly",
                DefaultExt = ".dll",
            };

            if (dialog.ShowDialog() == true)
            {
                ConnectionInfo.CustomTypeInfo.CustomAssemblyPath = dialog.FileName;
            }

            return null;
        }

        private Task<object> SelectClassClicked(object arg)
        {
            System.Windows.MessageBox.Show("aa");
            return null;
        }

        private async Task<object> GenerateDllClicked(object arg)
        {
            string fileName = ".\\ContextCode.cs";
            await contextGenerator.GenerateCode(ConnectionInfo.DatabaseInfo.CustomCxString, fileName);
            return await compilerService.CompileCode(fileName, "CrmContext.dll");             
        }
    }
}
