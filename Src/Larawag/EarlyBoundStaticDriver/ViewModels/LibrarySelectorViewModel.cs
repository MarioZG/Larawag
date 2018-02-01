using Larawag.Services;
using Larawag.Utils.Commands;
using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Larawag.EarlyBoundStaticDriver.ViewModels
{
    public class LibrarySelectorViewModel : ViewModelBase
    {
        public ICommand CommandGenerateDll { get; private set; }
        public ICommand CommandOpenGenerateDllLog { get; private set; }
        public ICommand CommandSelectDll { get; private set; }
        public ICommand CommandSelectClass { get; private set; }
        public ICommand CommandConfirmSettings { get; private set; }
        public ICommand CommandCancelSettings { get; private set; }
        

        private IOrganizationServiceContextGenerator contextGenerator;
        private ICompilerService compilerService;
        private IConnectionStringService connectionStringService;

        public IConnectionInfo ConnectionInfo { get; set; }

        public StringBuilder GeneratorOutput { get; private set; } = new StringBuilder();

        #region Event
        /// <summary>
        /// Raised when a connection to CRM has completed. 
        /// </summary>
        public event EventHandler SetupCompleted;
        #endregion


        public LibrarySelectorViewModel(IOrganizationServiceContextGenerator contextGenerator, ICompilerService compilerService, IConnectionStringService connectionStringService, Dispatcher dispatcher)
        {
            CommandGenerateDll =  new RealyAsyncCommand<object>(GenerateDllClicked);
            CommandSelectDll = new RealyAsyncCommand<object>(SelectDllClicked);
            CommandSelectClass = new RealyAsyncCommand<object>(SelectClassClicked);
            CommandConfirmSettings = new RealyAsyncCommand<object>(ConfirmSettingsClicked, ConfirmSettiingsCanExecute);
            CommandOpenGenerateDllLog = new RealyAsyncCommand<object>(OpenGenerateDllLogClicked);
            CommandCancelSettings = new RealyAsyncCommand<object>(CancelSettingsClicked);

            this.contextGenerator = contextGenerator;

            contextGenerator.OutputDataReceived += (sender, args) =>
            {
                GeneratorOutput.AppendLine(args.Data);
                dispatcher.Invoke(DispatcherPriority.Normal,
                   new System.Action(() =>
                   {
                       RaisePropertyChangedEvent(nameof(GeneratorOutput));
                   }
                ));
            };

            contextGenerator.ErrorDataReceived += (sender, args) =>
            {
                GeneratorOutput.AppendLine(args.Data);
                dispatcher.Invoke(DispatcherPriority.Normal,
                   new System.Action(() =>
                   {
                       RaisePropertyChangedEvent(nameof(GeneratorOutput));
                   }
                ));
            };

            this.compilerService = compilerService;
            this.connectionStringService = connectionStringService;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("Microsoft.Xrm.Sdk,"))
            {
                var assembly = Assembly.LoadFrom("Microsoft.Xrm.Sdk.dll");
                return assembly;
            }
            return null;
        }

        private Task<object> CancelSettingsClicked(object arg)
        {
            //do nothing?
            return ConfirmSettingsClicked(arg);
        }

        private Task<object> OpenGenerateDllLogClicked(object arg)
        {
            string workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(OrganizationServiceContextGenerator)).Location);

            Process.Start(workingDirectory + "\\CrmSvcUtil.log");

            return null;
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
                SetDllPath(dialog.FileName);
            }

            return null;
        }

        private void SetDllPath(string dllPath)
        {
            ConnectionInfo.CustomTypeInfo.CustomAssemblyPath = dllPath;
                RaisePropertyChangedEvent(nameof(ConnectionInfo));
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
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                Assembly a = Assembly.LoadFrom(ConnectionInfo.CustomTypeInfo.CustomAssemblyPath);
                var types = a.ExportedTypes.Where(t => t?.BaseType?.FullName == "Microsoft.Xrm.Sdk.Client.OrganizationServiceContext");

                customTypes = types.Select(t => t.FullName).ToArray();
               // customTypes = ConnectionInfo.CustomTypeInfo.GetCustomTypesInAssembly("Microsoft.Xrm.Sdk.Client.OrganizationServiceContext");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error obtaining custom types: " + ex.Message);
                return null;
            }
            if (customTypes.Length == 0)
            {
                System.Windows.MessageBox.Show("There are no public types in that assembly.");  // based on.........
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
            var connectionString = connectionStringService.GetConnectionString(ConnectionInfo);
            bool codeGenerated = await contextGenerator.GenerateCode(connectionString, fileName);
            if (codeGenerated)
            {
                bool compileDll = await compilerService.CompileCode(fileName, workingDirectory + "\\CrmContext.dll");
                if(compileDll)
                {
                    SetDllPath(workingDirectory + "\\CrmContext.dll");
                }
                return compileDll;
            }
            else
            {
                return false;
            }
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
