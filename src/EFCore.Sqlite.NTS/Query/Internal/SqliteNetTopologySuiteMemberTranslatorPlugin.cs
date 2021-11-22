// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteNetTopologySuiteMemberTranslatorPlugin : IMemberTranslatorPlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteNetTopologySuiteMemberTranslatorPlugin(
        ISqlExpressionFactory sqlExpressionFactory)
    {
        Translators = new IMemberTranslator[]
        {
            new SqliteGeometryMemberTranslator(sqlExpressionFactory),
            new SqliteGeometryCollectionMemberTranslator(sqlExpressionFactory),
            new SqliteLineStringMemberTranslator(sqlExpressionFactory),
            new SqliteMultiLineStringMemberTranslator(sqlExpressionFactory),
            new SqlitePointMemberTranslator(sqlExpressionFactory),
            new SqlitePolygonMemberTranslator(sqlExpressionFactory)
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IMemberTranslator> Translators { get; }
}
