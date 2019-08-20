using Larawag.EarlyBoundStaticDriver.Controls;
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
    public class SettingsFormViewModel : ViewModelBase
    {
        public ICommand CommandGenerateDll { get; private set; }
        public ICommand CommandOpenGenerateDllLog { get; private set; }
        public ICommand CommandOpenCompileDllLog { get; private set; }
        public ICommand CommandSelectDll { get; private set; }
        public ICommand CommandSelectClass { get; private set; }
        public ICommand CommandConfirmSettings { get; private set; }
        public ICommand CommandCancelSettings { get; private set; }
        public ICommand CommandLoginToCrm { get; private set; }

        private const string compilationLogFilename = "Compilationlog.txt";

        private IOrganizationServiceContextGenerator contextGenerator;
        private ICompilerService compilerService;
        private IConnectionStringService connectionStringService;

        private IConnectionInfo connectionInfo;
        public IConnectionInfo ConnectionInfo
        {
            get { return connectionInfo; }
            set { base.SetProperty<IConnectionInfo>(ref connectionInfo, value, onChanged:() => RaisePropertyChangedEvent(nameof(IsConnectionProvided))); }
        }

        public bool IsConnectionProvided
        {
            get
            {
                return connectionStringService.IsConnectionProvided(ConnectionInfo?.DatabaseInfo);
            }
        }

        //crappy, will add proper converter when needed in more cases
        public bool InvertIsConnectionProvided
        {
            get
            {
                return ! IsConnectionProvided;
            }
        }


        public StringBuilder GeneratorOutput { get; private set; } = new StringBuilder();

        #region Event
        /// <summary>
        /// Raised when a connection to CRM has completed. 
        /// </summary>
        public event EventHandler SetupCompleted;
        #endregion


        public SettingsFormViewModel(IOrganizationServiceContextGenerator contextGenerator, ICompilerService compilerService, IConnectionStringService connectionStringService, Dispatcher dispatcher)
        {
            CommandGenerateDll =  new RealyAsyncCommand<object>(GenerateDllClicked, CanGenerateDllClicked);
            CommandSelectDll = new RelayCommand(SelectDllClicked);
            CommandSelectClass = new RealyAsyncCommand<object>(SelectClassClicked, CanSelectClassClicked);
            CommandConfirmSettings = new RelayCommand<System.Windows.Window>(ConfirmSettingsClicked, ConfirmSettiingsCanExecute);
            CommandOpenGenerateDllLog = new RelayCommand(OpenGenerateDllLogClicked);
            CommandOpenCompileDllLog = new RelayCommand(OpenCompileDllLog);
            CommandCancelSettings = new RelayCommand<System.Windows.Window>(CancelSettingsClicked);
            CommandLoginToCrm = new RelayCommand(CommandLoginToCrmClicked);

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

        private void OpenCompileDllLog()
        {
            string workingDirectory = contextGenerator.GetWorkingFolder(ConnectionInfo.DatabaseInfo);

            Process.Start(Path.Combine(workingDirectory, compilationLogFilename));
        }

        private void CancelSettingsClicked(System.Windows.Window window)
        {
            SetupCompleted?.Invoke(this, new DriverSetupFinished(false));
            window.DialogResult = false;
            window.Close();
        }

        private void OpenGenerateDllLogClicked()
        {
            string workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(OrganizationServiceContextGenerator)).Location);

            var fielname = workingDirectory + "\\CrmSvcUtil.log";
            if (File.Exists(fielname))
            {
                Process.Start(fielname);
            }
        }

        private void SelectDllClicked()
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

        private bool CanGenerateDllClicked(object arg)
        {
            return connectionStringService.IsConnectionProvided(ConnectionInfo?.DatabaseInfo);
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
            string workingDirectory = contextGenerator.GetWorkingFolder(ConnectionInfo.DatabaseInfo);
            if (! Directory.Exists(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
            }
            string fileName = Path.Combine(workingDirectory,"ContextCode.cs");
            var connectionString = connectionStringService.GetConnectionString(ConnectionInfo);
            bool codeGenerated = await contextGenerator.GenerateCode(connectionString, fileName);
            if (codeGenerated)
            {
                var hostname = new Uri(ConnectionInfo.DatabaseInfo.Server).Host;
                var dllPath = Path.Combine(workingDirectory, hostname+ ".dll");
                var logFile = Path.Combine(workingDirectory, compilationLogFilename);
                var compilationErrors = await compilerService.CompileCode(fileName, dllPath, logFile);
                if(compilationErrors.Length == 0)
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

        private void ConfirmSettingsClicked(System.Windows.Window window)
        {
            SetupCompleted?.Invoke(this, new DriverSetupFinished(true));
            window.DialogResult = true;
            window.Close();
        }

        private bool ConfirmSettiingsCanExecute(System.Windows.Window window)
        {
            return ! String.IsNullOrWhiteSpace(ConnectionInfo?.CustomTypeInfo?.CustomTypeName);
        }

        private void CommandLoginToCrmClicked()
        {
            CRMLoginForm loginForm = new CRMLoginForm(ConnectionInfo);
            loginForm.ContextClassSelectionCompleted += LoginForm_ContextClassSelectionCompleted;
            var dialogResult = loginForm.ShowDialog().GetValueOrDefault();
            RaisePropertyChangedEvent(nameof(ConnectionInfo));
            RaisePropertyChangedEvent(nameof(IsConnectionProvided));
            RaisePropertyChangedEvent(nameof(InvertIsConnectionProvided));
           // return dialogResult;
        }
        private void LoginForm_ContextClassSelectionCompleted(object sender, EventArgs e)
        {
            if (sender is CRMLoginForm castedSender)
            {
                castedSender.Dispatcher.Invoke(() =>
                {
                    //castedSender.DialogResult = ((DriverSetupFinished)e).Confirmed;
                    castedSender.Close();
                });
            }
        }
    }
}
