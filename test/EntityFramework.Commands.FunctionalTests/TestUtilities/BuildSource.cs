// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Data.Entity.Commands.TestUtilities
{
    public class BuildSource
    {
        private readonly ICollection<BuildReference> _references = new List<BuildReference>
            {
                BuildReference.ByName("mscorlib")
            };

        public ICollection<BuildReference> References
        {
            get { return _references; }
        }

        public string Source { get; set; }
        public string TargetDir { get; set; }

        public BuildFileResult Build()
        {
            var projectName = Path.GetRandomFileName();
            var references = new List<MetadataReference>();

            foreach (var reference in _references)
            {
                if (reference.CopyLocal)
                {
                    File.Copy(reference.Path, Path.Combine(TargetDir, Path.GetFileName(reference.Path)));
                }

                references.Add(reference.Reference);
            }

            var compilation = CSharpCompilation.Create(
                projectName,
                new[] { SyntaxFactory.ParseSyntaxTree(Source) },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var targetPath = Path.Combine(TargetDir, projectName + ".dll");

            using (var stream = File.OpenWrite(targetPath))
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    // TODO: Show diagnostics
                    throw new InvalidOperationException("Build failed.");
                }
            }

            return new BuildFileResult(targetPath);
        }
    }
}
