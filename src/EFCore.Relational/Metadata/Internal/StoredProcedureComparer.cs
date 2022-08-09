// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
// Sealed for perf
public sealed class StoredProcedureComparer : IEqualityComparer<IStoredProcedure>, IComparer<IStoredProcedure>
{
    private StoredProcedureComparer()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly StoredProcedureComparer Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(IStoredProcedure? x, IStoredProcedure? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var xId = x.GetStoreIdentifier();
        var yId = y.GetStoreIdentifier();

        var result = 0;
        result = xId.StoreObjectType.CompareTo(yId.StoreObjectType);
        if (result != 0)
        {
            return result;
        }

        result = EntityTypeFullNameComparer.Instance.Compare(x.EntityType, y.EntityType);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(xId.Name, yId.Name);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(xId.Schema, yId.Schema);
        if (result != 0)
        {
            return result;
        }

        result = x.Parameters.Count().CompareTo(y.Parameters.Count());
        if (result != 0)
        {
            return result;
        }

        result = x.Parameters.Zip(y.Parameters, (xc, yc) => StringComparer.Ordinal.Compare(xc, yc))
            .FirstOrDefault(r => r != 0);
        if (result != 0)
        {
            return result;
        }

        return x.ResultColumns.Zip(y.ResultColumns, (xc, yc) => StringComparer.Ordinal.Compare(xc, yc))
            .FirstOrDefault(r => r != 0);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool Equals(IStoredProcedure? x, IStoredProcedure? y)
        => ReferenceEquals(x, y)
            || (x is not null
                && y is not null
                && x.EntityType == y.EntityType
                && x.GetStoreIdentifier() == y.GetStoreIdentifier()
                && x.Parameters.SequenceEqual(y.Parameters)
                && x.ResultColumns.SequenceEqual(y.ResultColumns));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int GetHashCode(IStoredProcedure obj)
        => obj.GetStoreIdentifier().GetHashCode();
}
