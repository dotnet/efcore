// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerFullTextCatalogTypeMapping : RelationalTypeMapping
{
    private static readonly ConstructorInfo _catalogConstructor
        = typeof(SqlServerFullTextCatalog).GetConstructor(
            [typeof(string), typeof(bool), typeof(bool)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqlServerFullTextCatalogTypeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerFullTextCatalogTypeMapping()
        : this(new RelationalTypeMappingParameters(new CoreTypeMappingParameters(typeof(SqlServerFullTextCatalog)), "fulltextcatalog"))
    {
    }

    private SqlServerFullTextCatalogTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerFullTextCatalogTypeMapping(parameters);

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => throw new NotSupportedException();

    /// <inheritdoc />
    public override Expression GenerateCodeLiteral(object value)
    {
        var catalog = (SqlServerFullTextCatalog)value;

        return Expression.New(
            _catalogConstructor,
            Expression.Constant(catalog.Name),
            Expression.Constant(catalog.IsDefault),
            Expression.Constant(catalog.IsAccentSensitive));
    }
}
