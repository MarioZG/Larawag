using Larawag.EarlyBoundStaticDriver.ViewModels;
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

namespace Larawag.Test.SettingsWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new Larawag.EarlyBoundStaticDriver.Controls.SettingsForm();
            var connInfo = new TestConnectionInfo()
            {
                DatabaseInfo = new TestDatabaseInfo(),
                CustomTypeInfo = new TestCustomTypeInfo(),
            };
            ((SettingsFormViewModel)(window.DataContext)).ConnectionInfo = connInfo;
            window.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var window = new Larawag.EarlyBoundStaticDriver.Controls.SettingsForm();
            var connInfo = new TestConnectionInfo()
            {
                DatabaseInfo = new TestDatabaseInfo()
                {
                    Server = "crm server name",
                    Database = "test 1111",
                    UserName = "mylogin@msonlone"
                },
                CustomTypeInfo = new TestCustomTypeInfo()
                {
                    CustomAssemblyPath = "some path assembly",
                    CustomTypeName = "context class selcted"

                }
            };
            ((SettingsFormViewModel)(window.DataContext)).ConnectionInfo = connInfo;
            window.ShowDialog();
        }
    }
}
