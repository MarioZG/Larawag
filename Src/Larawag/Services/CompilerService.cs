using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.Services
{
    public class CompilerService : ICompilerService
    {
        public async Task<ImmutableArray<Diagnostic>> CompileCode(string filename, string outputFileName, string logFile)
        {
            SyntaxTree programTree = await Task.Run(()=> CSharpSyntaxTree.ParseText(File.ReadAllText(filename))); ;

            //fix for service generation issue where some field have same sname as const woth logical name
            var invalidProps = programTree.GetRoot().ChildNodes().Where(c => c.GetType() == typeof(ClassDeclarationSyntax))
            .SelectMany(c => ((ClassDeclarationSyntax)c).ChildNodes().Where(cn => cn.GetType() == typeof(PropertyDeclarationSyntax) && ((PropertyDeclarationSyntax)cn).Identifier.Text == "EntityLogicalName")
                           .Select(cn => ((PropertyDeclarationSyntax)cn)));

            programTree = await Task.Run(() => CSharpSyntaxTree.ParseText(programTree.GetRoot().RemoveNodes(invalidProps, SyntaxRemoveOptions.KeepEndOfLine).GetText()));

            //return;

            SyntaxTree[] sourceTrees = { programTree };



                MetadataReference[] references = new MetadataReference[]{
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SyntaxTree).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(CSharpSyntaxTree).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.INotifyPropertyChanging).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.Serialization.DataContractAttribute).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.IQueryable<>).GetTypeInfo().Assembly.Location),

                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(filename), "..\\Microsoft.Xrm.Sdk.dll"))
            };

            // compilation
            var emitResult = await Task.Run(() => 
                                    CSharpCompilation.Create(Path.GetFileName(outputFileName),
                                         sourceTrees,
                                         references,
                                         new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)).Emit(outputFileName)
                                 );

            SaveCompilationLog(filename, emitResult.Diagnostics, logFile);

            return emitResult.Diagnostics;
        }

        private static void SaveCompilationLog(string filename, ImmutableArray<Diagnostic> results, string logFile)
        {
            File.WriteAllLines(logFile, results.Select(r => r.ToString()).ToArray());
        }
    }
}
