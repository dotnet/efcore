// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// Represents a query filter in a model.
/// </summary>
public interface IQueryFilter
{
    /// <summary>
    /// The LINQ expression of the filter.
    /// </summary>
    LambdaExpression? Expression { get; }

    /// <summary>
    /// The name of the filter.
    /// </summary>
    string? Key { get; }

    /// <summary>
    /// Indicates whether the query filter is anonymous.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Key))]
    bool IsAnonymous => Key == null;
}
