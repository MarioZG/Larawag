using Larawag.Services;
using Larawag.Utils;
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

        private async Task<object> SelectClassClicked(object arg)
        {
            var tcs = new TaskCompletionSource<object>();
            string assemPath = ConnectionInfo.CustomTypeInfo.CustomAssemblyPath;
            if (assemPath.Length == 0)
            {
                tcs.SetException(new ArgumentException("Missing assembly path."));
                return tcs.Task;
            }

            if (!File.Exists(assemPath))
            {
                tcs.SetException(new ArgumentException($"Assembly {assemPath} does not exist."));
                return tcs.Task;
            }

            string[] customTypes;
            try
            {
                Func<Type, bool> predicate = t => t?.BaseType?.FullName == "Microsoft.Xrm.Sdk.Client.OrganizationServiceContext";
                var types = AssemblyLoader.GetTypesFromAssembly(
                    ConnectionInfo.CustomTypeInfo.CustomAssemblyPath, 
                    predicate);

                customTypes = types.Select(t => t.FullName).ToArray();
            }
            catch (Exception ex)
            {
                tcs.SetException(new ArgumentException("Error obtaining custom types: " + ex.Message));
                return tcs.Task;
            }
            if (customTypes.Length == 0)
            {
                System.Windows.MessageBox.Show("There are no public types in that assembly.");  // based on.........
                tcs.SetException(new ArgumentException("There are no public types in that assembly."));
                return tcs.Task;
            }

            string result = (string)LINQPad.Extensibility.DataContext.UI.Dialogs.PickFromList("Choose Custom Type", customTypes);
            if (result != null)
            {
                ConnectionInfo.CustomTypeInfo.CustomTypeName = result;
                RaisePropertyChangedEvent(nameof(ConnectionInfo));
            }
            return tcs.Task;
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
