// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Entity.Commands.Tests.Migrations
{
#if VSBUILD

    // TODO: Use Roslyn when the System.Collections.Immutable version conflict is resolved.
    // Currently Microsoft.CodeAnalysis.CSharp uses 1.1.20 while EntityFramework uses 1.1.22.

    using System.CodeDom.Compiler;
    using Microsoft.CSharp;

    public static class CodeGeneratorTestHelper
    {
        public static Assembly Compile(string assemblyName, IEnumerable<string> sources, IEnumerable<string> references)
        {
            var compilerResults = new CSharpCodeProvider().CompileAssemblyFromSource(
                new CompilerParameters(references.Select(r => r + ".dll").ToArray())
                    {
                        GenerateInMemory = true,
                        GenerateExecutable = false
                    },
                sources.ToArray());

            if (compilerResults.Errors.Count > 0)
            {
                throw new InvalidOperationException(compilerResults.Errors[0].ToString());
            }

            return compilerResults.CompiledAssembly;
        }
    }

#else

    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Infrastructure;

    public static class CodeGeneratorTestHelper
    {
        public static Assembly Compile(string assemblyName, IEnumerable<string> sources, IEnumerable<string> references)
        {
            var compilation = CSharpCompilation.Create(
                assemblyName,
                sources.Select(s => SyntaxFactory.ParseSyntaxTree(s)),
                GetMetadataReferences(references),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);
                var diagnostics = emitResult.Diagnostics
                    .Where(d => d.Severity > DiagnosticSeverity.Info)
                    .ToArray();

                if (diagnostics.Any())
                {
                    throw new InvalidOperationException(diagnostics.First().ToString());
                }

                return Assembly.Load(stream.GetBuffer());
            }
        }

        private static IEnumerable<MetadataReference> GetMetadataReferences(IEnumerable<string> references)
        {
            var libraryManager = CallContextServiceLocator.Locator.ServiceProvider.GetService<ILibraryManager>();

            foreach (var reference in references.SelectMany(n => libraryManager.GetLibraryExport(n).MetadataReferences))
            {
                var fileReference = reference as IMetadataFileReference;

                yield return 
                    fileReference != null
                        ? new MetadataFileReference(fileReference.Path)
                        : ((IRoslynMetadataReference)reference).MetadataReference;
            }
        }
    }

#endif
}
