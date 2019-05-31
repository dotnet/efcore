// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="KeyBuilder" />.
    /// </summary>
    public static class RelationalKeyBuilderExtensions
    {
        /// <summary>
        ///     Configures the name of the key constraint in the database when targeting a relational database.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="name"> The name of the key. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static KeyBuilder HasName([NotNull] this KeyBuilder keyBuilder, [CanBeNull] string name)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            keyBuilder.Metadata.SetName(name);

            return keyBuilder;
        }

        /// <summary>
        ///     Configures the name of the key constraint in the database when targeting a relational database.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="name"> The name of the key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionKeyBuilder HasName(
            [NotNull] this IConventionKeyBuilder keyBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            if (keyBuilder.CanSetName(name, fromDataAnnotation))
            {
                keyBuilder.Metadata.SetName(name, fromDataAnnotation);
                return keyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given name can be set for the key constraint.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given name can be set for the key constraint. </returns>
        public static bool CanSetName(
            [NotNull] this IConventionKeyBuilder keyBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
            => keyBuilder.CanSetAnnotation(RelationalAnnotationNames.Name, name, fromDataAnnotation);
    }
}
