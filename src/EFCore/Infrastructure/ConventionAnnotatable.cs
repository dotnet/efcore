// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Base class for types that support reading and writing convention annotations.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class ConventionAnnotatable : Annotatable, IConventionAnnotatable, IMutableAnnotatable
    {
        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        public new virtual IEnumerable<ConventionAnnotation> GetAnnotations()
            => base.GetAnnotations().Cast<ConventionAnnotation>();

        /// <summary>
        ///     Adds a annotation with given key and value to this object using given configuration source.
        ///     Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <param name="configurationSource"> The configuration source of the annotation to be added. </param>
        /// <returns> The added annotation. </returns>
        public virtual ConventionAnnotation AddAnnotation(
            [NotNull] string name,
            [CanBeNull] object value,
            ConfigurationSource configurationSource)
            => (ConventionAnnotation)base.AddAnnotation(name, CreateAnnotation(name, value, configurationSource));

        /// <summary>
        ///     Sets the annotation with given key and value on this object using given configuration source.
        ///     Overwrites the existing annotation if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The key of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <param name="configurationSource"> The configuration source of the annotation to be set. </param>
        public virtual ConventionAnnotation SetAnnotation(
            [NotNull] string name,
            [CanBeNull] object value,
            ConfigurationSource configurationSource)
        {
            var oldAnnotation = FindAnnotation(name);
            if (oldAnnotation != null)
            {
                if (Equals(oldAnnotation.Value, value))
                {
                    oldAnnotation.UpdateConfigurationSource(configurationSource);
                    return oldAnnotation;
                }

                configurationSource = configurationSource.Max(oldAnnotation.GetConfigurationSource());
            }

            return (ConventionAnnotation)base.SetAnnotation(name, CreateAnnotation(name, value, configurationSource), oldAnnotation);
        }

        /// <summary>
        ///     Called when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override Annotation OnAnnotationSet(string name, Annotation annotation, Annotation oldAnnotation)
            => (Annotation)OnAnnotationSet(name, (IConventionAnnotation)annotation, (IConventionAnnotation)oldAnnotation);

        /// <summary>
        ///     Called when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected virtual IConventionAnnotation OnAnnotationSet(
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation)
            => annotation;

        /// <summary>
        ///     Gets the annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The key of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
        /// </returns>
        public new virtual ConventionAnnotation FindAnnotation([NotNull] string name)
            => (ConventionAnnotation)base.FindAnnotation(name);

        /// <summary>
        ///     Removes the given annotation from this object.
        /// </summary>
        /// <param name="name"> The annotation to remove. </param>
        /// <returns> The annotation that was removed. </returns>
        public new virtual ConventionAnnotation RemoveAnnotation([NotNull] string name)
            => (ConventionAnnotation)base.RemoveAnnotation(name);

        /// <inheritdoc />
        protected override Annotation CreateAnnotation(string name, object value)
            => CreateAnnotation(name, value, ConfigurationSource.Explicit);

        private static ConventionAnnotation CreateAnnotation(
            string name,
            object value,
            ConfigurationSource configurationSource)
            => new ConventionAnnotation(name, value, configurationSource);

        /// <inheritdoc />
        IConventionAnnotatableBuilder IConventionAnnotatable.Builder
            => throw new NotImplementedException();

        /// <inheritdoc />
        [DebuggerStepThrough]
        IEnumerable<IConventionAnnotation> IConventionAnnotatable.GetAnnotations()
            => GetAnnotations();

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionAnnotation IConventionAnnotatable.SetAnnotation(string name, object value, bool fromDataAnnotation)
            => SetAnnotation(name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        void IMutableAnnotatable.SetAnnotation(string name, object value)
            => SetAnnotation(name, value, ConfigurationSource.Explicit);

        /// <inheritdoc />
        object IMutableAnnotatable.this[string name]
        {
            [DebuggerStepThrough]
            get => this[name];
            [DebuggerStepThrough]
            set
            {
                if (value == null)
                {
                    RemoveAnnotation(name);
                }
                else
                {
                    SetAnnotation(name, value, ConfigurationSource.Explicit);
                }
            }
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionAnnotation IConventionAnnotatable.AddAnnotation(string name, object value, bool fromDataAnnotation)
            => AddAnnotation(name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionAnnotation IConventionAnnotatable.FindAnnotation(string name)
            => FindAnnotation(name);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionAnnotation IConventionAnnotatable.RemoveAnnotation(string name)
            => RemoveAnnotation(name);
    }
}
