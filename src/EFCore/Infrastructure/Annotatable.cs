// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public virtual IEnumerable<Annotation> GetAnnotations()
            => _annotations?.Values ?? Enumerable.Empty<Annotation>();

        /// <inheritdoc cref="IMutableAnnotatable.AddAnnotation" />
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
                throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(name, ToString()));
            }

            SetAnnotation(name, annotation, oldAnnotation: null);

            return annotation;
        }

        /// <inheritdoc />
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

        /// <inheritdoc cref="IAnnotatable.FindAnnotation" />
        public virtual Annotation FindAnnotation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return _annotations == null
                ? null
                : _annotations.TryGetValue(name, out var annotation)
                    ? annotation
                    : null;
        }

        /// <inheritdoc cref="IMutableAnnotatable.RemoveAnnotation" />
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

        /// <inheritdoc cref="IMutableAnnotatable.this[string]" />
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

        /// <inheritdoc />
        [DebuggerStepThrough]
        IEnumerable<IAnnotation> IAnnotatable.GetAnnotations()
            => GetAnnotations();

        /// <inheritdoc />
        [DebuggerStepThrough]
        IAnnotation IAnnotatable.FindAnnotation(string name)
            => FindAnnotation(name);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IAnnotation IMutableAnnotatable.AddAnnotation(string name, object value)
            => AddAnnotation(name, value);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IAnnotation IMutableAnnotatable.RemoveAnnotation(string name)
            => RemoveAnnotation(name);
    }
}
