// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class TypeBaseExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryGetOrdinalKey(this ITypeBase structuralType, [NotNullWhen(true)] out IProperty? ordinalKeyProperty)
    {
        if (structuralType is IEntityType entityType && entityType.IsOwned())
        {
            ordinalKeyProperty = entityType.GetProperties().SingleOrDefault(x => x.IsOrdinalKeyProperty());
            return ordinalKeyProperty != null;
        }

        ordinalKeyProperty = null;
        return false;
    }
}
