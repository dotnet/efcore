// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerSequenceHiLoValueGenerator<TValue> : HiLoValueGenerator<TValue>
{
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly ISqlServerUpdateSqlGenerator _sqlGenerator;
    private readonly ISqlServerConnection _connection;
    private readonly ISequence _sequence;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSequenceHiLoValueGenerator(
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        ISqlServerUpdateSqlGenerator sqlGenerator,
        SqlServerSequenceValueGeneratorState generatorState,
        ISqlServerConnection connection,
        IRelationalCommandDiagnosticsLogger commandLogger)
        : base(generatorState)
    {
        _sequence = generatorState.Sequence;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _sqlGenerator = sqlGenerator;
        _connection = connection;
        _commandLogger = commandLogger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override long GetNewLowValue()
        => (long)Convert.ChangeType(
            _rawSqlCommandBuilder
                .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                .ExecuteScalar(
                    new RelationalCommandParameterObject(
                        _connection,
                        parameterValues: null,
                        readerColumns: null,
                        context: null,
                        _commandLogger, CommandSource.ValueGenerator)),
            typeof(long),
            CultureInfo.InvariantCulture)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override async Task<long> GetNewLowValueAsync(CancellationToken cancellationToken = default)
        => (long)Convert.ChangeType(
            await _rawSqlCommandBuilder
                .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                .ExecuteScalarAsync(
                    new RelationalCommandParameterObject(
                        _connection,
                        parameterValues: null,
                        readerColumns: null,
                        context: null,
                        _commandLogger, CommandSource.ValueGenerator),
                    cancellationToken)
                .ConfigureAwait(false),
            typeof(long),
            CultureInfo.InvariantCulture)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool GeneratesTemporaryValues
        => false;
}
