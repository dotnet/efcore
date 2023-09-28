// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="IndexBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerIndexBuilderExtensions
{
    /// <summary>
    ///     Configures whether the index is clustered when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="clustered">A value indicating whether the index is clustered.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder IsClustered(this IndexBuilder indexBuilder, bool clustered = true)
    {
        indexBuilder.Metadata.SetIsClustered(clustered);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures whether the index is clustered when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="clustered">A value indicating whether the index is clustered.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IsClustered<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        bool clustered = true)
        => (IndexBuilder<TEntity>)IsClustered((IndexBuilder)indexBuilder, clustered);

    /// <summary>
    ///     Configures whether the index is clustered when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="clustered">A value indicating whether the index is clustered.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="clustered">A value indicating whether the index is clustered.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the index can be configured as clustered.</returns>
    public static bool CanSetIsClustered(
        this IConventionIndexBuilder indexBuilder,
        bool? clustered,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.Clustered, clustered, fromDataAnnotation);

    /// <summary>
    ///     Configures index include properties when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="propertyNames">An array of property names to be used in 'include' clause.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder IncludeProperties(this IndexBuilder indexBuilder, params string[] propertyNames)
    {
        Check.NotNull(propertyNames, nameof(propertyNames));

        indexBuilder.Metadata.SetIncludeProperties(propertyNames);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures index include properties when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="propertyNames">An array of property names to be used in 'include' clause.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IncludeProperties<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        params string[] propertyNames)
    {
        Check.NotNull(propertyNames, nameof(propertyNames));

        indexBuilder.Metadata.SetIncludeProperties(propertyNames);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures index include properties when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
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
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IncludeProperties<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        Expression<Func<TEntity, object?>> includeExpression)
    {
        Check.NotNull(includeExpression, nameof(includeExpression));

        IncludeProperties(
            indexBuilder,
            includeExpression.GetMemberAccessList().Select(EntityFrameworkMemberInfoExtensions.GetSimpleMemberName).ToArray());

        return indexBuilder;
    }

    /// <summary>
    ///     Configures index include properties when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="propertyNames">An array of property names to be used in 'include' clause.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="propertyNames">An array of property names to be used in 'include' clause.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given include properties can be set.</returns>
    public static bool CanSetIncludeProperties(
        this IConventionIndexBuilder indexBuilder,
        IReadOnlyList<string>? propertyNames,
        bool fromDataAnnotation = false)
        => (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            .Overrides(indexBuilder.Metadata.GetIncludePropertiesConfigurationSource())
            || indexBuilder.Metadata.GetIncludeProperties() is var currentProperties
            && ((propertyNames is null && currentProperties is null)
                || (propertyNames is not null && currentProperties is not null && propertyNames.SequenceEqual(currentProperties)));

    /// <summary>
    ///     Configures whether the index is created with online option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="createdOnline">A value indicating whether the index is created with online option.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder IsCreatedOnline(this IndexBuilder indexBuilder, bool createdOnline = true)
    {
        indexBuilder.Metadata.SetIsCreatedOnline(createdOnline);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures whether the index is created with online option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="createdOnline">A value indicating whether the index is created with online option.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> IsCreatedOnline<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        bool createdOnline = true)
        => (IndexBuilder<TEntity>)IsCreatedOnline((IndexBuilder)indexBuilder, createdOnline);

    /// <summary>
    ///     Configures whether the index is created with online option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="createdOnline">A value indicating whether the index is created with online option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="createdOnline">A value indicating whether the index is created with online option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    /// <returns><see langword="true" /> if the index can be configured with online option when targeting SQL Server.</returns>
    public static bool CanSetIsCreatedOnline(
        this IConventionIndexBuilder indexBuilder,
        bool? createdOnline,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.CreatedOnline, createdOnline, fromDataAnnotation);

    /// <summary>
    ///     Configures whether the index is created with fill factor option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="fillFactor">A value indicating whether the index is created with fill factor option.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder HasFillFactor(this IndexBuilder indexBuilder, int fillFactor)
    {
        indexBuilder.Metadata.SetFillFactor(fillFactor);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures whether the index is created with fill factor option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="fillFactor">A value indicating whether the index is created with fill factor option.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> HasFillFactor<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        int fillFactor)
        => (IndexBuilder<TEntity>)HasFillFactor((IndexBuilder)indexBuilder, fillFactor);

    /// <summary>
    ///     Configures whether the index is created with fill factor option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="fillFactor">A value indicating whether the index is created with fill factor option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="fillFactor">A value indicating whether the index is created with fill factor option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the index can be configured with fill factor option when targeting SQL Server.</returns>
    public static bool CanSetFillFactor(
        this IConventionIndexBuilder indexBuilder,
        int? fillFactor,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.FillFactor, fillFactor, fromDataAnnotation);

    /// <summary>
    ///     Configures whether the index is created with sort in tempdb option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="sortInTempDb">A value indicating whether the index is created with sort in tempdb option.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder SortInTempDb(this IndexBuilder indexBuilder, bool sortInTempDb = true)
    {
        indexBuilder.Metadata.SetSortInTempDb(sortInTempDb);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures whether the index is created with sort in tempdb option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="sortInTempDb">A value indicating whether the index is created with sort in tempdb option.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> SortInTempDb<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        bool sortInTempDb = true)
        => (IndexBuilder<TEntity>)SortInTempDb((IndexBuilder)indexBuilder, sortInTempDb);

    /// <summary>
    ///     Configures whether the index is created with sort in tempdb option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="sortInTempDb">A value indicating whether the index is created with sort in tempdb option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionIndexBuilder? SortInTempDb(
        this IConventionIndexBuilder indexBuilder,
        bool? sortInTempDb,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetSortInTempDb(sortInTempDb, fromDataAnnotation))
        {
            indexBuilder.Metadata.SetSortInTempDb(sortInTempDb, fromDataAnnotation);

            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the index can be configured with sort in tempdb option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="sortInTempDb">A value indicating whether the index is created with sort in tempdb option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    /// <returns><see langword="true" /> if the index can be configured with sort in tempdb option when targeting SQL Server.</returns>
    public static bool CanSetSortInTempDb(
        this IConventionIndexBuilder indexBuilder,
        bool? sortInTempDb,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.SortInTempDb, sortInTempDb, fromDataAnnotation);

    /// <summary>
    ///     Configures whether the index is created with data compression option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="dataCompressionType">A value indicating the data compression option to be used.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder UseDataCompression(this IndexBuilder indexBuilder, DataCompressionType dataCompressionType)
    {
        indexBuilder.Metadata.SetDataCompression(dataCompressionType);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures whether the index is created with data compression option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="dataCompressionType">A value indicating the data compression option to be used.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> UseDataCompression<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        DataCompressionType dataCompressionType)
        => (IndexBuilder<TEntity>)UseDataCompression((IndexBuilder)indexBuilder, dataCompressionType);

    /// <summary>
    ///     Configures whether the index is created with data compression option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="dataCompressionType">A value indicating the data compression option to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionIndexBuilder? UseDataCompression(
        this IConventionIndexBuilder indexBuilder,
        DataCompressionType? dataCompressionType,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetDataCompression(dataCompressionType, fromDataAnnotation))
        {
            indexBuilder.Metadata.SetDataCompression(dataCompressionType, fromDataAnnotation);

            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the index can be configured with data compression option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="dataCompressionType">A value indicating the data compression option to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    /// <returns><see langword="true" /> if the index can be configured with data compression option when targeting SQL Server.</returns>
    public static bool CanSetDataCompression(
        this IConventionIndexBuilder indexBuilder,
        DataCompressionType? dataCompressionType,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(SqlServerAnnotationNames.DataCompression, dataCompressionType, fromDataAnnotation);
}
