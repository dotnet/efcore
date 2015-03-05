// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Data.Entity.Commands.Tests.Migrations
{
#if VSBUILD
    public static partial class CodeGeneratorTestHelper
    {
        private static IEnumerable<MetadataReference> GetMetadataReferences(IEnumerable<string> references)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            return references.Select(n => MetadataReference.CreateFromFile(
                assemblies.Single(a => a.GetName().Name == n).Location));
        }
    }
#else
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.Runtime.Infrastructure;
    using Microsoft.Framework.Runtime.Roslyn;

    public static partial class CodeGeneratorTestHelper
    {
        private static IEnumerable<MetadataReference> GetMetadataReferences(IEnumerable<string> references)
        {
            var libraryManager = CallContextServiceLocator.Locator.ServiceProvider.GetService<ILibraryManager>();

            return references.SelectMany(n => libraryManager.GetLibraryExport(n).MetadataReferences).Select(r => 
                {
                    var fileReference = r as IMetadataFileReference;
                    return fileReference != null
                        ? MetadataReference.CreateFromFile(fileReference.Path)
                        : ((IRoslynMetadataReference)r).MetadataReference;
                });
        }
    }
#endif

    public static partial class CodeGeneratorTestHelper
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
    }
}
