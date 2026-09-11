// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerSequenceValueGeneratorFactory : ISqlServerSequenceValueGeneratorFactory
{
    private readonly ISqlServerUpdateSqlGenerator _sqlGenerator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSequenceValueGeneratorFactory(
        ISqlServerUpdateSqlGenerator sqlGenerator)
        => _sqlGenerator = sqlGenerator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueGenerator? TryCreate(
        IProperty property,
        Type type,
        SqlServerSequenceValueGeneratorState generatorState,
        ISqlServerConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IRelationalCommandDiagnosticsLogger commandLogger)
    {
        if (type == typeof(long))
        {
            return new SqlServerSequenceHiLoValueGenerator<long>(
                rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
        }

        if (type == typeof(int))
        {
            return new SqlServerSequenceHiLoValueGenerator<int>(
                rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
        }

        if (type == typeof(decimal))
        {
            return new SqlServerSequenceHiLoValueGenerator<decimal>(
                rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
        }

        if (type == typeof(short))
        {
            return new SqlServerSequenceHiLoValueGenerator<short>(
                rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
        }

        if (type == typeof(byte))
        {
            return new SqlServerSequenceHiLoValueGenerator<byte>(
                rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
        }

        if (type == typeof(char))
        {
            return new SqlServerSequenceHiLoValueGenerator<char>(
                rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
        }

        return type == typeof(ulong)
            ? new SqlServerSequenceHiLoValueGenerator<ulong>(
                rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger)
            : type == typeof(uint)
                ? new SqlServerSequenceHiLoValueGenerator<uint>(
                    rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger)
                : type == typeof(ushort)
                    ? new SqlServerSequenceHiLoValueGenerator<ushort>(
                        rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger)
                    : type == typeof(sbyte)
                        ? new SqlServerSequenceHiLoValueGenerator<sbyte>(
                            rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger)
                        : (ValueGenerator?)null;
    }
}
