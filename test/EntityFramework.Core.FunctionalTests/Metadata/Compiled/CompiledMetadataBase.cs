// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledMetadataBase
    {
        private IAnnotation[] _annotations;

        public IEnumerable<IAnnotation> Annotations => LazyInitializer.EnsureInitialized(ref _annotations, LoadAnnotations);

        public string this[string annotationName] => Annotations.FirstOrDefault(a => a.Name == annotationName)?.Value;

        protected abstract IAnnotation[] LoadAnnotations();

        protected static IEnumerable<IAnnotation> ZipAnnotations(string[] names, string[] values) => names.Zip(values, (n, v) => new Annotation(n, v)).ToArray();

        public Annotation GetAnnotation(string annotationName) => null;
    }
}
