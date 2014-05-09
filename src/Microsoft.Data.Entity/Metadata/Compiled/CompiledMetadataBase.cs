// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledMetadataBase
    {
        private IAnnotation[] _annotations;

        public IEnumerable<IAnnotation> Annotations
        {
            get { return LazyInitializer.EnsureInitialized(ref _annotations, LoadAnnotations); }
        }

        public string this[[NotNull] string annotationName]
        {
            get
            {
                var annotation = Annotations.FirstOrDefault(a => a.Name == annotationName);
                return annotation == null ? null : annotation.Value;
            }
        }

        protected abstract IAnnotation[] LoadAnnotations();

        protected static IEnumerable<IAnnotation> ZipAnnotations([NotNull] string[] names, [NotNull] string[] values)
        {
            return names.Zip(values, (n, v) => new Annotation(n, v)).ToArray();
        }

        public virtual string StorageName
        {
            get { return null; }
        }
    }
}
