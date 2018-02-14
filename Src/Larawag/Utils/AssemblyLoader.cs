using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.Utils
{

    public class AssemblyLoader : MarshalByRefObject
    {
        IEnumerable<Type> CallInternal(string dll, Func<Type, bool> predicate)
        {
            Assembly a = Assembly.LoadFile(dll);
            return a.ExportedTypes.Where(predicate).ToList();            
        }

        public static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("Microsoft.Xrm.Sdk,"))
            {
                var assembly = Assembly.LoadFrom("Microsoft.Xrm.Sdk.dll");
                return assembly;
            }
            return null;
        }

        internal static IEnumerable<Type> GetTypesFromAssembly(
            string customAssemblyPath, 
            Func<Type, bool> predicate)
        {
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/3d072d04-c2e8-4dd6-ac2c-e9e20adb0fb4/c-load-dll-in-separate-domain-and-use-its-methods?forum=csharpgeneral

            AppDomainSetup domaininfo = AppDomain.CurrentDomain.SetupInformation;
            //domaininfo.ApplicationBase = Assembly.GetExecutingAssembly().Location;
         //   domaininfo.PrivateBinPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain dom = AppDomain.CreateDomain("IsolatedDomain", null, domaininfo);
            dom.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //(object sender, ResolveEventArgs args) => {
            //    if (args.Name.StartsWith("Microsoft.Xrm.Sdk,"))
            //    {
            //        var assembly = Assembly.LoadFrom("Microsoft.Xrm.Sdk.dll");
            //        return assembly;
            //    }
            //    return null;
            //};
           // dom.Load(Assembly.GetExecutingAssembly().)
            AssemblyLoader ld = (AssemblyLoader)dom.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AssemblyLoader).FullName);
            var result = ld.CallInternal(customAssemblyPath, predicate);
            AppDomain.Unload(dom);
            return result;
        }
    }
}
