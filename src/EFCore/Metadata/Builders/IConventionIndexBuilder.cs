// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        ///     The index being configured.
        /// </summary>
        new IConventionIndex Metadata { get; }

        /// <summary>
        ///     Configures whether this index is unique (i.e. each set of values must be unique).
        /// </summary>
        /// <param name="unique"> A value indicating whether the index is unique. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the uniqueness was configured,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionIndexBuilder IsUnique(bool? unique, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether this index uniqueness can be configured
        ///     from the current configuration source
        /// </summary>
        /// <param name="unique"> A value indicating whether the index is unique. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the index uniqueness can be configured. </returns>
        bool CanSetIsUnique(bool? unique, bool fromDataAnnotation = false);
    }
}
