using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.Services
{
    public class OrganizationServiceContextGenerator : IOrganizationServiceContextGenerator
    {
        public event DataReceivedEventHandler ErrorDataReceived;
        public event DataReceivedEventHandler OutputDataReceived;

        public Task<bool> GenerateCode(string connectionString, string outFile)
        {
            string wd = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(OrganizationServiceContextGenerator)).Location);
            ProcessStartInfo startInfo = new ProcessStartInfo(wd+"\\CrmSvcUtil.exe")
            {
                Arguments = $"/connectionstring:\"{connectionString}\" /out:\"{outFile}\" /servicecontextname:CrmContext",
                UseShellExecute = false,
             //   RedirectStandardOutput = true,
             //   RedirectStandardError = true,
                WorkingDirectory = wd
            };

            // there is no non-generic TaskCompletionSource
                var tcs = new TaskCompletionSource<bool>();

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                if (process.ExitCode == 0)
                {
                    tcs.SetResult(true);
                }
                else
                {
                    tcs.SetResult(false);
                }
              
                process.Dispose();
            };

            //process.OutputDataReceived += (sender, args) =>
            //{
            //    OutputDataReceived?.Invoke(sender, args);
            //};

            //process.ErrorDataReceived += (sender, args) =>
            //{
            //    ErrorDataReceived?.Invoke(sender, args);
            //};

            process.Start();

            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();

            //tcs.SetResult(true);


            return tcs.Task;
        }

        public string GetWorkingFolder(IDatabaseInfo dbInfo)
        {
            string workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(OrganizationServiceContextGenerator)).Location);
            string hostname = new Uri(dbInfo.Server).Host;
            workingDirectory = Path.Combine(workingDirectory, hostname);
            return workingDirectory;
        }
    }
}
