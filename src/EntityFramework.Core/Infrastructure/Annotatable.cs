// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Base class for types that support reading and writing annotations. 
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class Annotatable : IAnnotatable
    {
        // TODO: Perf: use a mutable structure before the model is made readonly
        // Issue #868 
        private readonly LazyRef<ImmutableSortedSet<Annotation>> _annotations
            = new LazyRef<ImmutableSortedSet<Annotation>>(
                () => ImmutableSortedSet<Annotation>.Empty.WithComparer(new AnnotationComparer()));

        /// <summary>
        ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly added annotation. </returns>
        public virtual Annotation AddAnnotation([NotNull] string annotationName, [NotNull] object value)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));
            Check.NotNull(value, nameof(value));

            var annotation = new Annotation(annotationName, value);

            var previousLength = _annotations.Value.Count;
            _annotations.Value = _annotations.Value.Add(annotation);

            if (previousLength == _annotations.Value.Count)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(annotationName));
            }

            return annotation;
        }

        /// <summary>
        ///     Adds an annotation to this object or returns the existing annotation if one with the specified name already exists.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> 
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, the newly added annotation. 
        /// </returns>
        public virtual Annotation GetOrAddAnnotation([NotNull] string annotationName, [NotNull] object value)
            => FindAnnotation(annotationName) ?? AddAnnotation(annotationName, value);

        /// <summary>
        ///     Gets the annotation with the given name, returning null if it does not exist.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, null. 
        /// </returns>
        public virtual Annotation FindAnnotation([NotNull] string annotationName)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            Annotation annotation;
            return _annotations.HasValue
                   && _annotations.Value.TryGetValue(new Annotation(annotationName, "_"), out annotation)
                ? annotation
                : null;
        }

        /// <summary>
        ///     Removes the given annotation from this object.
        /// </summary>
        /// <param name="annotationName"> The annotation to remove. </param>
        /// <returns> The annotation that was removed. </returns>
        public virtual Annotation RemoveAnnotation([NotNull] string annotationName)
        {
            Check.NotNull(annotationName, nameof(annotationName));

            var previousAnnotations = _annotations.Value;
            var annotation = new Annotation(annotationName, "_");
            _annotations.Value = _annotations.Value.Remove(annotation);

            Annotation removedAnnotations = null;
            if (previousAnnotations.Count != _annotations.Value.Count)
            {
                previousAnnotations.TryGetValue(annotation, out removedAnnotations);
            }

            return removedAnnotations;
        }

        /// <summary>
        ///     Gets the value annotation with the given name, returning null if it does not exist.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation to find. </param>
        /// <returns>         
        ///     The value of the existing annotation if an annotation with the specified name already exists. Otherwise, null. 
        /// </returns>
        // ReSharper disable once AnnotationRedundancyInHierarchy
        // TODO: Fix API test to handle indexer
        public virtual object this[[NotNull] string annotationName]
        {
            get { return FindAnnotation(annotationName)?.Value; }
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

        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        public virtual IEnumerable<Annotation> GetAnnotations()
            => _annotations.HasValue
                ? (IEnumerable<Annotation>)_annotations.Value
                : ImmutableList<Annotation>.Empty;

        private class AnnotationComparer : IComparer<IAnnotation>
        {
            public int Compare(IAnnotation x, IAnnotation y) => StringComparer.Ordinal.Compare(x.Name, y.Name);
        }

        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        IEnumerable<IAnnotation> IAnnotatable.GetAnnotations() => GetAnnotations();

        /// <summary>
        ///     Gets the annotation with the given name, returning null if it does not exist.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, null. 
        /// </returns>
        IAnnotation IAnnotatable.FindAnnotation(string annotationName) => FindAnnotation(annotationName);
    }
}
