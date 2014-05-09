// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
