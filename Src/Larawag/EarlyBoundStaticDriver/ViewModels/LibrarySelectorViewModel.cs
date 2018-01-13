using Larawag.Services;
using Larawag.Utils.Commands;
using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Larawag.EarlyBoundStaticDriver.ViewModels
{
    public class LibrarySelectorViewModel : ViewModelBase
    {
        public ICommand CommandGenerateDll { get; private set; }
        public ICommand CommandSelectDll { get; private set; }
        public ICommand CommandSelectClass { get; private set; }
        public ICommand CommandConfirmSettings { get; private set; }
        

        IOrganizationServiceContextGenerator contextGenerator;
        ICompilerService compilerService;
        public IConnectionInfo ConnectionInfo { get; set; }

        #region Event
        /// <summary>
        /// Raised when a connection to CRM has completed. 
        /// </summary>
        public event EventHandler SetupCompleted;
        #endregion


        public LibrarySelectorViewModel(IOrganizationServiceContextGenerator contextGenerator, ICompilerService compilerService)
        {
            CommandGenerateDll =  new RealyAsyncCommand<object>(GenerateDllClicked);
            CommandSelectDll = new RealyAsyncCommand<object>(SelectDllClicked);
            CommandSelectClass = new RealyAsyncCommand<object>(SelectClassClicked);
            CommandConfirmSettings = new RealyAsyncCommand<object>(ConfirmSettingsClicked, ConfirmSettiingsCanExecute);
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
                RaisePropertyChangedEvent(nameof(ConnectionInfo));
            }

            return null;
        }

        private Task<object> SelectClassClicked(object arg)
        {
            string assemPath = ConnectionInfo.CustomTypeInfo.CustomAssemblyPath;
            if (assemPath.Length == 0)
            {
                //MessageBox.Show("First enter a path to an assembly.");
                return null;
            }

            if (!File.Exists(assemPath))
            {
                //MessageBox.Show("File '" + assemPath + "' does not exist.");
                return null;
            }

            string[] customTypes;
            try
            {
                // TODO: In a real-world driver, call the method accepting a base type instead (unless you're.
                // working with a POCO ORM). For instance: GetCustomTypesInAssembly ("System.Data.Linq.DataContext")
                // You can put interfaces in here, too.
                customTypes = ConnectionInfo.CustomTypeInfo.GetCustomTypesInAssembly("Microsoft.Xrm.Sdk.Client.OrganizationServiceContext");
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error obtaining custom types: " + ex.Message);
                return null;
            }
            if (customTypes.Length == 0)
            {
                //MessageBox.Show("There are no public types in that assembly.");  // based on.........
                return null;
            }

            string result = (string)LINQPad.Extensibility.DataContext.UI.Dialogs.PickFromList("Choose Custom Type", customTypes);
            if (result != null)
            {
                ConnectionInfo.CustomTypeInfo.CustomTypeName = result;
                RaisePropertyChangedEvent(nameof(ConnectionInfo));

            }
            return null;
        }

        private async Task<object> GenerateDllClicked(object arg)
        {
            string workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(OrganizationServiceContextGenerator)).Location);
            string fileName = workingDirectory+ "\\ContextCode.cs";
            await contextGenerator.GenerateCode(ConnectionInfo.DatabaseInfo.CustomCxString, fileName);
            return await compilerService.CompileCode(fileName, workingDirectory+ "\\CrmContext.dll");             
        }

        private Task<object> ConfirmSettingsClicked(object arg)
        {
            if(SetupCompleted != null)
            {
                SetupCompleted(this, null);
            }
            return null;
        }

        private bool ConfirmSettiingsCanExecute(object arg)
        {
            return ! String.IsNullOrWhiteSpace(ConnectionInfo?.CustomTypeInfo?.CustomTypeName);
        }
    }
}
