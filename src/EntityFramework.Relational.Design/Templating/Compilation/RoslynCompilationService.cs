// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.Templating.Compilation
{
    public class RoslynCompilationService : ICompilationService
    {
        public virtual CompilationResult Compile([NotNull] string content, [NotNull] List<MetadataReference> references)
        {
            Check.NotEmpty(content, nameof(content));
            Check.NotNull(references, nameof(references));

            var syntaxTrees = new[] { CSharpSyntaxTree.ParseText(content) };

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

                return CompilationResult.Successful(string.Empty, type);
            }
            return CompilationResult.Failed(content, result.ErrorMessages);
        }

        public static CompiledAssemblyResult GetAssemblyFromCompilation(
            [NotNull] CSharpCompilation compilation)
        {
            Check.NotNull(compilation, nameof(compilation));

            EmitResult result;
            using (var ms = new MemoryStream())
            {
                using (var pdb = new MemoryStream())
                {
                    if (PlatformHelper.IsMono)
                    {
                        result = compilation.Emit(ms, pdbStream: null);
                    }
                    else
                    {
                        result = compilation.Emit(ms, pdbStream: pdb);
                    }

                    if (!result.Success)
                    {
                        var formatter = new DiagnosticFormatter();
                        var errorMessages = result.Diagnostics
                            .Where(IsError)
                            .Select(d => formatter.Format(d));

                        return CompiledAssemblyResult.FromErrorMessages(errorMessages);
                    }

                    Assembly assembly;
                    if (PlatformHelper.IsMono)
                    {
                        var assemblyLoadMethod = typeof(Assembly).GetTypeInfo().GetDeclaredMethods("Load")
                            .First(
                                m =>
                                    {
                                        var parameters = m.GetParameters();
                                        return parameters.Length == 1 && parameters[0].ParameterType == typeof(byte[]);
                                    });
                        assembly = (Assembly)assemblyLoadMethod.Invoke(null, new[] { ms.ToArray() });
                    }
                    else
                    {
                        var assemblyLoadMethod = typeof(Assembly).GetTypeInfo().GetDeclaredMethods("Load")
                            .First(
                                m =>
                                    {
                                        var parameters = m.GetParameters();
                                        return parameters.Length == 2
                                               && parameters[0].ParameterType == typeof(byte[])
                                               && parameters[1].ParameterType == typeof(byte[]);
                                    });
                        assembly = (Assembly)assemblyLoadMethod.Invoke(null, new[] { ms.ToArray(), pdb.ToArray() });
                    }

                    return CompiledAssemblyResult.FromAssembly(assembly);
                }
            }
        }

        private static bool IsError([NotNull] Diagnostic diagnostic)
        {
            Check.NotNull(diagnostic, nameof(diagnostic));

            return diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error;
        }
    }
}
