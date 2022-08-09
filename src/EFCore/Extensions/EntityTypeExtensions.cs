// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Entity type extension methods for <see cref="IReadOnlyEntityType" />.
/// </summary>
[Obsolete("Use IReadOnlyEntityType")] // Delete with defining query
public static class EntityTypeExtensions
{
    /// <summary>
    ///     Gets the LINQ query used as the default source for queries of this type.
    /// </summary>
    /// <param name="entityType">The entity type to get the defining query for.</param>
    /// <returns>The LINQ query used as the default source.</returns>
    [Obsolete("Use InMemoryEntityTypeExtensions.GetInMemoryQuery")]
    public static LambdaExpression? GetDefiningQuery(this IEntityType entityType)
        => (LambdaExpression?)entityType[CoreAnnotationNames.DefiningQuery];
}
