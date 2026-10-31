// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Json.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteJsonDateTimeTypeMapping : SqliteDateTimeTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqliteJsonDateTimeTypeMapping Default { get; } = new(SqliteDateTimeTypeMapping.Default);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteJsonDateTimeTypeMapping(SqliteDateTimeTypeMapping mapping)
        : base(mapping, System.Data.DbType.String)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqliteJsonDateTimeTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteJsonDateTimeTypeMapping(parameters);

    internal static SqliteDateTimeTypeMapping Create(SqliteDateTimeTypeMapping mapping)
        => mapping is SqliteJsonDateTimeTypeMapping || GetProviderJsonValueReaderWriter(mapping) is SqliteJsonDateTimeReaderWriter
            ? mapping
            : new SqliteJsonDateTimeTypeMapping(mapping);

#pragma warning disable EF1001 // Internal EF Core API usage.
    private static JsonValueReaderWriter GetProviderJsonValueReaderWriter(CoreTypeMapping mapping)
        => mapping.JsonValueReaderWriter is IJsonConvertedValueReaderWriter convertedReaderWriter
            && ReferenceEquals(convertedReaderWriter.Converter, mapping.Converter)
                ? convertedReaderWriter.InnerReaderWriter
                : mapping.JsonValueReaderWriter!;
#pragma warning restore EF1001

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter.Value is { } value
            && value != DBNull.Value)
        {
            parameter.Value = (object?)GetJsonString(value) ?? DBNull.Value;
        }
    }

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => SqliteStringTypeMapping.Default.GenerateSqlLiteral(GetJsonString(value));

    private string? GetJsonString(object value)
    {
        using var document = JsonDocument.Parse(GetProviderJsonValueReaderWriter(this).ToJsonString(value));
        return document.RootElement.GetString();
    }
}
