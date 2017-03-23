// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

#if !NET46
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using IOPath = System.IO.Path;
#endif
namespace Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities
{
    public class BuildReference
    {
        private BuildReference(IEnumerable<MetadataReference> references, bool copyLocal = false, string path = null)
        {
            References = references;
            CopyLocal = copyLocal;
            Path = path;
        }

        public IEnumerable<MetadataReference> References { get; }

        public bool CopyLocal { get; }
        public string Path { get; }

        public static BuildReference ByName(string name, bool copyLocal = false)
        {
#if NET46
            var assembly = Assembly.Load(name);
            return new BuildReference(
                new[] { MetadataReference.CreateFromFile(assembly.Location) },
                copyLocal,
                new Uri(assembly.CodeBase).LocalPath);
#elif NETSTANDARD1_6 || NETCOREAPP1_1
            var references = Enumerable.ToList(
                from l in DependencyContext.Default.CompileLibraries
                from r in l.ResolveReferencePaths()
                where IOPath.GetFileNameWithoutExtension(r) == name
                select MetadataReference.CreateFromFile(r));
            if (references.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Assembly '{name}' not found.");
            }

            return new BuildReference(
                references,
                copyLocal);
#else
#error target frameworks need to be updated.
#endif
        }

        public static BuildReference ByPath(string path)
            => new BuildReference(new[] { MetadataReference.CreateFromFile(path) }, path: path);
    }
}
