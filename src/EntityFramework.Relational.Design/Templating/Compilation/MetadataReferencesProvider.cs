// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Utilities;

#if DNX451 || DNXCORE50
using System.IO;
using System.Linq;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Compilation;
using Microsoft.Framework.Runtime.Roslyn;
#endif

namespace Microsoft.Data.Entity.Relational.Design.Templating.Compilation
{
    public class MetadataReferencesProvider
    {
        private List<MetadataReference> _references = new List<MetadataReference>();
        private IServiceProvider _serviceProvider;

        public MetadataReferencesProvider([NotNull]IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
            AddDefaultReferences();
        }

        public virtual List<MetadataReference> GetApplicationReferences()
        {
            return _references;
        }

        public virtual void AddDefaultReferences()
        {
            _references.Add(MetadataReference.CreateFromAssembly(
                Assembly.Load(new AssemblyName("mscorlib"))));
            _references.Add(MetadataReference.CreateFromAssembly(
                Assembly.Load(new AssemblyName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))));
            _references.Add(MetadataReference.CreateFromAssembly(
                Assembly.Load(new AssemblyName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))));
            _references.Add(MetadataReference.CreateFromAssembly(
                Assembly.Load(new AssemblyName("Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"))));
            AddReferenceFromName("EntityFramework.Relational.Design");
        }

        public virtual void AddReferenceFromName([NotNull]string name)
        {
            Check.NotEmpty(name, nameof(name));

#if DNX451 || DNXCORE50
            var libraryManager = _serviceProvider.GetRequiredService<ILibraryManager>();
            foreach(var metadataReference in libraryManager.GetLibraryExport(name).MetadataReferences)
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
            }

            throw new InvalidOperationException(Strings.UnableToCreateMetadataReference(name));
#else
            _references.Add(MetadataReference.CreateFromAssembly(Assembly.Load(name)));
#endif
        }
    }
}