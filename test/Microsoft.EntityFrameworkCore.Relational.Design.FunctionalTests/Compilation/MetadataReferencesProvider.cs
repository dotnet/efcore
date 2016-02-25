// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.EntityFrameworkCore.Internal;

#if DNX451
using Microsoft.Extensions.CompilationAbstractions;
using Microsoft.Dnx.Compilation.CSharp;
#elif DNXCORE50
using Microsoft.Extensions.DependencyModel;
#endif

namespace Microsoft.EntityFrameworkCore.Relational.Design.FunctionalTests.Compilation
{
    public class MetadataReferencesProvider
    {

#if DNXCORE50
        private static readonly DependencyContext _dependencyContext =
            DependencyContext.Load(typeof(MetadataReferencesProvider).GetTypeInfo().Assembly);
#endif

        private bool _isInitialized;
        private readonly List<MetadataReference> _references = new List<MetadataReference>();

        public virtual List<MetadataReference> GetApplicationReferences()
        {
            if (!_isInitialized)
            {
                InitializeReferences();
            }

            return _references;
        }

        private void InitializeReferences()
        {
            _isInitialized = true;

#if DNXCORE50
            AddReferenceFromName("System.Collections");
            AddReferenceFromName("System.Dynamic.Runtime");
            AddReferenceFromName("System.Linq");
            AddReferenceFromName("System.Runtime");
            AddReferenceFromName("System.Threading.Tasks");
            AddReferenceFromName("Microsoft.CSharp");
#else
            _references.Add(MetadataReference.CreateFromFile(
                Assembly.Load(new AssemblyName("mscorlib")).Location));
            _references.Add(MetadataReference.CreateFromFile(
                Assembly.Load(new AssemblyName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location));
            _references.Add(MetadataReference.CreateFromFile(
                Assembly.Load(new AssemblyName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location));
            _references.Add(MetadataReference.CreateFromFile(
                Assembly.Load(new AssemblyName("Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")).Location));
#endif
            AddReferenceFromName("Microsoft.EntityFrameworkCore");
            AddReferenceFromName("Microsoft.EntityFrameworkCore.Relational.Design");
        }

        public virtual void Add(MetadataReference reference)
            => _references.Add(reference);

        public virtual void AddReferenceFromName(string name)
        {
            if (!_isInitialized)
            {
                InitializeReferences();
            }

#if DNX451
            if (CompilationServices.Default != null)
            {
                var libraryExport = CompilationServices.Default.LibraryExporter.GetExport(name);
                if (libraryExport != null)
                {
                    foreach(var metadataReference in libraryExport.MetadataReferences)
                    {
                        var roslynReference = metadataReference as IRoslynMetadataReference;
                        if (roslynReference != null)
                        {
                            _references.Add(roslynReference.MetadataReference);
                            return;
                        }

                        var fileMetadataReference = metadataReference as IMetadataFileReference;
                        if (fileMetadataReference != null)
                        {
                            var metadata = AssemblyMetadata.CreateFromStream(File.OpenRead(fileMetadataReference.Path));
                            _references.Add(metadata.GetReference());
                            return;
                        }

                        var metadataProjectReference = metadataReference as IMetadataProjectReference;
                        if (metadataProjectReference != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                metadataProjectReference.EmitReferenceAssembly(stream);

                                _references.Add(MetadataReference.CreateFromStream(stream));
                                return;
                            }
                        }
                    }
                }
            }
#elif DNXCORE50
            if (_dependencyContext != null)
            {
                var library = _dependencyContext
                    .CompileLibraries
                    .FirstOrDefault(l => l.PackageName.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (library != null)
                {
                    _references.AddRange(library.ResolveReferencePaths().Select(file => MetadataReference.CreateFromFile(file)));
                }
            }
#else
            _references.Add(MetadataReference.CreateFromFile(Assembly.Load(name).Location));
#endif
        }
    }
}
