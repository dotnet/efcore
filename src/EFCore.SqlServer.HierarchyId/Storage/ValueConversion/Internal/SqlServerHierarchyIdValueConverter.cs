// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerHierarchyIdValueConverter : ValueConverter<HierarchyId, SqlBytes>
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

    private static SqlBytes ToProvider(HierarchyId hid)
    {
        using var memory = new MemoryStream();
        using var writer = new BinaryWriter(memory);

        hid.Write(writer);
        return new SqlBytes(memory.ToArray());
    }

    private static HierarchyId FromProvider(SqlBytes bytes)
    {
        using var memory = new MemoryStream(bytes.Value);
        using var reader = new BinaryReader(memory);

        return HierarchyId.Read(reader)!;
    }
}
