﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteJsonTypeMapping : JsonTypeMapping
{
    private static readonly MethodInfo GetStringMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetString), new[] { typeof(int) })!;

    private static readonly PropertyInfo UTF8Property
        = typeof(Encoding).GetProperty(nameof(Encoding.UTF8))!;

    private static readonly MethodInfo EncodingGetBytesMethod
        = typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), new[] { typeof(string) })!;

    private static readonly ConstructorInfo MemoryStreamConstructor
        = typeof(MemoryStream).GetConstructor(new[] { typeof(byte[]) })!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqliteJsonTypeMapping Default { get; } = new(SqliteTypeMappingSource.TextTypeName);

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteJsonTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    public SqliteJsonTypeMapping(string storeType)
        : base(storeType, typeof(JsonElement), System.Data.DbType.String)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqliteJsonTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override MethodInfo GetDataReaderMethod()
        => GetStringMethod;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression CustomizeDataReaderExpression(Expression expression)
        => Expression.New(
            MemoryStreamConstructor,
            Expression.Call(
                Expression.Property(null, UTF8Property),
                EncodingGetBytesMethod,
                expression));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{EscapeSqlLiteral(JsonSerializer.Serialize(value))}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string EscapeSqlLiteral(string literal)
        => literal.Replace("'", "''");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteJsonTypeMapping(parameters);
}
