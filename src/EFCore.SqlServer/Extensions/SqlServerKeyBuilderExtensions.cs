// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="KeyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerKeyBuilderExtensions
{
    /// <summary>
    ///     Configures whether the key is clustered when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="clustered">A value indicating whether the key is clustered.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static KeyBuilder IsClustered(this KeyBuilder keyBuilder, bool clustered = true)
    {
        keyBuilder.Metadata.SetIsClustered(clustered);

        return keyBuilder;
    }

    /// <summary>
    ///     Configures whether the key is clustered when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="clustered">A value indicating whether the key is clustered.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static KeyBuilder<TEntity> IsClustered<TEntity>(
        this KeyBuilder<TEntity> keyBuilder,
        bool clustered = true)
        => (KeyBuilder<TEntity>)IsClustered((KeyBuilder)keyBuilder, clustered);

    /// <summary>
    ///     Configures whether the key is clustered when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="clustered">A value indicating whether the key is clustered.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="clustered">A value indicating whether the key is clustered.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the key can be configured as clustered.</returns>
    public static bool CanSetIsClustered(
        this IConventionKeyBuilder keyBuilder,
        bool? clustered,
        bool fromDataAnnotation = false)
        => keyBuilder.CanSetAnnotation(SqlServerAnnotationNames.Clustered, clustered, fromDataAnnotation);

    /// <summary>
    ///     Configures whether the key is created with fill factor option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="fillFactor">A value indicating whether the key is created with fill factor option.</param>
    /// <returns>A builder to further configure the key.</returns>
    public static KeyBuilder HasFillFactor(this KeyBuilder keyBuilder, int fillFactor)
    {
        keyBuilder.Metadata.SetFillFactor(fillFactor);

        return keyBuilder;
    }

    /// <summary>
    ///     Configures whether the key is created with fill factor option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="fillFactor">A value indicating whether the key is created with fill factor option.</param>
    /// <returns>A builder to further configure the key.</returns>
    public static KeyBuilder<TEntity> HasFillFactor<TEntity>(
        this KeyBuilder<TEntity> keyBuilder,
        int fillFactor)
        => (KeyBuilder<TEntity>)HasFillFactor((KeyBuilder)keyBuilder, fillFactor);

    /// <summary>
    ///     Configures whether the key is created with fill factor option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="fillFactor">A value indicating whether the key is created with fill factor option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionKeyBuilder? HasFillFactor(
        this IConventionKeyBuilder keyBuilder,
        int? fillFactor,
        bool fromDataAnnotation = false)
    {
        if (keyBuilder.CanSetFillFactor(fillFactor, fromDataAnnotation))
        {
            keyBuilder.Metadata.SetFillFactor(fillFactor, fromDataAnnotation);

            return keyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the key can be configured with fill factor option when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="fillFactor">A value indicating whether the key is created with fill factor option.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the key can be configured with fill factor option when targeting SQL Server.</returns>
    public static bool CanSetFillFactor(
        this IConventionKeyBuilder keyBuilder,
        int? fillFactor,
        bool fromDataAnnotation = false)
        => keyBuilder.CanSetAnnotation(SqlServerAnnotationNames.FillFactor, fillFactor, fromDataAnnotation);
}
