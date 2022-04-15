// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IMutableEntityType" />.
/// </summary>
[Obsolete("Use IMutableEntityType")] // Delete with defining query
public static class MutableEntityTypeExtensions
{
    /// <summary>
    ///     Sets the LINQ query used as the default source for queries of this type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="definingQuery">The LINQ query used as the default source.</param>
    [Obsolete("Use InMemoryEntityTypeExtensions.SetInMemoryQuery")]
    public static void SetDefiningQuery(
        this IMutableEntityType entityType,
        LambdaExpression? definingQuery)
        => ((EntityType)entityType).SetDefiningQuery(definingQuery, ConfigurationSource.Explicit);
}
