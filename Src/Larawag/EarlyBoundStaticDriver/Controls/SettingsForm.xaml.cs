using Larawag.EarlyBoundStaticDriver.ViewModels;
using Larawag.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Larawag.EarlyBoundStaticDriver.Controls
{
    /// <summary>
    /// Interaction logic for SettingsForm.xaml
    /// </summary>
    public partial class SettingsForm : Window
    {
        public SettingsForm()
        {
            this.DataContext = new SettingsFormViewModel(
                new OrganizationServiceContextGenerator(),
                new CompilerService(),
                new ConnectionStringService(),
                this.Dispatcher);
            InitializeComponent();
        }
    }
}
