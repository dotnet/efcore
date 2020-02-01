// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
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
    public class Annotatable : IMutableAnnotatable
    {
        private SortedDictionary<string, Annotation> _annotations;

        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        public virtual IEnumerable<Annotation> GetAnnotations() =>
            _annotations?.Values ?? Enumerable.Empty<Annotation>();

        /// <summary>
        ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly added annotation. </returns>
        public virtual Annotation AddAnnotation([NotNull] string name, [CanBeNull] object value)
        {
            Check.NotEmpty(name, nameof(name));

            var annotation = CreateAnnotation(name, value);

            return AddAnnotation(name, annotation);
        }

        /// <summary>
        ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="annotation"> The annotation to be added. </param>
        /// <returns> The added annotation. </returns>
        protected virtual Annotation AddAnnotation([NotNull] string name, [NotNull] Annotation annotation)
        {
            if (FindAnnotation(name) != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(name));
            }

            SetAnnotation(name, annotation, oldAnnotation: null);

            return annotation;
        }

        /// <summary>
        ///     Sets the annotation stored under the given key. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        public virtual void SetAnnotation(string name, object value)
        {
            var oldAnnotation = FindAnnotation(name);
            if (oldAnnotation != null
                && Equals(oldAnnotation.Value, value))
            {
                return;
            }

            SetAnnotation(name, CreateAnnotation(name, value), oldAnnotation);
        }

        /// <summary>
        ///     Sets the annotation stored under the given key. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="annotation"> The annotation to be set. </param>
        /// <param name="oldAnnotation"> The annotation being replaced. </param>
        /// <returns> The annotation that was set. </returns>
        protected virtual Annotation SetAnnotation(
            [NotNull] string name,
            [NotNull] Annotation annotation,
            [CanBeNull] Annotation oldAnnotation)
        {
            if (_annotations == null)
            {
                _annotations = new SortedDictionary<string, Annotation>();
            }

            _annotations[name] = annotation;

            return OnAnnotationSet(name, annotation, oldAnnotation);
        }

        /// <summary>
        ///     Called when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected virtual Annotation OnAnnotationSet(
            [NotNull] string name,
            [CanBeNull] Annotation annotation,
            [CanBeNull] Annotation oldAnnotation)
            => annotation;

        /// <summary>
        ///     Gets the annotation with the given name, returning <c>null</c> if it does not exist.
        /// </summary>
        /// <param name="name"> The key of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <c>null</c>.
        /// </returns>
        public virtual Annotation FindAnnotation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return _annotations == null
                ? null
                : _annotations.TryGetValue(name, out var annotation)
                    ? annotation
                    : null;
        }

        /// <summary>
        ///     Removes the given annotation from this object.
        /// </summary>
        /// <param name="name"> The annotation to remove. </param>
        /// <returns> The annotation that was removed. </returns>
        public virtual Annotation RemoveAnnotation([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            var annotation = FindAnnotation(name);
            if (annotation == null)
            {
                return null;
            }

            _annotations.Remove(name);

            if (_annotations.Count == 0)
            {
                _annotations = null;
            }

            OnAnnotationSet(name, null, annotation);

            return annotation;
        }

        /// <summary>
        ///     Gets the value annotation with the given name, returning <c>null</c> if it does not exist.
        /// </summary>
        /// <param name="name"> The key of the annotation to find. </param>
        /// <returns>
        ///     The value of the existing annotation if an annotation with the specified name already exists.
        ///     Otherwise, <c>null</c>.
        /// </returns>
        public virtual object this[string name]
        {
            get => FindAnnotation(name)?.Value;
            set
            {
                Check.NotEmpty(name, nameof(name));

                if (value == null)
                {
                    RemoveAnnotation(name);
                }
                else
                {
                    SetAnnotation(name, value);
                }
            }
        }

        /// <summary>
        ///     Creates a new annotation.
        /// </summary>
        /// <param name="name"> The key of the annotation. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly created annotation. </returns>
        protected virtual Annotation CreateAnnotation([NotNull] string name, [CanBeNull] object value)
            => new Annotation(name, value);

        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        IEnumerable<IAnnotation> IAnnotatable.GetAnnotations() => GetAnnotations();

        /// <inheritdoc />
        IAnnotation IAnnotatable.FindAnnotation(string name) => FindAnnotation(name);

        /// <inheritdoc />
        IAnnotation IMutableAnnotatable.AddAnnotation(string name, object value) => AddAnnotation(name, value);

        /// <inheritdoc />
        IAnnotation IMutableAnnotatable.RemoveAnnotation(string name) => RemoveAnnotation(name);
    }
}
