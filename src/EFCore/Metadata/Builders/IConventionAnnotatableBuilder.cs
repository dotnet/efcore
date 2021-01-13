// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionAnnotatable" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     Gets the annotatable item being configured.
        /// </summary>
        IConventionAnnotatable Metadata { get; }

        /// <summary>
        ///     Gets the model builder.
        /// </summary>
        IConventionModelBuilder ModelBuilder { get; }

        /// <summary>
        ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
        /// </summary>
        /// <param name="name"> The name of the annotation to be set. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An <see cref="IConventionAnnotatableBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
        /// </returns>
        IConventionAnnotatableBuilder HasAnnotation([NotNull] string name, [CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
        ///     Removes the annotation if <see langword="null" /> value is specified.
        /// </summary>
        /// <param name="name"> The name of the annotation to be set. </param>
        /// <param name="value"> The value to be stored in the annotation. <see langword="null" /> to remove the annotations. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An <see cref="IConventionAnnotatableBuilder" /> to continue configuration if the annotation was set or removed,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionAnnotatableBuilder HasNonNullAnnotation(
            [NotNull] string name,
            [CanBeNull] object value,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets or removes the annotation stored under the given name.
        /// </summary>
        /// <param name="name"> The name of the annotation to be set. </param>
        /// <param name="value"> The value to be stored in the annotation. <see langword="null" /> to remove the annotations. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An <see cref="IConventionAnnotatableBuilder" /> to continue configuration if the annotation was set or removed,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        [Obsolete("Use HasNonNullAnnotation")]
        IConventionAnnotatableBuilder SetOrRemoveAnnotation(
            [NotNull] string name,
            [CanBeNull] object value,
            bool fromDataAnnotation = false)
            => HasNonNullAnnotation(name, value, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether an annotation with the given name and value can be set from this configuration source.
        /// </summary>
        /// <param name="name"> The name of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the annotation can be set, <see langword="false" /> otherwise. </returns>
        bool CanSetAnnotation([NotNull] string name, [CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes the annotation with the given name from this object.
        /// </summary>
        /// <param name="name"> The name of the annotation to remove. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An <see cref="IConventionAnnotatableBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
        /// </returns>
        IConventionAnnotatableBuilder HasNoAnnotation([NotNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes the annotation with the given name from this object.
        /// </summary>
        /// <param name="name"> The name of the annotation to remove. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An <see cref="IConventionAnnotatableBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
        /// </returns>
        [Obsolete("Use HasNoAnnotation")]
        IConventionAnnotatableBuilder RemoveAnnotation([NotNull] string name, bool fromDataAnnotation = false)
            => HasNoAnnotation(name, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether an annotation with the given name can be removed using this configuration source.
        /// </summary>
        /// <param name="name"> The name of the annotation to remove. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the annotation can be removed, <see langword="false" /> otherwise. </returns>
        bool CanRemoveAnnotation([NotNull] string name, bool fromDataAnnotation = false);
    }
}
