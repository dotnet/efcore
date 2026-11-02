// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteJsonScalarTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteJsonScalarTypeMapping(RelationalTypeMapping mapping)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    mapping.ClrType, mapping.Converter, mapping.Comparer, mapping.KeyComparer, mapping.ProviderValueComparer,
                    elementMapping: mapping.ElementTypeMapping, jsonValueReaderWriter: mapping.JsonValueReaderWriter),
                mapping.StoreType, mapping.StoreTypePostfix, mapping.DbType, mapping.IsUnicode,
                mapping.Size, mapping.IsFixedLength, mapping.Precision, mapping.Scale),
            mapping)
    {
    }

    private SqliteJsonScalarTypeMapping(RelationalTypeMappingParameters parameters, RelationalTypeMapping sourceMapping)
        : base(parameters)
        => SourceMapping = sourceMapping;

    internal RelationalTypeMapping SourceMapping { get; }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteJsonScalarTypeMapping(parameters, SourceMapping);

    /// <inheritdoc />
    public override Expression GenerateCodeLiteral(object value)
        => SourceMapping.GenerateCodeLiteral(value);

    /// <inheritdoc />
    public override MethodInfo GetDataReaderMethod()
        => (Converter?.ProviderClrType ?? ClrType) == typeof(byte[])
            ? typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetValue), [typeof(int)])!
            : SourceMapping.GetDataReaderMethod();

    /// <inheritdoc />
    public override Expression CustomizeDataReaderExpression(Expression expression)
        => (Converter?.ProviderClrType ?? ClrType) == typeof(byte[])
            ? ReadJsonExpression(this, expression)
            : SourceMapping.CustomizeDataReaderExpression(expression);

    /// <inheritdoc />
    public override DbParameter CreateParameter(
        DbCommand command,
        string name,
        object? value,
        bool? nullable = null,
        ParameterDirection direction = ParameterDirection.Input)
        => CreateJsonParameter(this, command, name, value, nullable, direction);

    /// <inheritdoc />
    public override string GenerateSqlLiteral(object? value)
        => GenerateJsonSqlLiteral(this, value);

    internal static DbParameter CreateJsonParameter(
        RelationalTypeMapping mapping,
        DbCommand command,
        string name,
        object? value,
        bool? nullable,
        ParameterDirection direction)
    {
        var (scalar, scalarMapping) = Serialize(mapping, value);
        return scalarMapping.CreateParameter(command, name, scalar, nullable, direction);
    }

    internal static string GenerateJsonSqlLiteral(RelationalTypeMapping mapping, object? value)
    {
        var (scalar, scalarMapping) = Serialize(mapping, value);
        return scalarMapping.GenerateSqlLiteral(scalar);
    }

    internal static Expression ReadJsonExpression(RelationalTypeMapping mapping, Expression expression)
    {
        var readerWriter = SqliteTypeMappingSource.GetProviderReaderWriter(mapping)!;
        var converter = ReferenceEquals(readerWriter, mapping.JsonValueReaderWriter) ? mapping.Converter : null;
        return Expression.Convert(
            Expression.Call(
                typeof(SqliteJsonScalarTypeMapping).GetMethod(nameof(ReadJsonValue))!,
                expression,
                Expression.Constant(readerWriter, typeof(JsonValueReaderWriter)),
                Expression.Constant(converter, typeof(ValueConverter))),
            mapping.Converter?.ProviderClrType ?? mapping.ClrType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static object? ReadJsonValue(object value, JsonValueReaderWriter readerWriter, ValueConverter? converter)
    {
        var json = value switch
        {
            string text => "\"" + JsonEncodedText.Encode(text) + "\"",
            long number => number.ToString(CultureInfo.InvariantCulture),
            double number => number.ToString("R", CultureInfo.InvariantCulture),
            _ => throw new UnreachableException()
        };
        var result = readerWriter.FromJsonString(json);
        return converter is null ? result : converter.ConvertToProvider(result);
    }

    private static (object? Value, RelationalTypeMapping Mapping) Serialize(RelationalTypeMapping mapping, object? value)
    {
        var readerWriter = mapping.JsonValueReaderWriter!;
        if (value is null && !readerWriter.HandlesNullWrites)
        {
            return (null, SqliteStringTypeMapping.Default);
        }

        if (value?.GetType().IsInteger() == true && mapping.ClrType.IsEnum)
        {
            value = Enum.ToObject(mapping.ClrType, value);
        }

        using var document = JsonDocument.Parse(readerWriter.ToJsonString(value));
        var element = document.RootElement;
        return element.ValueKind switch
        {
            JsonValueKind.Null => (null, SqliteStringTypeMapping.Default),
            JsonValueKind.String => (element.GetString(), SqliteStringTypeMapping.Default),
            JsonValueKind.Number when element.TryGetInt64(out var number) => (number, LongTypeMapping.Default),
            JsonValueKind.Number => (element.GetDouble(), DoubleTypeMapping.Default),
            JsonValueKind.True => (1L, LongTypeMapping.Default),
            JsonValueKind.False => (0L, LongTypeMapping.Default),
            _ => (element.GetRawText(), SqliteStringTypeMapping.Default)
        };
    }
}
