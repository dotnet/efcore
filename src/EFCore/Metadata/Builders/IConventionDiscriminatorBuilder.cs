// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API surface for setting discriminator values from conventions.
    /// </summary>
    public interface IConventionDiscriminatorBuilder
    {
        /// <summary>
        ///     Configures if the discriminator mapping is complete.
        /// </summary>
        /// <param name="complete"> The value indicating if this discriminator mapping is complete. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        IConventionDiscriminatorBuilder IsComplete(bool complete, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the discriminator mapping is complete can be set from this configuration source.
        /// </summary>
        /// <param name="complete"> The value indicating if this discriminator mapping is complete. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the discriminator value can be set from this configuration source. </returns>
        bool CanSetIsComplete(bool complete, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the discriminator value to use.
        /// </summary>
        /// <param name="value"> The discriminator value. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        IConventionDiscriminatorBuilder HasValue([CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given type.
        /// </summary>
        /// <param name="entityType"> The entity type for which a discriminator value is being set. </param>
        /// <param name="value"> The discriminator value. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        IConventionDiscriminatorBuilder HasValue(
            [NotNull] IConventionEntityType entityType,
            [CanBeNull] object value,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the discriminator value can be set from this configuration source.
        /// </summary>
        /// <param name="value"> The discriminator value. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the discriminator value can be set from this configuration source. </returns>
        bool CanSetValue([CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the discriminator value can be set from this configuration source.
        /// </summary>
        /// <param name="entityType"> The entity type for which a discriminator value is being set. </param>
        /// <param name="value"> The discriminator value. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the discriminator value can be set from this configuration source. </returns>
        bool CanSetValue([NotNull] IConventionEntityType entityType, [CanBeNull] object value, bool fromDataAnnotation = false);
    }
}
