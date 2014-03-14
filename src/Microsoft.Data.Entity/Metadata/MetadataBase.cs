// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public abstract class MetadataBase : IMetadata
    {
        private readonly LazyRef<ImmutableSortedSet<Annotation>> _annotations
            = new LazyRef<ImmutableSortedSet<Annotation>>(
                () => ImmutableSortedSet<Annotation>.Empty.WithComparer(new AnnotationComparer()));

        public virtual string StorageName { get; [param: CanBeNull] set; }

        public virtual void AddAnnotation([NotNull] Annotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.Value = _annotations.Value.Remove(annotation).Add(annotation);
        }

        public virtual void RemoveAnnotation([NotNull] Annotation annotation)
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

        public virtual IReadOnlyList<Annotation> Annotations
        {
            get
            {
                return _annotations.HasValue
                    ? (IReadOnlyList<Annotation>)_annotations.Value
                    : ImmutableList<Annotation>.Empty;
            }
        }

        IReadOnlyList<IAnnotation> IMetadata.Annotations
        {
            get { return Annotations; }
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
