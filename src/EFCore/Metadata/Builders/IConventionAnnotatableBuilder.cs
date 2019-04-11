// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        ///     The annotatable item being configured.
        /// </summary>
        IConventionAnnotatable Metadata { get; }

        /// <summary>
        ///     The model builder.
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
        ///     An <see cref="IConventionAnnotatableBuilder" /> to continue configuration if the annotation was set, <c>null</c> otherwise.
        /// </returns>
        IConventionAnnotatableBuilder HasAnnotation([NotNull] string name, [CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets or removes the annotation stored under the given name.
        /// </summary>
        /// <param name="name"> The name of the annotation to be set. </param>
        /// <param name="value"> The value to be stored in the annotation. <c>null</c> to remove the annotations. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An <see cref="IConventionAnnotatableBuilder" /> to continue configuration if the annotation was set or removed,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionAnnotatableBuilder SetOrRemoveAnnotation(
            [NotNull] string name, [CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether an annotation with the given name and value can be set from this configuration source.
        /// </summary>
        /// <param name="name"> The name of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the annotation can be set, <c>false</c> otherwise. </returns>
        bool CanSetAnnotation([NotNull] string name, [CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes the annotation with the given name from this object.
        /// </summary>
        /// <param name="name"> The name of the annotation to remove. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An <see cref="IConventionAnnotatableBuilder" /> to continue configuration if the annotation was set, <c>null</c> otherwise.
        /// </returns>
        IConventionAnnotatableBuilder RemoveAnnotation([NotNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether an annotation with the given name can be removed using this configuration source.
        /// </summary>
        /// <param name="name"> The name of the annotation to remove. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the annotation can be removed, <c>false</c> otherwise. </returns>
        bool CanRemoveAnnotation([NotNull] string name, bool fromDataAnnotation = false);
    }
}
