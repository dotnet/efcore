// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Represents the results from a call to
///     <see cref="SqlServerQueryableExtensions.VectorSearch{T, TVector}(DbSet{T}, Expression{Func{T, TVector}}, TVector, string, int)" />.
/// </summary>
[Experimental(EFDiagnostics.SqlServerVectorSearch)]
public readonly struct VectorSearchResult<T>(T value, double distance)
{
    /// <summary>
    ///     The entity instance representing the row with a similar vector.
    /// </summary>
    public T Value { get; } = value;

    /// <summary>
    ///    The distance between the query vector and the similar vector.
    /// </summary>
    public double Distance { get; } = distance;
}
