// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

#if DNX451 || DNXCORE50
using System.Linq;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
using Microsoft.Framework.Runtime.Roslyn;
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
            var assembly = Assembly.Load(name);
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                return new BuildReference(
                    MetadataReference.CreateFromFile(assembly.Location),
                    copyLocal,
                    new Uri(assembly.CodeBase).LocalPath);
            }
            if (copyLocal)
            {
                throw new InvalidOperationException(
                    $"In-memory assembly '{name}' cannot be copied locally.");
            }

            return new BuildReference(ResolveReference(name));
        }

        public static BuildReference ByPath(string path, bool copyLocal = false)
        {
            return new BuildReference(
                MetadataReference.CreateFromFile(path),
                copyLocal,
                path);
        }

        private static MetadataReference ResolveReference(string name)
        {
#if DNX451 || DNXCORE50
            return CallContextServiceLocator
                .Locator
                .ServiceProvider
                .GetService<ILibraryManager>()
                .GetLibraryExport(name)
                .MetadataReferences
                .Cast<IRoslynMetadataReference>()
                .Single()
                .MetadataReference;
#else
            throw new InvalidOperationException(
                $"In-memory assembly '{name}' cannot be referenced.");
#endif
        }
    }
}
