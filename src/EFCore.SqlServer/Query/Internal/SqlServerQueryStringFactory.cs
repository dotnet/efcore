// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryStringFactory : IRelationalQueryStringFactory
{
    private readonly IRelationalTypeMappingSource _typeMapper;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQueryStringFactory(IRelationalTypeMappingSource typeMapper)
    {
        _typeMapper = typeMapper;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Create(DbCommand command)
    {
        if (command.Parameters.Count == 0)
        {
            return command.CommandText;
        }

        var builder = new StringBuilder();
        foreach (DbParameter parameter in command.Parameters)
        {
            var typeName = TypeNameBuilder.CreateTypeName(parameter);

            builder
                .Append("DECLARE ")
                .Append(parameter.ParameterName)
                .Append(' ')
                .Append(typeName)
                .Append(" = ");

            if (parameter.Value == DBNull.Value || parameter.Value is null)
            {
                builder.Append("NULL");
            }
            else
            {
                var typeMapping = _typeMapper.FindMapping(parameter.Value.GetType(), typeName);

                builder
                    .Append(
                        parameter.Value is SqlBytes sqlBytes
                            ? new SqlServerByteArrayTypeMapping(typeName).GenerateSqlLiteral(sqlBytes.Value)
                            : typeMapping != null
                                ? typeMapping.GenerateSqlLiteral(parameter.Value)
                                : parameter.Value.ToString());
            }

            builder.AppendLine(";");
        }

        return builder
            .AppendLine()
            .Append(command.CommandText).ToString();
    }
}

internal static class TypeNameBuilder
{
    private static StringBuilder AppendSize(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter.Size > 0)
        {
            builder
                .Append('(')
                .Append(parameter.Size.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        return builder;
    }

    private static StringBuilder AppendSizeOrMax(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter.Size > 0)
        {
            builder.AppendSize(parameter);
        }
        else if (parameter.Size == -1)
        {
            builder.Append("(max)");
        }

        return builder;
    }

    private static StringBuilder AppendPrecision(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter.Precision > 0)
        {
            builder
                .Append('(')
                .Append(parameter.Precision.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        return builder;
    }

    private static StringBuilder AppendScale(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter.Scale > 0)
        {
            builder
                .Append('(')
                .Append(parameter.Scale.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        return builder;
    }

    private static StringBuilder AppendPrecisionAndScale(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter is { Precision: > 0, Scale: > 0 })
        {
            return builder
                .Append('(')
                .Append(parameter.Precision.ToString(CultureInfo.InvariantCulture))
                .Append(',')
                .Append(parameter.Scale.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        return builder.AppendPrecision(parameter);
    }

    public static string CreateTypeName(DbParameter parameter)
    {
        if (parameter is SqlParameter sqlParameter)
        {
            var builder = new StringBuilder();
            return (sqlParameter.SqlDbType switch
            {
                SqlDbType.BigInt => builder.Append("bigint"),
                SqlDbType.Binary => builder.Append("binary").AppendSize(parameter),
                SqlDbType.Bit => builder.Append("bit"),
                SqlDbType.Char => builder.Append("char").AppendSize(parameter),
                SqlDbType.Date => builder.Append("date"),
                SqlDbType.DateTime => builder.Append("datetime"),
                SqlDbType.DateTime2 => builder.Append("datetime2").AppendPrecision(parameter),
                SqlDbType.DateTimeOffset => builder.Append("datetimeoffset").AppendPrecision(parameter),
                SqlDbType.Decimal => builder.Append("decimal").AppendPrecisionAndScale(parameter),
                SqlDbType.Float => builder.Append("float").AppendSize(parameter),
                SqlDbType.Image => builder.Append("image"),
                SqlDbType.Int => builder.Append("int"),
                SqlDbType.Money => builder.Append("money"),
                SqlDbType.NChar => builder.Append("nchar").AppendSize(parameter),
                SqlDbType.NText => builder.Append("ntext"),
                SqlDbType.NVarChar => builder.Append("nvarchar").AppendSizeOrMax(parameter),
                SqlDbType.Real => builder.Append("real"),
                SqlDbType.SmallDateTime => builder.Append("smalldatetime"),
                SqlDbType.SmallInt => builder.Append("smallint"),
                SqlDbType.SmallMoney => builder.Append("smallmoney"),
                SqlDbType.Structured => builder.Append("structured"),
                SqlDbType.Text => builder.Append("text"),
                SqlDbType.Time => builder.Append("time").AppendScale(parameter),
                SqlDbType.Timestamp => builder.Append("rowversion"),
                SqlDbType.TinyInt => builder.Append("tinyint"),
                SqlDbType.Udt => builder.Append(sqlParameter.UdtTypeName),
                SqlDbType.UniqueIdentifier => builder.Append("uniqueIdentifier"),
                SqlDbType.VarBinary => builder.Append("varbinary").AppendSizeOrMax(parameter),
                SqlDbType.VarChar => builder.Append("varchar").AppendSizeOrMax(parameter),
                SqlDbType.Variant => builder.Append("sql_variant"),
                SqlDbType.Xml => builder.Append("xml"),
                _ => builder.Append("sql_variant")
            }).ToString();
        }

        return "sql_variant";
    }
}
