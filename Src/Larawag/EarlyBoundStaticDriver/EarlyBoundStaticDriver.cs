using Larawag.EarlyBoundStaticDriver.ViewModels;
using LINQPad.Extensibility.DataContext;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.CrmConnectControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Larawag.EarlyBoundStaticDriver
{
    public class EarlyBoundDriver : StaticDataContextDriver
    {
        public override string Name { get { return "Dynamics early bound CRM Driver"; } }

        public override string Author { get { return "https://github.com/MarioZG"; } }

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            var connString = cxInfo.DatabaseInfo.CustomCxString;
            var passName = Regex.Match(connString, "Url=(?<url>.+?);");
            Uri url = new Uri(passName.Groups["url"].Value);
            passName = Regex.Match(connString, "Username=(?<Username>.+?)[@;]");
            return url.Host + $"[{passName.Groups["Username"]}]";
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            CRMLoginForm loginForm = new CRMLoginForm(cxInfo);
            loginForm.ContextClassSelectionCompleted += LoginForm_ContextClassSelectionCompleted;
            loginForm.ShowDialog();

            CrmConnectionManager connManager = loginForm.CrmConnectionMgr;

            if (connManager != null && connManager.CrmSvc != null && connManager.CrmSvc.IsReady)
            {
                cxInfo.DatabaseInfo.CustomCxString = $"Url={connManager.CrmSvc.ConnectedOrgPublishedEndpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication]};  Username={connManager.CrmSvc.OrganizationServiceProxy.ClientCredentials.UserName.UserName}; Password={connManager.CrmSvc.OrganizationServiceProxy.ClientCredentials.UserName.Password}; AuthType={connManager.CrmSvc.ActiveAuthenticationType};";
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LoginForm_ContextClassSelectionCompleted(object sender, EventArgs e)
        {
            if (sender is CRMLoginForm)
            {
                ((CRMLoginForm)sender).Dispatcher.Invoke(() =>
                {
                    ((CRMLoginForm)sender).Close();
                });
            }
        }

        #region populate schema explorer - c&p from example

        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
        {
            // Return the objects with which to populate the Schema Explorer by reflecting over customType.

            // We'll start by retrieving all the properties of the custom type that implement IEnumerable<T>:
            var topLevelProps =
            (
                from prop in customType.GetProperties()
                where prop.PropertyType != typeof(string)

                // Display all properties of type IEnumerable<T> (except for string!)
                let ienumerableOfT = prop.PropertyType.GetInterface("System.Collections.Generic.IEnumerable`1")
                where ienumerableOfT != null

                orderby prop.Name

                select new ExplorerItem(prop.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                {
                    IsEnumerable = true,
                    ToolTipText = FormatTypeName(prop.PropertyType, false),

                    // Store the entity type to the Tag property. We'll use it later.
                    Tag = ienumerableOfT.GetGenericArguments()[0],

                    DragText = prop.Name
                }

            ).ToList();

            // Create a lookup keying each element type to the properties of that type. This will allow
            // us to build hyperlink targets allowing the user to click between associations:
            var elementTypeLookup = topLevelProps.ToLookup(tp => (Type)tp.Tag);

            // Populate the columns (properties) of each entity:
            foreach (ExplorerItem table in topLevelProps)
                table.Children = ((Type)table.Tag)
                    .GetProperties()
                    .Select(childProp => GetChildItem(elementTypeLookup, childProp))
                    .OrderBy(childItem => childItem.Kind)
                    .ToList();

            return topLevelProps;
        }

        ExplorerItem GetChildItem(ILookup<Type, ExplorerItem> elementTypeLookup, PropertyInfo childProp)
        {
            // If the property's type is in our list of entities, then it's a Many:1 (or 1:1) reference.
            // We'll assume it's a Many:1 (we can't reliably identify 1:1s purely from reflection).
            if (elementTypeLookup.Contains(childProp.PropertyType))
                return new ExplorerItem(childProp.Name, ExplorerItemKind.ReferenceLink, ExplorerIcon.ManyToOne)
                {
                    HyperlinkTarget = elementTypeLookup[childProp.PropertyType].First(),
                    // FormatTypeName is a helper method that returns a nicely formatted type name.
                    ToolTipText = FormatTypeName(childProp.PropertyType, true),
                    DragText = childProp.Name
                };

            // Is the property's type a collection of entities?
            Type ienumerableOfT = childProp.PropertyType.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (ienumerableOfT != null)
            {
                Type elementType = ienumerableOfT.GetGenericArguments()[0];
                if (elementTypeLookup.Contains(elementType))
                    return new ExplorerItem(childProp.Name, ExplorerItemKind.CollectionLink, ExplorerIcon.OneToMany)
                    {
                        HyperlinkTarget = elementTypeLookup[elementType].First(),
                        ToolTipText = FormatTypeName(elementType, true),
                        DragText = childProp.Name
                    };
            }

            // Ordinary property:
            return new ExplorerItem(childProp.Name + " (" + FormatTypeName(childProp.PropertyType, false) + ")",
                ExplorerItemKind.Property, ExplorerIcon.Column)
            {
                DragText = childProp.Name
            };
        }

        #endregion


        #region creating context
        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            return new[] { new ParameterDescriptor("service", "Microsoft.Xrm.Sdk.IOrganizationService") };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            var connString = cxInfo.DatabaseInfo.CustomCxString;
            var connection = new CrmServiceClient(connString);

            return new object[] { connection };
        }
        #endregion
    }
}
