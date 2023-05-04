// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.SqlServer.Types;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerHierarchyIdValueConverter : ValueConverter<HierarchyId?, SqlHierarchyId>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerHierarchyIdValueConverter()
        : base(h => ToProvider(h), b => FromProvider(b))
    {
    }

    private static SqlHierarchyId ToProvider(HierarchyId? value)
        => (SqlHierarchyId)value;

    private static HierarchyId? FromProvider(SqlHierarchyId value)
        => (HierarchyId?)value;
}
