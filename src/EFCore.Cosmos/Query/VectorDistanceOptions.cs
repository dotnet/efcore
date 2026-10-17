// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Options to be passed to
///     <see
///         cref="CosmosDbFunctionsExtensions.VectorDistance(DbFunctions, ReadOnlyMemory{float}, ReadOnlyMemory{float}, bool?, VectorDistanceOptions)" />
/// </summary>
public sealed class VectorDistanceOptions
{
    /// <summary>
    ///     The metric used to compute distance/similarity.
    /// </summary>
    public DistanceFunction? DistanceFunction { get; set; }

    /// <summary>
    ///     The data type of the vectors. <c>float32</c>, <c>int8</c>, <c>uint8</c> values. Default value is <c>float32</c>.
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    ///     An integer specifying the size of the search list when conducting a vector search on the DiskANN index.
    ///     Increasing this may improve accuracy at the expense of RU cost and latency. Min=1, Default=10, Max=100.
    /// </summary>
    public int? SearchListSizeMultiplier { get; set; }

    /// <summary>
    ///     An integer specifying the size of the search list when conducting a vector search on the quantizedFlat index.
    ///     Increasing this may improve accuracy at the expense of RU cost and latency. Min=1, Default=5, Max=100.
    /// </summary>
    public int? QuantizedVectorListMultiplier { get; set; }
}
