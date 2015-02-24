// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public abstract class MetadataBase : IMetadata
    {
        // TODO: Perf: use a mutable structure before the model is made readonly
        // Issue #868 
        private readonly LazyRef<ImmutableSortedSet<Annotation>> _annotations
            = new LazyRef<ImmutableSortedSet<Annotation>>(
                () => ImmutableSortedSet<Annotation>.Empty.WithComparer(new AnnotationComparer()));

        public virtual Annotation AddAnnotation([NotNull] string annotationName, [NotNull] string value)
        {
            Check.NotNull(annotationName, nameof(annotationName));
            Check.NotNull(value, nameof(value));

            var annotation = new Annotation(annotationName, value);

            var previousLength = _annotations.Value.Count;
            _annotations.Value = _annotations.Value.Add(annotation);

            if (previousLength == _annotations.Value.Count)
            {
                throw new InvalidOperationException(Strings.DuplicateAnnotation(annotationName));
            }

            return annotation;
        }

        public virtual Annotation GetOrAddAnnotation([NotNull] string annotationName, [NotNull] string value)
        {
            return TryGetAnnotation(annotationName) ?? AddAnnotation(annotationName, value);
        }

        [CanBeNull]
        public virtual Annotation TryGetAnnotation([NotNull] string annotationName)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            Annotation annotation;
            return _annotations.HasValue
                   && _annotations.Value.TryGetValue(new Annotation(annotationName, "_"), out annotation)
                ? annotation
                : null;
        }

        public virtual Annotation GetAnnotation(string annotationName)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            var annotation = TryGetAnnotation(annotationName);
            if (annotation == null)
            {
                throw new ModelItemNotFoundException(Strings.AnnotationNotFound(annotationName));
            }

            return annotation;
        }

        public virtual Annotation RemoveAnnotation([NotNull] Annotation annotation)
        {
            Check.NotNull(annotation, nameof(annotation));

            var previousAnnotations = _annotations.Value;
            _annotations.Value = _annotations.Value.Remove(annotation);

            Annotation removedAnnotations = null;
            if (previousAnnotations.Count != _annotations.Value.Count)
            {
                previousAnnotations.TryGetValue(annotation, out removedAnnotations);
            }

            return removedAnnotations;
        }

        // ReSharper disable once AnnotationRedundanceInHierarchy
        public virtual string this[[NotNull] string annotationName]
        {
            get { return TryGetAnnotation(annotationName)?.Value; }
            [param: CanBeNull]
            set
            {
                Check.NotEmpty(annotationName, nameof(annotationName));

                _annotations.Value = _annotations.Value.Remove(new Annotation(annotationName, "_"));

                if (value != null)
                {
                    AddAnnotation(annotationName, value);
                }
            }
        }

        public virtual IEnumerable<Annotation> Annotations
            => _annotations.HasValue
                ? (IEnumerable<Annotation>)_annotations.Value
                : ImmutableList<Annotation>.Empty;

        private class AnnotationComparer : IComparer<IAnnotation>
        {
            public int Compare(IAnnotation x, IAnnotation y)
            {
                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }
        }

        IEnumerable<IAnnotation> IMetadata.Annotations => Annotations;
    }
}
