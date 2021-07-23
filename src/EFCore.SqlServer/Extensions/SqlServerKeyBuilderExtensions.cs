// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        public static KeyBuilder IsClustered(this KeyBuilder keyBuilder, bool clustered = true)
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
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static KeyBuilder<TEntity> IsClustered<TEntity>(
            this KeyBuilder<TEntity> keyBuilder,
            bool clustered = true)
            => (KeyBuilder<TEntity>)IsClustered((KeyBuilder)keyBuilder, clustered);

        /// <summary>
        ///     Configures whether the key is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="clustered"> A value indicating whether the key is clustered. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionKeyBuilder? IsClustered(
            this IConventionKeyBuilder keyBuilder,
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
        /// <returns> <see langword="true" /> if the key can be configured as clustered. </returns>
        public static bool CanSetIsClustered(
            this IConventionKeyBuilder keyBuilder,
            bool? clustered,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            return keyBuilder.CanSetAnnotation(SqlServerAnnotationNames.Clustered, clustered, fromDataAnnotation);
        }
    }
}
