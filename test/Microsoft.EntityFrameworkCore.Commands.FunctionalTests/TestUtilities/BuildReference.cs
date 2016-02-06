// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Extensions.CompilationAbstractions;
using IOPath = System.IO.Path;

namespace Microsoft.EntityFrameworkCore.Commands.TestUtilities
{
    public class BuildReference
    {
        public BuildReference(MetadataReference reference, bool copyLocal = false, string path = null)
        {
            Reference = reference;
            CopyLocal = copyLocal;
            Path = path;
        }

        public MetadataReference Reference { get; }

        public bool CopyLocal { get; }

        public string Path { get; }

        public static BuildReference ByName(string name, bool copyLocal = false)
        {
            if (CompilationServices.Default != null)
            {
                var library = CompilationServices.Default.LibraryExporter.GetExport(name);
                if (library != null)
                {
                    var metadataReference = library.MetadataReferences.Single();

                    var roslynMetadataReference = metadataReference as IRoslynMetadataReference;
                    if (roslynMetadataReference != null)
                    {
                        if (copyLocal)
                        {
                            throw new InvalidOperationException(
                                $"In-memory assembly '{name}' cannot be copied locally.");
                        }

                        return new BuildReference(roslynMetadataReference.MetadataReference);
                    }

                    var metadataFileReference = metadataReference as IMetadataFileReference;
                    if (metadataFileReference != null)
                    {
                        return ByPath(metadataFileReference.Path, copyLocal);
                    }

                    var metadataProjectReference = metadataReference as IMetadataProjectReference;
                    if (metadataProjectReference != null)
                    {
                        if (copyLocal)
                        {
                            throw new InvalidOperationException(
                                $"In-memory assembly '{name}' cannot be copied locally.");
                        }

                        using (var stream = new MemoryStream())
                        {
                            metadataProjectReference.EmitReferenceAssembly(stream);

                            return new BuildReference(MetadataReference.CreateFromStream(stream));
                        }
                    }
                }
            }
#if NET451
            var assembly = Assembly.Load(name);
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                return new BuildReference(
                    MetadataReference.CreateFromFile(assembly.Location),
                    copyLocal,
                    new Uri(assembly.CodeBase).LocalPath);
            }
#endif

            throw new InvalidOperationException(
                $"Assembly '{name}' not found.");
        }

        public static BuildReference ByPath(string path, bool copyLocal = false)
        {
            return new BuildReference(
                MetadataReference.CreateFromFile(path),
                copyLocal,
                path);
        }
    }
}
