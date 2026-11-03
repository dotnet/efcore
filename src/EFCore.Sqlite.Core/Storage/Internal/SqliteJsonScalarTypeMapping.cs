// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Globalization;
using System.Text.Json;

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

    private SqliteJsonScalarTypeMapping(
        RelationalTypeMappingParameters parameters,
        RelationalTypeMapping sourceMapping)
        : base(parameters)
    {
        SourceMapping = sourceMapping;
    }

    internal RelationalTypeMapping SourceMapping { get; }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteJsonScalarTypeMapping(parameters, SourceMapping);

    /// <inheritdoc />
    public override Expression GenerateCodeLiteral(object value)
        => SourceMapping.GenerateCodeLiteral(value);

    /// <inheritdoc />
    public override MethodInfo GetDataReaderMethod()
        => SourceMapping.GetDataReaderMethod();

    /// <inheritdoc />
    public override Expression CustomizeDataReaderExpression(Expression expression)
        => SourceMapping.CustomizeDataReaderExpression(expression);

    /// <inheritdoc />
    public override DbParameter CreateParameter(
        DbCommand command,
        string name,
        object? value,
        bool? nullable = null,
        ParameterDirection direction = ParameterDirection.Input)
        => (Converter?.ProviderClrType ?? ClrType) == typeof(byte[])
            ? SourceMapping.CreateParameter(command, name, value, nullable, direction)
            : CreateJsonParameter(this, command, name, value, nullable, direction);

    /// <inheritdoc />
    public override string GenerateSqlLiteral(object? value)
        => (Converter?.ProviderClrType ?? ClrType) == typeof(byte[])
            ? SourceMapping.GenerateSqlLiteral(value)
            : GenerateJsonSqlLiteral(this, value);

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

    private static (object? Value, RelationalTypeMapping Mapping) Serialize(RelationalTypeMapping mapping, object? value)
    {
        var readerWriter = mapping.JsonValueReaderWriter!;
        if (value is null && !readerWriter.HandlesNullWrites)
        {
            return (null, SqliteStringTypeMapping.Default);
        }

        if (value is not null && !mapping.ClrType.IsInstanceOfType(value))
        {
            if (mapping.Converter is null
                && mapping.ClrType != typeof(char)
                && mapping.ClrType.IsInteger()
                && value.GetType().IsInteger())
            {
                // A promoted comparison operand need not fit in the property's CLR range.
                return (value, LongTypeMapping.Default);
            }

            value = mapping.ClrType.IsEnum
                ? Enum.ToObject(mapping.ClrType, value)
                : Convert.ChangeType(value, mapping.ClrType.UnwrapNullableType(), CultureInfo.InvariantCulture);
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
