// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerNetTopologySuiteMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerNetTopologySuiteMethodCallTranslatorPlugin(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        Translators = new IMethodCallTranslator[]
        {
            new SqlServerGeometryMethodTranslator(typeMappingSource, sqlExpressionFactory),
            new SqlServerGeometryCollectionMethodTranslator(typeMappingSource, sqlExpressionFactory),
            new SqlServerLineStringMethodTranslator(typeMappingSource, sqlExpressionFactory),
            new SqlServerNetTopologySuiteDbFunctionsMethodCallTranslator(typeMappingSource, sqlExpressionFactory),
            new SqlServerPolygonMethodTranslator(typeMappingSource, sqlExpressionFactory)
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
}
