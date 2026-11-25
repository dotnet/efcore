// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class SqliteDynamicTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqliteDynamicTypeMapping Default { get; } = new("TEXT");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteDynamicTypeMapping(string storeType)
        : base(storeType, typeof(object))
    {
    }

    private SqliteDynamicTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteDynamicTypeMapping(parameters);

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => value switch
        {
            string => SqliteStringTypeMapping.Default.GenerateSqlLiteral(value),
            long => LongTypeMapping.Default.GenerateSqlLiteral(value),
            double => DoubleTypeMapping.Default.GenerateSqlLiteral(value),
            byte[] => SqliteByteArrayTypeMapping.Default.GenerateSqlLiteral(value),
            _ => throw new InvalidOperationException(CoreStrings.TranslationFailed(value.GetType().ShortDisplayName()))
        };
}
