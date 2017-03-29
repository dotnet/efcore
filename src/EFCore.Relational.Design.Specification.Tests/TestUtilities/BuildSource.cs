// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities
{
    public class BuildSource
    {
        public ICollection<BuildReference> References { get; } = new List<BuildReference>
        {
#if NET46
            BuildReference.ByName("mscorlib"),
            BuildReference.ByName("System.Runtime, Version=4.0.20.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
#elif NETSTANDARD1_6 || NETCOREAPP2_0
            BuildReference.ByName("System.Runtime")
#else
#error target frameworks need to be updated.
#endif
        };

        public string TargetDir { get; set; }
        public ICollection<string> Sources { get; set; } = new List<string>();

        public BuildFileResult Build()
        {
            var projectName = Path.GetRandomFileName();
            var references = new List<MetadataReference>();

            foreach (var reference in References)
            {
                if (reference.CopyLocal)
                {
                    if (string.IsNullOrEmpty(reference.Path))
                    {
                        throw new InvalidOperationException("Could not find path for reference " + reference);
                    }
                    File.Copy(reference.Path, Path.Combine(TargetDir, Path.GetFileName(reference.Path)), overwrite: true);
                }

                references.AddRange(reference.References);
            }

            var compilation = CSharpCompilation.Create(
                projectName,
                Sources.Select(s => SyntaxFactory.ParseSyntaxTree(s)),
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var targetPath = Path.Combine(TargetDir ?? Path.GetTempPath(), projectName + ".dll");

            using (var stream = File.OpenWrite(targetPath))
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Build failed. Diagnostics: {string.Join(Environment.NewLine, result.Diagnostics)}");
                }
            }

            return new BuildFileResult(targetPath);
        }

        public Assembly BuildInMemory()
        {
            var projectName = Path.GetRandomFileName();
            var references = new List<MetadataReference>();

            foreach (var reference in References)
            {
                if (reference.CopyLocal)
                {
                    throw new InvalidOperationException("Assemblies cannot be copied locally for in-memory builds.");
                }

                references.AddRange(reference.References);
            }

            var compilation = CSharpCompilation.Create(
                projectName,
                Sources.Select(s => SyntaxFactory.ParseSyntaxTree(s)),
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            Assembly assembly;
            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Build failed. Diagnostics: {string.Join(Environment.NewLine, result.Diagnostics)}");
                }

#if NET46
                assembly = Assembly.Load(stream.ToArray());
#elif NETSTANDARD1_6 || NETCOREAPP2_0
                assembly = (Assembly)typeof(Assembly).GetTypeInfo().GetDeclaredMethods("Load")
                    .First(
                        m =>
                        {
                            var parameters = m.GetParameters();

                            return parameters.Length == 1 && parameters[0].ParameterType == typeof(byte[]);
                        })
                    .Invoke(null, new[] { stream.ToArray() });
#else
#error target frameworks need to be updated.
#endif
            }

            return assembly;
        }
    }
}
