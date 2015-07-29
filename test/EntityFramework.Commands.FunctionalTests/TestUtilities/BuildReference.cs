// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

#if DNX451 || DNXCORE50
using System.Diagnostics;
using System.Linq;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Dnx.Compilation.CSharp;
#endif

namespace Microsoft.Data.Entity.Commands.TestUtilities
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
#if DNX451 || DNXCORE50
            var library = CallContextServiceLocator
                .Locator
                .ServiceProvider
                .GetService<ILibraryExporter>()
                .GetLibraryExport(name);
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
                Debug.Assert(
                    metadataFileReference != null,
                    "Unexpected metadata reference type: " + metadataReference.GetType().Name);

                return ByPath(metadataFileReference.Path, copyLocal);
            }
#endif
#if !DNXCORE50
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
