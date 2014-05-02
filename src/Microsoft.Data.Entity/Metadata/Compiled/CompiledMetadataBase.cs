// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
