﻿using Larawag.EarlyBoundStaticDriver.ViewModels;
using Larawag.Services;
using LINQPad.Extensibility.DataContext;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.CrmConnectControl;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Larawag.EarlyBoundStaticDriver.Controls
{
    /// <summary>
    /// Interaction logic for CRMLoginForm1.xaml
    /// </summary>
    public partial class CRMLoginForm : Window
    {
        #region Vars
        /// <summary>
        /// Microsoft.Xrm.Tooling.Connector services
        /// </summary>
        private CrmServiceClient CrmSvc = null;
        /// <summary>
        /// Bool flag to determine if there is a connection 
        /// </summary>
        private bool bIsConnectedComplete = false;
        /// <summary>
        /// CRM Connection Manager component. 
        /// </summary>
        private CrmConnectionManager mgr = null;
        /// <summary>
        ///  This is used to allow the UI to reset w/out closing 
        /// </summary>
        private bool resetUiFlag = false;
        #endregion

        #region Properties
        /// <summary>
        /// CRM Connection Manager 
        /// </summary>
        public CrmConnectionManager CrmConnectionMgr { get { return mgr; } }
        #endregion

        #region Event
        /// <summary>
        /// Raised when a connection to CRM has completed. 
        /// </summary>
        public event EventHandler ContextClassSelectionCompleted;
        #endregion


        public CRMLoginForm(IConnectionInfo cxInfo)
        {
            this.cxInfo = cxInfo;
            DataContext = cxInfo.CustomTypeInfo;
            InitializeComponent();

            //// Should be used for testing only.
            //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            //{
            //    MessageBox.Show("CertError");
            //    return true;
            //};
        }

        IConnectionInfo cxInfo;
        private IConnectionStringService connectionStringService = new ConnectionStringService();

        /// <summary>
        /// Raised when the window loads for the first time. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*
				This is the setup process for the login control, 
				The login control uses a class called CrmConnectionManager to manage the interaction with CRM, this class and also be queried as later points for information about the current connection. 
				In this case, the login control is referred to as CrmLoginCtrl
			 */

            // Set off flag. 
            bIsConnectedComplete = false;

            // Init the CRM Connection manager.. 
            mgr = new CrmConnectionManager();
            // Pass a reference to the current UI or container control,  this is used to synchronize UI threads In the login control
            mgr.ParentControl = CrmLoginCtrl;
            // if you are using an unmanaged client, excel for example, and need to store the config in the users local directory
            // set this option to true. 
            mgr.UseUserLocalDirectoryForConfigStore = true;
            // if you are using an unmanaged client,  you need to provide the name of an exe to use to create app config key's for. 
            //mgr.HostApplicatioNameOveride = "MyExecName.exe";
            // CrmLoginCtrl is the Login control,  this sets the CrmConnection Manager into it. 
            CrmLoginCtrl.SetGlobalStoreAccess(mgr);
            // There are several modes to the login control UI
            CrmLoginCtrl.SetControlMode(ServerLoginConfigCtrlMode.FullLoginPanel);
            // this wires an event that is raised when the login button is pressed. 
            CrmLoginCtrl.ConnectionCheckBegining += new EventHandler(CrmLoginCtrl_ConnectionCheckBegining);
            // this wires an event that is raised when an error in the connect process occurs. 
            CrmLoginCtrl.ConnectErrorEvent += new EventHandler<ConnectErrorEventArgs>(CrmLoginCtrl_ConnectErrorEvent);
            // this wires an event that is raised when a status event is returned. 
            CrmLoginCtrl.ConnectionStatusEvent += new EventHandler<ConnectStatusEventArgs>(CrmLoginCtrl_ConnectionStatusEvent);
            // this wires an event that is raised when the user clicks the cancel button. 
            CrmLoginCtrl.UserCancelClicked += new EventHandler(CrmLoginCtrl_UserCancelClicked);
            // Check to see if its possible to do an Auto Login 

        }

        #region Events

        /// <summary>
        /// Updates from the Auto Login process. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mgr_ServerConnectionStatusUpdate(object sender, ServerConnectStatusEventArgs e)
        {
            // The Status event will contain information about the current login process,  if Connected is false, then there is not yet a connection. 
            // Set the updated status of the loading process. 
            Dispatcher.Invoke(DispatcherPriority.Normal,
                               new System.Action(() =>
                               {
                                   this.Title = string.IsNullOrWhiteSpace(e.StatusMessage) ? e.ErrorMessage : e.StatusMessage;
                               }
                                   ));

        }

        /// <summary>
        /// Complete Event from the Auto Login process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mgr_ConnectionCheckComplete(object sender, ServerConnectStatusEventArgs e)
        {
            // The Status event will contain information about the current login process,  if Connected is false, then there is not yet a connection. 
            // Unwire events that we are not using anymore, this prevents issues if the user uses the control after a failed login. 
            ((CrmConnectionManager)sender).ConnectionCheckComplete -= mgr_ConnectionCheckComplete;
            ((CrmConnectionManager)sender).ServerConnectionStatusUpdate -= mgr_ServerConnectionStatusUpdate;

            if (!e.Connected)
            {
                // if its not connected pop the login screen here. 
                if (e.MultiOrgsFound)
                    MessageBox.Show("Unable to Login to CRM using cached credentials. Org Not found", "Login Failure");
                else
                    MessageBox.Show("Unable to Login to CRM using cached credentials", "Login Failure");

                resetUiFlag = true;
                CrmLoginCtrl.GoBackToLogin();
                // Bad Login Get back on the UI. 
                Dispatcher.Invoke(DispatcherPriority.Normal,
                       new System.Action(() =>
                       {
                           this.Title = "Failed to Login with cached credentials.";
                           MessageBox.Show(this.Title, "Notification from ConnectionManager", MessageBoxButton.OK, MessageBoxImage.Error);
                           CrmLoginCtrl.IsEnabled = true;
                       }
                        ));
                resetUiFlag = false;
            }
            else
            {
                // Good Login Get back on the UI 
                if (e.Connected && !bIsConnectedComplete)
                    ProcessSuccess();
            }

        }

        /// <summary>
        ///  Login control connect check starting. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmLoginCtrl_ConnectionCheckBegining(object sender, EventArgs e)
        {
            bIsConnectedComplete = false;
            Dispatcher.Invoke(DispatcherPriority.Normal,
                               new System.Action(() =>
                               {
                                   this.Title = "Starting Login Process. ";
                                   CrmLoginCtrl.IsEnabled = true;
                               }
                                   ));
        }

        /// <summary>
        /// Login control connect check status event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmLoginCtrl_ConnectionStatusEvent(object sender, ConnectStatusEventArgs e)
        {
            //Here we are using the bIsConnectedComplete bool to check to make sure we only process this call once. 
            if (e.ConnectSucceeded && !bIsConnectedComplete)
            {
                ProcessSuccess();
            }

        }

        /// <summary>
        /// Login control Error event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmLoginCtrl_ConnectErrorEvent(object sender, ConnectErrorEventArgs e)
        {
            //MessageBox.Show(e.ErrorMessage, "Error here");
        }

        /// <summary>
        /// Login Control Cancel event raised. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmLoginCtrl_UserCancelClicked(object sender, EventArgs e)
        {
            if (!resetUiFlag)
            {
                this.Close();
            }
        }

        #endregion

        /// <summary>
        /// This raises and processes Success
        /// </summary>
        private void ProcessSuccess()
        {
            resetUiFlag = true;
            bIsConnectedComplete = true;
            CrmSvc = mgr.CrmSvc;
            CrmLoginCtrl.GoBackToLogin();
            Dispatcher.Invoke(DispatcherPriority.Normal,
               new System.Action(() =>
               {
                   this.Title = "Notification from Parent";
                   CrmLoginCtrl.IsEnabled = true;
               }
                ));

            //here we should be connected to crm and we need connection info to generate model if requested!

            if (CrmConnectionMgr != null && CrmConnectionMgr.CrmSvc != null && CrmConnectionMgr.CrmSvc.IsReady)
            {
                connectionStringService.ApplyDBInfoInfoFromClientService(CrmConnectionMgr.CrmSvc, cxInfo.DatabaseInfo);
            }

            this.DialogResult = true;
            
            // Notify Caller that we are done with success. 
            ContextClassSelectionCompleted?.Invoke(this, null);

            resetUiFlag = false;
        }

        private void CRMLoginForm_SetupCompleted(object sender, EventArgs e)
        {
            ContextClassSelectionCompleted?.Invoke(this, e);
        }

    }

    #region system.diagnostics settings for this control

    // Add or merge this section to your app to enable diagnostics on the use of the CRM login control and connection
    /*
  <system.diagnostics>
    <trace autoflush="true" />
    <sources>
      <source name="Microsoft.Xrm.Tooling.Connector.CrmServiceClient"
              switchName="Microsoft.Xrm.Tooling.Connector.CrmServiceClient"
              switchType="System.Diagnostics.SourceSwitch">
        <listeners>
          <add name="console" type="System.Diagnostics.DefaultTraceListener" />
          <remove name="Default"/>
          <add name ="fileListener"/>
        </listeners>
      </source>
      <source name="Microsoft.Xrm.Tooling.CrmConnectControl"
              switchName="Microsoft.Xrm.Tooling.CrmConnectControl"
              switchType="System.Diagnostics.SourceSwitch">
        <listeners>
          <add name="console" type="System.Diagnostics.DefaultTraceListener" />
          <remove name="Default"/>
          <add name ="fileListener"/>
        </listeners>
      </source>

      <source name="Microsoft.Xrm.Tooling.WebResourceUtility"
              switchName="Microsoft.Xrm.Tooling.WebResourceUtility"
              switchType="System.Diagnostics.SourceSwitch">
        <listeners>
          <add name="console" type="System.Diagnostics.DefaultTraceListener" />
          <remove name="Default"/>
          <add name ="fileListener"/>
        </listeners>
      </source>
      
    <!-- WCF DEBUG SOURCES -->
      <source name="System.IdentityModel" switchName="System.IdentityModel">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <!-- Log all messages in the 'Messages' tab of SvcTraceViewer. -->
      <source name="System.ServiceModel.MessageLogging" switchName="System.ServiceModel.MessageLogging" >
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <!-- ActivityTracing and propogateActivity are used to flesh out the 'Activities' tab in
           SvcTraceViewer to aid debugging. -->
      <source name="System.ServiceModel" switchName="System.ServiceModel" propagateActivity="true">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <!-- END WCF DEBUG SOURCES -->
    </sources>
    <switches>
      <!-- 
            Possible values for switches: Off, Error, Warining, Info, Verbose
                Verbose:    includes Error, Warning, Info, Trace levels
                Info:       includes Error, Warning, Info levels
                Warning:    includes Error, Warning levels
                Error:      includes Error level
        -->
      <add name="Microsoft.Xrm.Tooling.Connector.CrmServiceClient" value="Verbose" />
      <add name="Microsoft.Xrm.Tooling.CrmConnectControl" value="Verbose"/>
      <add name="Microsoft.Xrm.Tooling.WebResourceUtility" value="Verbose" />
      <add name="System.IdentityModel" value="Verbose"/>
      <add name="System.ServiceModel.MessageLogging" value="Verbose"/>
      <add name="System.ServiceModel" value="Error, ActivityTracing"/>
      
    </switches>
    <sharedListeners>
      <add name="fileListener" type="System.Diagnostics.TextWriterTraceListener" initializeData="LoginControlTesterLog.txt"/>
      <!--<add name="eventLogListener" type="System.Diagnostics.EventLogTraceListener" initializeData="CRM UII"/>-->
      <add name="xml" type="System.Diagnostics.XmlWriterTraceListener" initializeData="CrmToolBox.svclog" />
    </sharedListeners>
  </system.diagnostics>
*/

    #endregion
}
