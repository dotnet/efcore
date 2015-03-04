// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Framework.Runtime.Roslyn;

#if ASPNET50
using System.Linq;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
#endif

namespace Microsoft.Data.Entity.Commands.TestUtilities
{
    public class BuildReference
    {
        private readonly MetadataReference _reference;
        private readonly bool _copyLocal;
        private readonly string _path;

        public BuildReference(MetadataReference reference, bool copyLocal = false, string path = null)
        {
            _reference = reference;
            _copyLocal = copyLocal;
            _path = path;
        }

        public MetadataReference Reference
        {
            get { return _reference; }
        }

        public bool CopyLocal
        {
            get { return _copyLocal; }
        }

        public string Path
        {
            get { return _path; }
        }

        public static BuildReference ByName(string name, bool copyLocal = false)
        {
            var assembly = Assembly.Load(name);
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                return new BuildReference(
                    MetadataReference.CreateFromAssembly(assembly),
                    copyLocal,
                    new Uri(assembly.CodeBase).LocalPath);
            }
            if (copyLocal)
            {
                throw new InvalidOperationException(
                    string.Format("In-memory assembly '{0}' cannot be copied locally.", name));
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
#if ASPNET50
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
                string.Format("In-memory assembly '{0}' cannot be referenced.", name));
#endif
        }
    }
}
