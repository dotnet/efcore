// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class TableBaseIdentityComparer : IEqualityComparer<ITableBase>
{
    /// <inheritdoc />
    public bool Equals(ITableBase? x, ITableBase? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return y is null;
        }

        if (y is null)
        {
            return false;
        }

        return x.Name == y.Name
            && x.Schema == y.Schema;
    }

    /// <inheritdoc />
    public int GetHashCode([DisallowNull] ITableBase obj)
    {
        var hash = new HashCode();
        hash.Add(obj.Name);
        hash.Add(obj.Schema);

        return hash.ToHashCode();
    }
}
