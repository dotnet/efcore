// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
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
    #region IsClustered

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

    #endregion IsClustered

    #region IncludeProperties

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

    #endregion IncludeProperties

    #region IsCreatedOnline

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

    #endregion IsCreatedOnline

    #region FillFactor

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

    #endregion FillFactor

    #region SortInTempDb

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

    #endregion SortInTempDb

    #region DataCompression

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

    #endregion DataCompression

    #region VectorMetric

    /// <summary>
    ///     Returns whether the index is a vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Whether the index is a vector index.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static bool IsVectorIndex(this IReadOnlyIndex index)
        => index is RuntimeIndex
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : index.FindAnnotation(SqlServerAnnotationNames.VectorIndexMetric) is not null;

    /// <summary>
    ///     Returns the similarity metric for the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The similarity metric for the vector index.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static string? GetVectorMetric(this IReadOnlyIndex index)
        => index is RuntimeIndex
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)index[SqlServerAnnotationNames.VectorIndexMetric];

    /// <summary>
    ///     Returns the similarity metric for the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The similarity metric for the vector index.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static string? GetVectorMetric(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.VectorIndexMetric);
        if (annotation != null)
        {
            return (string?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetVectorMetric(storeObject);
    }

    /// <summary>
    ///     Sets the similarity metric for the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="metric">The value to set.</param>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static void SetVectorMetric(this IMutableIndex index, string? metric)
        => index.SetAnnotation(SqlServerAnnotationNames.VectorIndexMetric, metric);

    /// <summary>
    ///     Sets the similarity metric for the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="metric">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static string? SetVectorMetric(
        this IConventionIndex index,
        string? metric,
        bool fromDataAnnotation = false)
        => (string?)index.SetAnnotation(
            SqlServerAnnotationNames.VectorIndexMetric,
            metric,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the similarity metric of the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the similarity metric of the vector index.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static ConfigurationSource? GetVectorMetricConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.VectorIndexMetric)?.GetConfigurationSource();

    #endregion VectorMetric

    #region VectorIndexType

    /// <summary>
    ///     Returns the type of the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The type of the vector index.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static string? GetVectorIndexType(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)index[SqlServerAnnotationNames.VectorIndexType];

    /// <summary>
    ///     Returns the type of the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The type of the vector index.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static string? GetVectorIndexType(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.VectorIndexType);
        if (annotation != null)
        {
            return (string?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetVectorIndexType(storeObject);
    }

    /// <summary>
    ///     Sets the type of the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="type">The value to set.</param>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static void SetVectorIndexType(this IMutableIndex index, string? type)
        => index.SetAnnotation(SqlServerAnnotationNames.VectorIndexType, type);

    /// <summary>
    ///     Sets the type of the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="type">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static string? SetVectorIndexType(
        this IConventionIndex index,
        string? type,
        bool fromDataAnnotation = false)
        => (string?)index.SetAnnotation(
            SqlServerAnnotationNames.VectorIndexType,
            type,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the type of the vector index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the type of the vector index.</returns>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static ConfigurationSource? GetVectorIndexTypeConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.VectorIndexType)?.GetConfigurationSource();

    #endregion VectorIndexType

    #region FullTextKeyIndex

    /// <summary>
    ///     Returns whether the index is a full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns><see langword="true" /> if the index is a full-text index.</returns>
    public static bool IsFullTextIndex(this IReadOnlyIndex index)
        => index is RuntimeIndex
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : index.FindAnnotation(SqlServerAnnotationNames.FullTextIndex) is not null;

    /// <summary>
    ///     Returns the KEY INDEX name for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The KEY INDEX name, or <see langword="null" /> if the index is not a full-text index.</returns>
    public static string? GetFullTextKeyIndex(this IReadOnlyIndex index)
        => index is RuntimeIndex
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)index[SqlServerAnnotationNames.FullTextIndex];

    /// <summary>
    ///     Returns the KEY INDEX name for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The KEY INDEX name, or <see langword="null" /> if the index is not a full-text index.</returns>
    public static string? GetFullTextKeyIndex(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.FullTextIndex);
        if (annotation != null)
        {
            return (string?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetFullTextKeyIndex(storeObject);
    }

    /// <summary>
    ///     Sets the KEY INDEX name for the full-text index. Setting a non-null value marks this index as a full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="keyIndexName">The KEY INDEX name to set.</param>
    public static void SetFullTextKeyIndex(this IMutableIndex index, string? keyIndexName)
        => index.SetAnnotation(SqlServerAnnotationNames.FullTextIndex, keyIndexName);

    /// <summary>
    ///     Sets the KEY INDEX name for the full-text index. Setting a non-null value marks this index as a full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="keyIndexName">The KEY INDEX name to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetFullTextKeyIndex(
        this IConventionIndex index,
        string? keyIndexName,
        bool fromDataAnnotation = false)
        => (string?)index.SetAnnotation(
            SqlServerAnnotationNames.FullTextIndex,
            keyIndexName,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the KEY INDEX of the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the KEY INDEX.</returns>
    public static ConfigurationSource? GetFullTextKeyIndexConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.FullTextIndex)?.GetConfigurationSource();

    #endregion FullTextKeyIndex

    #region FullTextCatalog

    /// <summary>
    ///     Returns the full-text catalog name for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The full-text catalog name, or <see langword="null" /> if not set.</returns>
    public static string? GetFullTextCatalog(this IReadOnlyIndex index)
        => index is RuntimeIndex
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)index[SqlServerAnnotationNames.FullTextCatalog];

    /// <summary>
    ///     Returns the full-text catalog name for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The full-text catalog name, or <see langword="null" /> if not set.</returns>
    public static string? GetFullTextCatalog(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.FullTextCatalog);
        if (annotation != null)
        {
            return (string?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetFullTextCatalog(storeObject);
    }

    /// <summary>
    ///     Sets the full-text catalog name for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="catalogName">The catalog name to set.</param>
    public static void SetFullTextCatalog(this IMutableIndex index, string? catalogName)
        => index.SetAnnotation(SqlServerAnnotationNames.FullTextCatalog, catalogName);

    /// <summary>
    ///     Sets the full-text catalog name for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="catalogName">The catalog name to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetFullTextCatalog(
        this IConventionIndex index,
        string? catalogName,
        bool fromDataAnnotation = false)
        => (string?)index.SetAnnotation(
            SqlServerAnnotationNames.FullTextCatalog,
            catalogName,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the full-text catalog of the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the full-text catalog.</returns>
    public static ConfigurationSource? GetFullTextCatalogConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.FullTextCatalog)?.GetConfigurationSource();

    #endregion FullTextCatalog

    #region FullTextChangeTracking

    /// <summary>
    ///     Returns the change tracking mode for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The change tracking mode, or <see langword="null" /> if not set.</returns>
    public static FullTextChangeTracking? GetFullTextChangeTracking(this IReadOnlyIndex index)
        => index is RuntimeIndex
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (FullTextChangeTracking?)index[SqlServerAnnotationNames.FullTextChangeTracking];

    /// <summary>
    ///     Returns the change tracking mode for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The change tracking mode, or <see langword="null" /> if not set.</returns>
    public static FullTextChangeTracking? GetFullTextChangeTracking(
        this IReadOnlyIndex index,
        in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SqlServerAnnotationNames.FullTextChangeTracking);
        if (annotation != null)
        {
            return (FullTextChangeTracking?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetFullTextChangeTracking(storeObject);
    }

    /// <summary>
    ///     Sets the change tracking mode for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="changeTracking">The change tracking mode to set.</param>
    public static void SetFullTextChangeTracking(this IMutableIndex index, FullTextChangeTracking? changeTracking)
        => index.SetAnnotation(SqlServerAnnotationNames.FullTextChangeTracking, changeTracking);

    /// <summary>
    ///     Sets the change tracking mode for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="changeTracking">The change tracking mode to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static FullTextChangeTracking? SetFullTextChangeTracking(
        this IConventionIndex index,
        FullTextChangeTracking? changeTracking,
        bool fromDataAnnotation = false)
        => (FullTextChangeTracking?)index.SetAnnotation(
            SqlServerAnnotationNames.FullTextChangeTracking,
            changeTracking,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the change tracking mode of the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the change tracking mode.</returns>
    public static ConfigurationSource? GetFullTextChangeTrackingConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.FullTextChangeTracking)?.GetConfigurationSource();

    #endregion FullTextChangeTracking

    #region FullTextLanguage

    /// <summary>
    ///     Returns the full-text language for a specific property in the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The language term, or <see langword="null" /> if not set.</returns>
    public static string? GetFullTextLanguage(this IReadOnlyIndex index, string propertyName)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var languages = (IReadOnlyDictionary<string, string>?)index[SqlServerAnnotationNames.FullTextLanguages];
        return languages != null && languages.TryGetValue(propertyName, out var language) ? language : null;
    }

    /// <summary>
    ///     Returns all full-text languages configured for the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>A dictionary of property names to language terms, or <see langword="null" /> if none are set.</returns>
    public static IReadOnlyDictionary<string, string>? GetFullTextLanguages(this IReadOnlyIndex index)
        => index is RuntimeIndex
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (IReadOnlyDictionary<string, string>?)index[SqlServerAnnotationNames.FullTextLanguages];

    /// <summary>
    ///     Sets the full-text language for a specific property in the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="language">The language term to set, or <see langword="null" /> to remove.</param>
    public static void SetFullTextLanguage(this IMutableIndex index, string propertyName, string? language)
    {
        var languages = (Dictionary<string, string>?)index[SqlServerAnnotationNames.FullTextLanguages];
        if (language is null)
        {
            if (languages != null)
            {
                languages.Remove(propertyName);
                if (languages.Count == 0)
                {
                    index.RemoveAnnotation(SqlServerAnnotationNames.FullTextLanguages);
                }
            }
        }
        else
        {
            languages ??= [];
            languages[propertyName] = language;
            index.SetAnnotation(SqlServerAnnotationNames.FullTextLanguages, languages);
        }
    }

    /// <summary>
    ///     Sets the full-text languages for all properties in the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="languages">A dictionary of property names to language terms, or <see langword="null" /> to remove all.</param>
    public static void SetFullTextLanguages(this IMutableIndex index, IReadOnlyDictionary<string, string>? languages)
        => index.SetAnnotation(SqlServerAnnotationNames.FullTextLanguages, languages);

    /// <summary>
    ///     Sets the full-text language for a specific property in the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="language">The language term to set, or <see langword="null" /> to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetFullTextLanguage(
        this IConventionIndex index,
        string propertyName,
        string? language,
        bool fromDataAnnotation = false)
    {
        var languages = (Dictionary<string, string>?)index[SqlServerAnnotationNames.FullTextLanguages];
        if (language is null)
        {
            if (languages != null)
            {
                languages.Remove(propertyName);
                if (languages.Count == 0)
                {
                    index.RemoveAnnotation(SqlServerAnnotationNames.FullTextLanguages);
                }
            }
        }
        else
        {
            languages ??= [];
            languages[propertyName] = language;
            index.SetAnnotation(SqlServerAnnotationNames.FullTextLanguages, languages, fromDataAnnotation);
        }

        return language;
    }

    /// <summary>
    ///     Sets the full-text languages for all properties in the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="languages">A dictionary of property names to language terms, or <see langword="null" /> to remove all.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static IReadOnlyDictionary<string, string>? SetFullTextLanguages(
        this IConventionIndex index,
        IReadOnlyDictionary<string, string>? languages,
        bool fromDataAnnotation = false)
        => (IReadOnlyDictionary<string, string>?)index.SetAnnotation(
            SqlServerAnnotationNames.FullTextLanguages,
            languages,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the full-text languages of the full-text index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the full-text languages.</returns>
    public static ConfigurationSource? GetFullTextLanguagesConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SqlServerAnnotationNames.FullTextLanguages)?.GetConfigurationSource();

    #endregion FullTextLanguage
}
