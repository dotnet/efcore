// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="KeyBuilder" />.
    /// </summary>
    public static class SqlServerKeyBuilderExtensions
    {
        /// <summary>
        ///     Configures whether the key is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="clustered"> A value indicating whether the key is clustered. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static KeyBuilder IsClustered([NotNull] this KeyBuilder keyBuilder, bool clustered = true)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            keyBuilder.Metadata.SetIsClustered(clustered);

            return keyBuilder;
        }

        /// <summary>
        ///     Configures whether the key is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="clustered"> A value indicating whether the key is clustered. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionKeyBuilder IsClustered(
            [NotNull] this IConventionKeyBuilder keyBuilder,
            bool? clustered,
            bool fromDataAnnotation = false)
        {
            if (keyBuilder.CanSetIsClustered(clustered, fromDataAnnotation))
            {
                keyBuilder.Metadata.SetIsClustered(clustered, fromDataAnnotation);
                return keyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the key can be configured as clustered.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="clustered"> A value indicating whether the key is clustered. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the key can be configured as clustered. </returns>
        public static bool CanSetIsClustered(
            [NotNull] this IConventionKeyBuilder keyBuilder,
            bool? clustered,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            return keyBuilder.CanSetAnnotation(SqlServerAnnotationNames.Clustered, clustered, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures whether the key is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="clustered"> A value indicating whether the key is clustered. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        [Obsolete("Use IsClustered")]
        public static KeyBuilder ForSqlServerIsClustered([NotNull] this KeyBuilder keyBuilder, bool clustered = true)
            => keyBuilder.IsClustered(clustered);

        /// <summary>
        ///     Configures whether the key is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="clustered"> A value indicating whether the key is clustered. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use IsClustered")]
        public static IConventionKeyBuilder ForSqlServerIsClustered(
            [NotNull] this IConventionKeyBuilder keyBuilder,
            bool? clustered,
            bool fromDataAnnotation = false)
            => keyBuilder.IsClustered(clustered, fromDataAnnotation);
    }
}
