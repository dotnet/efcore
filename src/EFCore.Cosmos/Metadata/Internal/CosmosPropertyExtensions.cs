// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosPropertyExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsOrdinalKeyProperty(this IReadOnlyProperty property)
    {
        Check.DebugAssert(
            property.DeclaringEntityType.IsOwned(), $"Expected {property.DeclaringEntityType.DisplayName()} to be owned.");
        Check.DebugAssert(property.GetJsonPropertyName().Length == 0, $"Expected {property.Name} to be non-persisted.");

        return property.FindContainingPrimaryKey() is IReadOnlyKey key
            && key.Properties.Count > 1
            && !property.IsForeignKey()
            && property.ClrType == typeof(int)
            && property.ValueGenerated == ValueGenerated.OnAdd;
    }
}
