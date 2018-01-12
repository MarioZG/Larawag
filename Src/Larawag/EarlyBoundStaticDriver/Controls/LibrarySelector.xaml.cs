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
    /// Interaction logic for LibrarySelector.xaml
    /// </summary>
    public partial class LibrarySelector : UserControl
    {
        public LibrarySelector()
        {
            this.DataContext = new LibrarySelectorViewModel(new OrganizationServiceContextGenerator(), new CompilerService());
            InitializeComponent();
        }
    }
}
