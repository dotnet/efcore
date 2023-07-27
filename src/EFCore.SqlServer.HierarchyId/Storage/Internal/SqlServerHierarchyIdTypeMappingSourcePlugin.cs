// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.SqlServer.Types;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerHierarchyIdTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;

        if (string.Equals(storeTypeName, "hierarchyid", StringComparison.OrdinalIgnoreCase))
        {
            if (clrType is null
                || clrType == typeof(HierarchyId))
            {
                return SqlServerHierarchyIdTypeMapping.Default;
            }

            if (clrType == typeof(SqlHierarchyId))
            {
                return SqlServerSqlHierarchyIdTypeMapping.Default;
            }

            return null;
        }

        if (clrType == typeof(HierarchyId))
        {
            return SqlServerHierarchyIdTypeMapping.Default;
        }

        if (clrType == typeof(SqlHierarchyId))
        {
            return SqlServerSqlHierarchyIdTypeMapping.Default;
        }

        return null;
    }
}
