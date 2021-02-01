// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

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
    public class Annotatable : IAnnotatable, IMutableAnnotatable
    {
        private SortedDictionary<string, Annotation>? _annotations;
        private ConcurrentDictionary<string, Annotation>? _runtimeAnnotations;

        /// <summary>
        ///     <para>Indicates whether the current object is read-only.</para>
        ///     <para>
        ///         Annotations cannot be changed when the object is read-only.
        ///         Runtime annotations cannot be changed when the object is not read-only.
        ///     </para>
        /// </summary>
        public virtual bool IsReadOnly => false;

        /// <summary>
        ///     Throws if the model is not read-only.
        /// </summary>
        protected virtual void EnsureReadOnly()
        {
            if (!IsReadOnly)
            {
                throw new InvalidOperationException(CoreStrings.ModelMutable);
            }
        }

        /// <summary>
        ///     Throws if the model is read-only.
        /// </summary>
        protected virtual void EnsureMutable()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(CoreStrings.ModelReadOnly);
            }
        }

        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        public virtual IEnumerable<Annotation> GetAnnotations()
            => _annotations?.Values ?? Enumerable.Empty<Annotation>();

        /// <summary>
        ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly added annotation. </returns>
        public virtual Annotation AddAnnotation([NotNull] string name, [CanBeNull] object? value)
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

        /// <summary>
        ///     Sets the annotation stored under the given key. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        public virtual void SetAnnotation(string name, object? value)
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
        protected virtual Annotation? SetAnnotation(
            [NotNull] string name,
            [NotNull] Annotation annotation,
            [CanBeNull] Annotation? oldAnnotation)
        {
            EnsureMutable();

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
        protected virtual Annotation? OnAnnotationSet(
            [NotNull] string name,
            [CanBeNull] Annotation? annotation,
            [CanBeNull] Annotation? oldAnnotation)
            => annotation;

        /// <summary>
        ///     Gets the annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The key of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
        /// </returns>
        public virtual Annotation? FindAnnotation([NotNull] string name)
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
        public virtual Annotation? RemoveAnnotation([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));
            EnsureMutable();

            var annotation = FindAnnotation(name);
            if (annotation == null)
            {
                return null;
            }

            _annotations!.Remove(name);

            if (_annotations.Count == 0)
            {
                _annotations = null;
            }

            OnAnnotationSet(name, null, annotation);

            return annotation;
        }

        /// <summary>
        ///     Gets the value annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The key of the annotation to find. </param>
        /// <returns>
        ///     The value of the existing annotation if an annotation with the specified name already exists.
        ///     Otherwise, <see langword="null" />.
        /// </returns>
        public virtual object? this[string name]
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
        protected virtual Annotation CreateAnnotation([NotNull] string name, [CanBeNull] object? value)
            => new(name, value);

        /// <summary>
        ///     Gets all runtime annotations on the current object.
        /// </summary>
        public virtual IEnumerable<Annotation> GetRuntimeAnnotations()
            => _runtimeAnnotations == null
            ? Enumerable.Empty<Annotation>()
            : _runtimeAnnotations.OrderBy(p => p.Key).Select(p => p.Value);

        /// <summary>
        ///     Adds a runtime annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly added annotation. </returns>
        public virtual Annotation AddRuntimeAnnotation([NotNull] string name, [CanBeNull] object? value)
        {
            Check.NotEmpty(name, nameof(name));

            var annotation = CreateRuntimeAnnotation(name, value);

            return AddRuntimeAnnotation(name, annotation);
        }

        /// <summary>
        ///     Adds a runtime annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="annotation"> The annotation to be added. </param>
        /// <returns> The added annotation. </returns>
        protected virtual Annotation AddRuntimeAnnotation([NotNull] string name, [NotNull] Annotation annotation)
            => GetOrCreateRuntimeAnnotations().TryAdd(name, annotation)
                ? annotation
                : throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(name, ToString()));

        /// <summary>
        ///     Sets the runtime annotation stored under the given key. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        public virtual Annotation SetRuntimeAnnotation([NotNull] string name, [CanBeNull] object? value)
            => GetOrCreateRuntimeAnnotations().AddOrUpdate(name,
                static (n, a) => a.Annotatable.CreateRuntimeAnnotation(n, a.Value),
                static (n, oldAnnotation, a) =>
                    !Equals(oldAnnotation.Value, a.Value) ? a.Annotatable.CreateRuntimeAnnotation(n, a.Value) : oldAnnotation,
                (Value: value, Annotatable: this));

        /// <summary>
        ///     Sets the runtime annotation stored under the given key. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="annotation"> The annotation to be set. </param>
        /// <param name="oldAnnotation"> The annotation being replaced. </param>
        /// <returns> The annotation that was set. </returns>
        protected virtual Annotation SetRuntimeAnnotation(
            [NotNull] string name,
            [NotNull] Annotation annotation,
            [CanBeNull] Annotation? oldAnnotation)
        {
            GetOrCreateRuntimeAnnotations()[name] = annotation;

            return annotation;
        }

        /// <summary>
        ///     Gets the value of the runtime annotation with the given name, adding it if one does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation. </param>
        /// <param name="valueFactory"> The factory used to create the value if the annotation doesn't exist. </param>
        /// <param name="factoryArgument"> An argument for the factory method. </param>
        /// <returns>
        ///     The value of the existing runtime annotation if an annotation with the specified name already exists.
        ///     Otherwise a newly created value.
        /// </returns>
        public virtual TValue GetOrAddRuntimeAnnotationValue<TValue, TArg>(
            string name,
            Func<TArg?, TValue> valueFactory,
            TArg? factoryArgument)
            => (TValue)GetOrCreateRuntimeAnnotations().GetOrAdd(
                name,
                static (n, t) => t.Annotatable.CreateRuntimeAnnotation(n, t.CreateValue(t.Argument)),
                (CreateValue: valueFactory, Argument: factoryArgument, Annotatable: this)).Value;

        /// <summary>
        ///     Gets the runtime annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The key of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
        /// </returns>
        public virtual Annotation? FindRuntimeAnnotation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return _runtimeAnnotations == null
                ? null
                : _runtimeAnnotations.TryGetValue(name, out var annotation)
                    ? annotation
                    : null;
        }

        /// <summary>
        ///     Removes the given runtime annotation from this object.
        /// </summary>
        /// <param name="name"> The annotation to remove. </param>
        /// <returns> The annotation that was removed. </returns>
        public virtual Annotation? RemoveRuntimeAnnotation([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));
            EnsureReadOnly();

            if (_runtimeAnnotations == null)
            {
                return null;
            }

            _runtimeAnnotations.Remove(name, out var annotation);

            return annotation;
        }

        /// <summary>
        ///     Creates a new runtime annotation.
        /// </summary>
        /// <param name="name"> The key of the annotation. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly created annotation. </returns>
        protected virtual Annotation CreateRuntimeAnnotation([NotNull] string name, [CanBeNull] object? value)
            => new Annotation(name, value);

        private ConcurrentDictionary<string, Annotation> GetOrCreateRuntimeAnnotations()
        {
            EnsureReadOnly();

            return NonCapturingLazyInitializer.EnsureInitialized(
                        ref _runtimeAnnotations, (object?)null, static _ => new ConcurrentDictionary<string, Annotation>());
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IEnumerable<IAnnotation> IReadOnlyAnnotatable.GetAnnotations()
            => GetAnnotations();

        /// <inheritdoc />
        [DebuggerStepThrough]
        IAnnotation? IReadOnlyAnnotatable.FindAnnotation(string name)
            => FindAnnotation(name);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IAnnotation IMutableAnnotatable.AddAnnotation(string name, object? value)
            => AddAnnotation(name, value);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IAnnotation? IMutableAnnotatable.RemoveAnnotation(string name)
            => RemoveAnnotation(name);

        /// <inheritdoc />
        IEnumerable<IAnnotation> IAnnotatable.GetRuntimeAnnotations()
            => GetRuntimeAnnotations();

        /// <inheritdoc />
        IAnnotation? IAnnotatable.FindRuntimeAnnotation(string name)
            => FindRuntimeAnnotation(name);

        /// <inheritdoc />
        IAnnotation IAnnotatable.AddRuntimeAnnotation(string name, object? value)
            => AddRuntimeAnnotation(name, value);

        /// <inheritdoc />
        IAnnotation? IAnnotatable.RemoveRuntimeAnnotation(string name)
            => RemoveRuntimeAnnotation(name);

        IAnnotation IAnnotatable.SetRuntimeAnnotation(string name, object? value)
            => SetRuntimeAnnotation(name, value);
    }
}
