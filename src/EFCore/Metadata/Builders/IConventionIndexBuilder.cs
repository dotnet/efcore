// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionIndex" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionIndexBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     Gets the index being configured.
        /// </summary>
        new IConventionIndex Metadata { get; }

        /// <summary>
        ///     Configures whether this index is unique (i.e. each set of values must be unique).
        /// </summary>
        /// <param name="unique"> A value indicating whether the index is unique. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the uniqueness was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionIndexBuilder? IsUnique(bool? unique, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether this index uniqueness can be configured
        ///     from the current configuration source.
        /// </summary>
        /// <param name="unique"> A value indicating whether the index is unique. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the index uniqueness can be configured. </returns>
        bool CanSetIsUnique(bool? unique, bool fromDataAnnotation = false);
    }
}
