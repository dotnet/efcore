// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        public static IndexBuilder IsClustered(this IndexBuilder indexBuilder, bool clustered = true)
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
            this IndexBuilder<TEntity> indexBuilder,
            bool clustered = true)
            => (IndexBuilder<TEntity>)IsClustered((IndexBuilder)indexBuilder, clustered);

        /// <summary>
        ///     Configures whether the index is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionIndexBuilder? IsClustered(
            this IConventionIndexBuilder indexBuilder,
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
        /// <returns> <see langword="true" /> if the index can be configured as clustered. </returns>
        public static bool CanSetIsClustered(
            this IConventionIndexBuilder indexBuilder,
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
        public static IndexBuilder IncludeProperties(this IndexBuilder indexBuilder, params string[] propertyNames)
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
        /// <param name="propertyNames"> An array of property names to be used in 'include' clause. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder<TEntity> IncludeProperties<TEntity>(
            this IndexBuilder<TEntity> indexBuilder,
            params string[] propertyNames)
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
            this IndexBuilder<TEntity> indexBuilder,
            Expression<Func<TEntity, object?>> includeExpression)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(includeExpression, nameof(includeExpression));

            IncludeProperties(
                indexBuilder,
                includeExpression.GetMemberAccessList().Select(EntityFrameworkMemberInfoExtensions.GetSimpleMemberName).ToArray());

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
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionIndexBuilder? IncludeProperties(
            this IConventionIndexBuilder indexBuilder,
            IReadOnlyList<string>? propertyNames,
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
        /// <returns> <see langword="true" /> if the given include properties can be set. </returns>
        public static bool CanSetIncludeProperties(
            this IConventionIndexBuilder indexBuilder,
            IReadOnlyList<string>? propertyNames,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                .Overrides(indexBuilder.Metadata.GetIncludePropertiesConfigurationSource())
                || indexBuilder.Metadata.GetIncludeProperties() is var currentProperties
                && ((propertyNames is null && currentProperties is null)
                    || (propertyNames is not null && currentProperties is not null && propertyNames.SequenceEqual(currentProperties)));
        }

        /// <summary>
        ///     Configures whether the index is created with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder IsCreatedOnline(this IndexBuilder indexBuilder, bool createdOnline = true)
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
            this IndexBuilder<TEntity> indexBuilder,
            bool createdOnline = true)
            => (IndexBuilder<TEntity>)IsCreatedOnline((IndexBuilder)indexBuilder, createdOnline);

        /// <summary>
        ///     Configures whether the index is created with online option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="createdOnline"> A value indicating whether the index is created with online option. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionIndexBuilder? IsCreatedOnline(
            this IConventionIndexBuilder indexBuilder,
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
        ///     <see langword="null" /> otherwise.
        /// </returns>
        /// <returns> <see langword="true" /> if the index can be configured with online option when targeting SQL Server. </returns>
        public static bool CanSetIsCreatedOnline(
            this IConventionIndexBuilder indexBuilder,
            bool? createdOnline,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.CreatedOnline, createdOnline, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures whether the index is created with fill factor option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="fillFactor"> A value indicating whether the index is created with fill factor option. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder HasFillFactor(this IndexBuilder indexBuilder, int fillFactor)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SetFillFactor(fillFactor);

            return indexBuilder;
        }

        /// <summary>
        ///     Configures whether the index is created with fill factor option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="fillFactor"> A value indicating whether the index is created with fill factor option. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder<TEntity> HasFillFactor<TEntity>(
            this IndexBuilder<TEntity> indexBuilder,
            int fillFactor)
            => (IndexBuilder<TEntity>)HasFillFactor((IndexBuilder)indexBuilder, fillFactor);

        /// <summary>
        ///     Configures whether the index is created with fill factor option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="fillFactor"> A value indicating whether the index is created with fill factor option. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionIndexBuilder? HasFillFactor(
            this IConventionIndexBuilder indexBuilder,
            int? fillFactor,
            bool fromDataAnnotation = false)
        {
            if (indexBuilder.CanSetFillFactor(fillFactor, fromDataAnnotation))
            {
                indexBuilder.Metadata.SetFillFactor(fillFactor, fromDataAnnotation);

                return indexBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the index can be configured with fill factor option when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="fillFactor"> A value indicating whether the index is created with fill factor option. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the index can be configured with fill factor option when targeting SQL Server. </returns>
        public static bool CanSetFillFactor(
            this IConventionIndexBuilder indexBuilder,
            int? fillFactor,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.FillFactor, fillFactor, fromDataAnnotation);
        }
    }
}
