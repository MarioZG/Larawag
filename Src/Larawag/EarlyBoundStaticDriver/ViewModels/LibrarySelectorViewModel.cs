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

        private IConnectionInfo connectionInfo;
        public IConnectionInfo ConnectionInfo
        {
            get { return connectionInfo; }
            set { base.SetProperty<IConnectionInfo>(ref connectionInfo, value); }
        }

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
            CommandSelectDll = new RelayCommand(SelectDllClicked);
            CommandSelectClass = new RealyAsyncCommand<object>(SelectClassClicked, CanSelectClassClicked);
            CommandConfirmSettings = new RelayCommand(ConfirmSettingsClicked, ConfirmSettiingsCanExecute);
            CommandOpenGenerateDllLog = new RelayCommand(OpenGenerateDllLogClicked);
            CommandCancelSettings = new RelayCommand(CancelSettingsClicked);

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

        private void CancelSettingsClicked(object arg)
        {
            SetupCompleted?.Invoke(this, new DriverSetupFinished(false));
        }

        private void OpenGenerateDllLogClicked(object arg)
        {
            string workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(OrganizationServiceContextGenerator)).Location);

            Process.Start(workingDirectory + "\\CrmSvcUtil.log");
        }

        private void SelectDllClicked(object arg)
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
        }

        private void SetDllPath(string dllPath)
        {
            ConnectionInfo.CustomTypeInfo.CustomAssemblyPath = dllPath;
            RaisePropertyChangedEvent(nameof(ConnectionInfo));
        }

        private bool CanSelectClassClicked(object arg)
        {
            return !String.IsNullOrWhiteSpace(ConnectionInfo?.CustomTypeInfo?.CustomAssemblyPath);
        }

        private async Task<object> SelectClassClicked(object arg)
        {
            var tcs = new TaskCompletionSource<object>();
            string assemPath = ConnectionInfo.CustomTypeInfo.CustomAssemblyPath;
            if (assemPath.Length == 0)
            {
                await Task.Run(async() => { throw new ArgumentException("Missing assembly path."); }); 
            }

            if (!File.Exists(assemPath))
            {
                await Task.Run(async () => { throw new ArgumentException($"Assembly {assemPath} does not exist."); });
            }

            string[] customTypes = null;
            try
            {
                Func<Type, bool> predicate = t => t?.BaseType?.FullName == "Microsoft.Xrm.Sdk.Client.OrganizationServiceContext";
                Func<Type, object> selector = t => t.FullName;
                var types = AssemblyLoader.GetTypesFromAssembly(
                    ConnectionInfo.CustomTypeInfo.CustomAssemblyPath, 
                    predicate,
                    selector);

                customTypes = types.Cast<string>().ToArray();
            }
            catch (Exception ex)
            {
                await Task.Run(async () => { throw new Exception("Error obtaining custom types: " + ex.Message); });
            }
            if (customTypes.Length == 0)
            {
                System.Windows.MessageBox.Show("There are no public types in that assembly.");  // based on.........
                await Task.Run(async () => { throw new Exception("There are no public types in that assembly."); });
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
            if (String.IsNullOrWhiteSpace(ConnectionInfo.DatabaseInfo.CustomCxString))
            {
                await Task.Run(async () => { throw new Exception("Missing connection string."); });
            }
            if (String.IsNullOrWhiteSpace(ConnectionInfo.DatabaseInfo.CustomCxString))
            {
                await Task.Run(async () => { throw new Exception("Missing connection server."); });
            }
            string workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(OrganizationServiceContextGenerator)).Location);
            string hostname = new Uri(ConnectionInfo.DatabaseInfo.Server).Host;
            workingDirectory = Path.Combine(workingDirectory, hostname);
            if(! Directory.Exists(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
            }
            string fileName = Path.Combine(workingDirectory,"ContextCode.cs");
            var connectionString = connectionStringService.GetConnectionString(ConnectionInfo);
            bool codeGenerated = await contextGenerator.GenerateCode(connectionString, fileName);
            if (codeGenerated)
            {
                var dllPath = Path.Combine(workingDirectory, hostname+ ".dll");
                var compilationErrors = await compilerService.CompileCode(fileName, dllPath, "");
                if(compilationErrors.Count == 0)
                {
                    SetDllPath(dllPath);
                }
                else
                {
                    await Task.Run(async () => { throw new Exception("Error while compiling code:"+Environment.NewLine + String.Join(Environment.NewLine, compilationErrors)); });
                }
            }
            else
            {
                await Task.Run(async () => { throw new Exception("Error while generating code."); });
            }
            return null;
        }

        private void ConfirmSettingsClicked(object arg)
        {
            SetupCompleted?.Invoke(this, new DriverSetupFinished(true));
        }

        private bool ConfirmSettiingsCanExecute(object arg)
        {
            return ! String.IsNullOrWhiteSpace(ConnectionInfo?.CustomTypeInfo?.CustomTypeName);
        }
    }
}
