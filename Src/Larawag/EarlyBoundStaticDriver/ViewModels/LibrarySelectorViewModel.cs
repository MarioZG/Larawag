using Larawag.Utils.Commands;
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


        public LibrarySelectorViewModel()
        {
            CommandGenerateDll =  new RealyAsyncCommand<object>(GenerateDllClicked);
            CommandSelectDll = new RealyAsyncCommand<object>(SelectDllClicked);
            CommandSelectClass = new RealyAsyncCommand<object>(SelectClassClicked);
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
                //_cxInfo.CustomTypeInfo.CustomAssemblyPath = dialog.FileName;
            }

            return null;
        }

        private Task<object> SelectClassClicked(object arg)
        {
            System.Windows.MessageBox.Show("aa");
            return null;
        }

        private Task<object> GenerateDllClicked(object arg)
        {
            System.Windows.MessageBox.Show("aa");
            return null;
        }
    }
}
