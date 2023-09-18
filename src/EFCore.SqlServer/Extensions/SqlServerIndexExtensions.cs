// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Index extension methods for SQL Server-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerIndexExtensions
{
    /// <summary>
    ///     Returns a value indicating whether the index is clustered.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><see langword="true" /> if the index is clustered.</returns>
    public static bool? IsClustered(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (bool?)index[SqlServerAnnotationNames.Clustered];

    /// <summary>
    ///     Returns a value indicating whether the index is clustered.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns><see langword="true" /> if the index is clustered.</returns>
    public static bool? IsClustered(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.Clustered);
        if (annotation != null)
        {
            return (bool?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.IsClustered(storeObject);
    }

    /// <summary>
    ///     Sets a value indicating whether the index is clustered.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index.</param>
    public static void SetIsClustered(this IMutableIndex index, bool? value)
        => index.SetAnnotation(
            SqlServerAnnotationNames.Clustered,
            value);

    /// <summary>
    ///     Sets a value indicating whether the index is clustered.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="index">The index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsClustered(
        this IConventionIndex index,
        bool? value,
        bool fromDataAnnotation = false)
        => (bool?)index.SetAnnotation(
            SqlServerAnnotationNames.Clustered,
            value,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the index is clustered.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for whether the index is clustered.</returns>
    public static ConfigurationSource? GetIsClusteredConfigurationSource(this IConventionIndex property)
        => property.FindAnnotation(SqlServerAnnotationNames.Clustered)?.GetConfigurationSource();

    /// <summary>
    ///     Returns included property names, or <see langword="null" /> if they have not been specified.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The included property names, or <see langword="null" /> if they have not been specified.</returns>
    public static IReadOnlyList<string>? GetIncludeProperties(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string[]?)index[SqlServerAnnotationNames.Include];

    /// <summary>
    ///     Returns included property names, or <see langword="null" /> if they have not been specified.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The included property names, or <see langword="null" /> if they have not been specified.</returns>
    public static IReadOnlyList<string>? GetIncludeProperties(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.Include);
        if (annotation != null)
        {
            return (IReadOnlyList<string>?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetIncludeProperties(storeObject);
    }

    /// <summary>
    ///     Sets included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="properties">The value to set.</param>
    public static void SetIncludeProperties(this IMutableIndex index, IReadOnlyList<string> properties)
        => index.SetAnnotation(
            SqlServerAnnotationNames.Include,
            properties);

    /// <summary>
    ///     Sets included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <param name="properties">The value to set.</param>
    /// <returns>The configured property names.</returns>
    public static IReadOnlyList<string>? SetIncludeProperties(
        this IConventionIndex index,
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation = false)
        => (IReadOnlyList<string>?)index.SetAnnotation(
            SqlServerAnnotationNames.Include,
            properties,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the included property names.</returns>
    public static ConfigurationSource? GetIncludePropertiesConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.Include)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value indicating whether the index is online.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><see langword="true" /> if the index is online.</returns>
    public static bool? IsCreatedOnline(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (bool?)index[SqlServerAnnotationNames.CreatedOnline];

    /// <summary>
    ///     Returns a value indicating whether the index is online.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns><see langword="true" /> if the index is online.</returns>
    public static bool? IsCreatedOnline(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.CreatedOnline);
        if (annotation != null)
        {
            return (bool?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.IsCreatedOnline(storeObject);
    }

    /// <summary>
    ///     Sets a value indicating whether the index is online.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="createdOnline">The value to set.</param>
    public static void SetIsCreatedOnline(this IMutableIndex index, bool? createdOnline)
        => index.SetAnnotation(
            SqlServerAnnotationNames.CreatedOnline,
            createdOnline);

    /// <summary>
    ///     Sets a value indicating whether the index is online.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="createdOnline">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsCreatedOnline(
        this IConventionIndex index,
        bool? createdOnline,
        bool fromDataAnnotation = false)
        => (bool?)index.SetAnnotation(
            SqlServerAnnotationNames.CreatedOnline,
            createdOnline,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the index is online.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for whether the index is online.</returns>
    public static ConfigurationSource? GetIsCreatedOnlineConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.CreatedOnline)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the fill factor that the index uses.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The fill factor that the index uses</returns>
    public static int? GetFillFactor(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (int?)index[SqlServerAnnotationNames.FillFactor];

    /// <summary>
    ///     Returns the fill factor that the index uses.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The fill factor that the index uses</returns>
    public static int? GetFillFactor(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.FillFactor);
        if (annotation != null)
        {
            return (int?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetFillFactor(storeObject);
    }

    /// <summary>
    ///     Sets a value indicating whether the index uses the fill factor.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="fillFactor">The value to set.</param>
    public static void SetFillFactor(this IMutableIndex index, int? fillFactor)
    {
        if (fillFactor is <= 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(fillFactor));
        }

        index.SetAnnotation(
            SqlServerAnnotationNames.FillFactor,
            fillFactor);
    }

    /// <summary>
    ///     Defines a value indicating whether the index uses the fill factor.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="fillFactor">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static int? SetFillFactor(
        this IConventionIndex index,
        int? fillFactor,
        bool fromDataAnnotation = false)
    {
        if (fillFactor is <= 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(fillFactor));
        }

        return (int?)index.SetAnnotation(
            SqlServerAnnotationNames.FillFactor,
            fillFactor,
            fromDataAnnotation)?.Value;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the index uses the fill factor.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for whether the index uses the fill factor.</returns>
    public static ConfigurationSource? GetFillFactorConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.FillFactor)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value indicating whether the index is sorted in tempdb.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><see langword="true" /> if the index is sorted in tempdb.</returns>
    public static bool? GetSortInTempDb(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (bool?)index[SqlServerAnnotationNames.SortInTempDb];

    /// <summary>
    ///     Returns a value indicating whether the index is sorted in tempdb.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns><see langword="true" /> if the index is sorted in tempdb.</returns>
    public static bool? GetSortInTempDb(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.SortInTempDb);
        if (annotation != null)
        {
            return (bool?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetSortInTempDb(storeObject);
    }

    /// <summary>
    ///     Sets a value indicating whether the index is sorted in tempdb.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="sortInTempDb">The value to set.</param>
    public static void SetSortInTempDb(this IMutableIndex index, bool? sortInTempDb)
        => index.SetAnnotation(
            SqlServerAnnotationNames.SortInTempDb,
            sortInTempDb);

    /// <summary>
    ///     Sets a value indicating whether the index is sorted in tempdb.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="sortInTempDb">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetSortInTempDb(
        this IConventionIndex index,
        bool? sortInTempDb,
        bool fromDataAnnotation = false)
        => (bool?)index.SetAnnotation(
            SqlServerAnnotationNames.SortInTempDb,
            sortInTempDb,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for whether the index is sorted in tempdb.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for whether the index is sorted in tempdb.</returns>
    public static ConfigurationSource? GetSortInTempDbConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.SortInTempDb)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the data compression that the index uses.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The data compression that the index uses</returns>
    public static DataCompressionType? GetDataCompression(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (DataCompressionType?)index[SqlServerAnnotationNames.DataCompression];

    /// <summary>
    ///     Returns the data compression that the index uses.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The data compression that the index uses</returns>
    public static DataCompressionType? GetDataCompression(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.DataCompression);
        if (annotation != null)
        {
            return (DataCompressionType?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetDataCompression(storeObject);
    }

    /// <summary>
    ///     Sets a value indicating the data compression the index uses.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="dataCompression">The value to set.</param>
    public static void SetDataCompression(this IMutableIndex index, DataCompressionType? dataCompression)
        => index.SetAnnotation(
            SqlServerAnnotationNames.DataCompression,
            dataCompression);

    /// <summary>
    ///     Sets a value indicating the data compression the index uses.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="dataCompression">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static DataCompressionType? SetDataCompression(
        this IConventionIndex index,
        DataCompressionType? dataCompression,
        bool fromDataAnnotation = false)
        => (DataCompressionType?)index.SetAnnotation(
            SqlServerAnnotationNames.DataCompression,
            dataCompression,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the data compression the index uses.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the data compression the index uses.</returns>
    public static ConfigurationSource? GetDataCompressionConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.DataCompression)?.GetConfigurationSource();
}
