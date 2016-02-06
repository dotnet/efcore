// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

#if NET451 || DNXCORE50
using System;
using System.IO;
using Microsoft.Extensions.CompilationAbstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Dnx.Compilation.CSharp;
#endif

namespace Microsoft.EntityFrameworkCore.Relational.Design.FunctionalTests.Compilation
{
    public class MetadataReferencesProvider
    {
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

#if NET451 || DNXCORE50
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
#endif
#if NET451
            _references.Add(MetadataReference.CreateFromFile(Assembly.Load(name).Location));
#else
            throw new InvalidOperationException("Unable to create metadata reference from name: " + name);
#endif
        }
    }
}
