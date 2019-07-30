// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="IndexBuilder" />.
    /// </summary>
    public static class SqlServerIndexBuilderExtensions
    {
        /// <summary>
        ///     Configures whether the index is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder IsClustered([NotNull] this IndexBuilder indexBuilder, bool clustered = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SetIsClustered(clustered);

            return indexBuilder;
        }

        /// <summary>
        ///     Configures whether the index is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder<TEntity> IsClustered<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder, bool clustered = true)
            => (IndexBuilder<TEntity>)IsClustered((IndexBuilder)indexBuilder, clustered);

        /// <summary>
        ///     Configures whether the index is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionIndexBuilder IsClustered(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            bool? clustered,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetIsClustered(clustered, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetIsClustered(clustered, fromDataAnnotation);
                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the index can be configured as clustered.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the index can be configured as clustered. </returns>
        public static bool CanSetIsClustered(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            bool? clustered,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.Clustered, clustered, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures index include properties when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="propertyNames"> An array of property names to be used in 'include' clause. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder IncludeProperties([NotNull] this IndexBuilder indexBuilder, [NotNull] params string[] propertyNames)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(propertyNames, nameof(propertyNames));

            indexBuilder.Metadata.SetIncludeProperties(propertyNames);

            return indexBuilder;
        }

        /// <summary>
        ///     Configures index include properties when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="includeExpression">
        ///     <para>
        ///         A lambda expression representing the property(s) to be included in the 'include' clause
        ///         (<c>blog => blog.Url</c>).
        ///     </para>
        ///     <para>
        ///         If multiple properties are to be included then specify an anonymous type including the
        ///         properties (<c>post => new { post.Title, post.BlogId }</c>).
        ///     </para>
        /// </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder<TEntity> IncludeProperties<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder, [NotNull] Expression<Func<TEntity, object>> includeExpression)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(includeExpression, nameof(includeExpression));

            IncludeProperties(
                indexBuilder,
                includeExpression.GetPropertyAccessList().Select(MemberInfoExtensions.GetSimpleMemberName).ToArray());

            return indexBuilder;
        }

        /// <summary>
        ///     Configures index include properties when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="propertyNames"> An array of property names to be used in 'include' clause. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionIndexBuilder IncludeProperties(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [NotNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetIncludeProperties(propertyNames, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetIncludeProperties(propertyNames, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given include properties can be set.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="propertyNames"> An array of property names to be used in 'include' clause. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given include properties can be set. </returns>
        public static bool CanSetIncludeProperties(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [CanBeNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                   .Overrides(indexBuilder.Metadata.GetIncludePropertiesConfigurationSource())
                   || StructuralComparisons.StructuralEqualityComparer.Equals(
                       propertyNames, indexBuilder.Metadata.GetIncludeProperties());
        }

        /// <summary>
        ///     Configures whether the index is created with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder IsCreatedOnline([NotNull] this IndexBuilder indexBuilder, bool createdOnline = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SetIsCreatedOnline(createdOnline);

            return indexBuilder;
        }

        /// <summary>
        ///     Configures whether the index is created with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder<TEntity> IsCreatedOnline<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder, bool createdOnline = true)
            => (IndexBuilder<TEntity>)IsCreatedOnline((IndexBuilder)indexBuilder, createdOnline);

        /// <summary>
        ///     Configures whether the index is created with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionIndexBuilder IsCreatedOnline(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            bool? createdOnline,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetIsCreatedOnline(createdOnline, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetIsCreatedOnline(createdOnline, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the index can be configured with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        /// <returns> <c>true</c> if the index can be configured with online option when targeting SQL Server. </returns>
        public static bool CanSetIsCreatedOnline(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            bool? createdOnline,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.CreatedOnline, createdOnline, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures whether the index is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <returns> A builder to further configure the index. </returns>
        [Obsolete("Use IsClustered")]
        public static IndexBuilder ForSqlServerIsClustered([NotNull] this IndexBuilder indexBuilder, bool clustered = true)
            => indexBuilder.IsClustered(clustered);

        /// <summary>
        ///     Configures whether the index is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <returns> A builder to further configure the index. </returns>
        [Obsolete("Use IsClustered")]
        public static IndexBuilder<TEntity> ForSqlServerIsClustered<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder, bool clustered = true)
            => indexBuilder.IsClustered(clustered);

        /// <summary>
        ///     Configures whether the index is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use IsClustered")]
        public static IConventionIndexBuilder ForSqlServerIsClustered(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            bool? clustered,
            bool fromDataAnnotation = false)
            => indexBuilder.IsClustered(clustered, fromDataAnnotation);

        /// <summary>
        ///     Configures index include properties when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="propertyNames"> An array of property names to be used in 'include' clause. </param>
        /// <returns> A builder to further configure the index. </returns>
        [Obsolete("Use IncludeProperties")]
        public static IndexBuilder ForSqlServerInclude([NotNull] this IndexBuilder indexBuilder, [NotNull] params string[] propertyNames)
            => indexBuilder.IncludeProperties(propertyNames);

        /// <summary>
        ///     Configures index include properties when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="includeExpression">
        ///     <para>
        ///         A lambda expression representing the property(s) to be included in the 'include' clause
        ///         (<c>blog => blog.Url</c>).
        ///     </para>
        ///     <para>
        ///         If multiple properties are to be included then specify an anonymous type including the
        ///         properties (<c>post => new { post.Title, post.BlogId }</c>).
        ///     </para>
        /// </param>
        /// <returns> A builder to further configure the index. </returns>
        [Obsolete("Use IncludeProperties")]
        public static IndexBuilder<TEntity> ForSqlServerInclude<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder, [NotNull] Expression<Func<TEntity, object>> includeExpression)
            => indexBuilder.IncludeProperties(includeExpression);

        /// <summary>
        ///     Configures index include properties when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="propertyNames"> An array of property names to be used in 'include' clause. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use IncludeProperties")]
        public static IConventionIndexBuilder ForSqlServerInclude(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            [NotNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false)
            => indexBuilder.IncludeProperties(propertyNames, fromDataAnnotation);

        /// <summary>
        ///     Configures whether the index is created with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <returns> A builder to further configure the index. </returns>
        [Obsolete("Use IsCreatedOnline")]
        public static IndexBuilder ForSqlServerIsCreatedOnline([NotNull] this IndexBuilder indexBuilder, bool createdOnline = true)
            => indexBuilder.IsCreatedOnline(createdOnline);

        /// <summary>
        ///     Configures whether the index is created with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <returns> A builder to further configure the index. </returns>
        [Obsolete("Use IsCreatedOnline")]
        public static IndexBuilder<TEntity> ForSqlServerIsCreatedOnline<TEntity>(
            [NotNull] this IndexBuilder<TEntity> indexBuilder, bool createdOnline = true)
            => indexBuilder.IsCreatedOnline(createdOnline);

        /// <summary>
        ///     Configures whether the index is created with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        [Obsolete("Use IsCreatedOnline")]
        public static IConventionIndexBuilder ForSqlServerIsCreatedOnline(
            [NotNull] this IConventionIndexBuilder indexBuilder,
            bool? createdOnline,
            bool fromDataAnnotation = false)
            => indexBuilder.IsCreatedOnline(createdOnline, fromDataAnnotation);
    }
}
