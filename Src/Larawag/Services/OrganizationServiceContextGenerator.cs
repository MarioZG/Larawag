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
        public Task<object> GenerateCode(string connectionString, string outFile)
        {
            string wd = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(OrganizationServiceContextGenerator)).Location);
            ProcessStartInfo startInfo = new ProcessStartInfo(wd+"\\CrmSvcUtil.exe")
            {
                Arguments = $"/connectionstring:\"{connectionString}\" /out:\"{outFile}\" /servicecontextname:CrmContext",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = wd
            };

            // there is no non-generic TaskCompletionSource
                var tcs = new TaskCompletionSource<object>();

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                StringBuilder output = new StringBuilder();
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    output.AppendLine(line);
                }
                output.AppendLine("ERRORS:");
                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    output.AppendLine(line);
                }

                //errror - process.ExitCode ==2

                tcs.SetResult(true);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
