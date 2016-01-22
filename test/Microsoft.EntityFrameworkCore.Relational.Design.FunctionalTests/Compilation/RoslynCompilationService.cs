// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.EntityFrameworkCore.Relational.Design.FunctionalTests.Compilation
{
    public class RoslynCompilationService
    {
        public virtual CompilationResult Compile(
            IEnumerable<string> contents, List<MetadataReference> references)
        {
            var syntaxTrees = contents
                .Select(content => CSharpSyntaxTree.ParseText(content));

            var assemblyName = Path.GetRandomFileName();

            var compilation = CSharpCompilation.Create(assemblyName,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: syntaxTrees,
                references: references);

            var result = GetAssemblyFromCompilation(compilation);
            if (result.Success)
            {
                var type = result.Assembly.GetExportedTypes()
                    .First();

                return CompilationResult.Successful(type);
            }
            return CompilationResult.Failed(result.ErrorMessages);
        }

        public static CompiledAssemblyResult GetAssemblyFromCompilation(
            CSharpCompilation compilation)
        {
            EmitResult result;
            using (var ms = new MemoryStream())
            {
                result = compilation.Emit(ms, pdbStream: null);

                if (!result.Success)
                {
                    var formatter = new DiagnosticFormatter();
                    var errorMessages = result.Diagnostics
                        .Where(IsError)
                        .Select(d => formatter.Format(d));

                    return CompiledAssemblyResult.FromErrorMessages(errorMessages);
                }

                Assembly assembly;
                var assemblyLoadMethod = typeof(Assembly).GetTypeInfo().GetDeclaredMethods("Load")
                    .First(
                        m =>
                            {
                                var parameters = m.GetParameters();
                                return parameters.Length == 1 && parameters[0].ParameterType == typeof(byte[]);
                            });
                assembly = (Assembly)assemblyLoadMethod.Invoke(null, new[] { ms.ToArray() });
                return CompiledAssemblyResult.FromAssembly(assembly);
            }
        }

        private static bool IsError(Diagnostic diagnostic)
        {
            return diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error;
        }
    }
}
