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
    public class Annotations : IEnumerable<IAnnotation>
    {
        private readonly LazyRef<ImmutableSortedSet<IAnnotation>> _annotations
            = new LazyRef<ImmutableSortedSet<IAnnotation>>(
                () => ImmutableSortedSet<IAnnotation>.Empty.WithComparer(new AnnotationComparer()));

        public virtual void Add([NotNull] IAnnotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.Value = _annotations.Value.Remove(annotation).Add(annotation);
        }

        public virtual void Remove([NotNull] IAnnotation annotation)
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

                IAnnotation value;
                return _annotations.HasValue
                       && _annotations.Value.TryGetValue(new Annotation(annotationName, "_"), out value)
                    ? value.Value
                    : null;
            }
            [param: CanBeNull]
            set
            {
                Check.NotEmpty(annotationName, "annotationName");

                var annotation = new Annotation(annotationName, value ?? "_");
                
                var afterRemove = _annotations.Value.Remove(annotation);

                _annotations.Value = value == null ? afterRemove : afterRemove.Add(annotation);
            }
        }

        public virtual IEnumerator<IAnnotation> GetEnumerator()
        {
            return _annotations.HasValue
                ? (IEnumerator<IAnnotation>)_annotations.Value.GetEnumerator()
                : ImmutableList<IAnnotation>.Empty.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class AnnotationComparer : IComparer<IAnnotation>
        {
            public int Compare(IAnnotation x, IAnnotation y)
            {
                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }
        }
    }
}
