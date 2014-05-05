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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Annotations : IEnumerable<Annotation>
    {
        private readonly LazyRef<ImmutableSortedSet<Annotation>> _annotations
            = new LazyRef<ImmutableSortedSet<Annotation>>(
                () => ImmutableSortedSet<Annotation>.Empty.WithComparer(new AnnotationComparer()));

        public virtual void Add([NotNull] Annotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.Value = _annotations.Value.Remove(annotation).Add(annotation);
        }

        public virtual void Remove([NotNull] Annotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.Value = _annotations.Value.Remove(annotation);
        }

        // ReSharper disable once AnnotationRedundanceInHierarchy
        public virtual string this[[param: NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName");

                Annotation value;
                return _annotations.HasValue
                       && _annotations.Value.TryGetValue(new Annotation(annotationName, "_"), out value)
                    ? value.Value
                    : null;
            }
            [param: NotNull]
            set
            {
                Check.NotEmpty(annotationName, "annotationName");
                Check.NotEmpty(value, "value");

                var annotation = new Annotation(annotationName, value);
                _annotations.Value = _annotations.Value.Remove(annotation).Add(annotation);
            }
        }

        public IEnumerator<Annotation> GetEnumerator()
        {
            return _annotations.HasValue
                ? (IEnumerator<Annotation>)_annotations.Value.GetEnumerator()
                : ImmutableList<Annotation>.Empty.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class AnnotationComparer : IComparer<Annotation>
        {
            public int Compare(Annotation x, Annotation y)
            {
                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }
        }
    }
}
