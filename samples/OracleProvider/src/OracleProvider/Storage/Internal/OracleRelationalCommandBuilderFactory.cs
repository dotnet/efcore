// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleRelationalCommandBuilderFactory : RelationalCommandBuilderFactory
    {
        public OracleRelationalCommandBuilderFactory(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(logger, typeMapper)
        {
        }

        protected override IRelationalCommandBuilder CreateCore(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            IRelationalTypeMapper relationalTypeMapper)
            => new OracleRelationalCommandBuilder(logger, relationalTypeMapper);

        private sealed class OracleRelationalCommandBuilder : RelationalCommandBuilder
        {
            public OracleRelationalCommandBuilder(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                IRelationalTypeMapper typeMapper)
                : base(logger, typeMapper)
            {
            }

            protected override IRelationalCommand BuildCore(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                string commandText,
                IReadOnlyList<IRelationalParameter> parameters)
                => new OracleRelationalCommand(logger, commandText, parameters);

            private sealed class OracleRelationalCommand : RelationalCommand
            {
                public OracleRelationalCommand(
                    IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                    string commandText,
                    IReadOnlyList<IRelationalParameter> parameters)
                    : base(
                        logger,
                        AdjustCommandText(commandText),
                        parameters)
                {
                }

                private static string AdjustCommandText(string commandText)
                {
                    commandText = commandText.Trim();

                    return !commandText.StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase)
                           && !commandText.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase)
                           && !commandText.StartsWith("CREATE OR REPLACE", StringComparison.OrdinalIgnoreCase)
                           && commandText.EndsWith(";", StringComparison.Ordinal)
                        ? commandText.Substring(0, commandText.Length - 1)
                        : commandText;
                }

                protected override object Execute(
                    IRelationalConnection connection,
                    DbCommandMethod executeMethod,
                    IReadOnlyDictionary<string, object> parameterValues)
                {
                    Check.NotNull(connection, nameof(connection));

                    var dbCommand = CreateCommand(connection, parameterValues);

                    connection.Open();

                    var commandId = Guid.NewGuid();
                    var startTime = DateTimeOffset.UtcNow;
                    var stopwatch = Stopwatch.StartNew();

                    Logger.CommandExecuting(
                        dbCommand,
                        executeMethod,
                        commandId,
                        connection.ConnectionId,
                        async: false,
                        startTime: startTime);

                    object result;
                    var readerOpen = false;

                    try
                    {
                        switch (executeMethod)
                        {
                            case DbCommandMethod.ExecuteNonQuery:
                            {
                                result = dbCommand.ExecuteNonQuery();

                                break;
                            }
                            case DbCommandMethod.ExecuteScalar:
                            {
                                result = dbCommand.ExecuteScalar();

                                break;
                            }
                            case DbCommandMethod.ExecuteReader:
                            {
                                result
                                    = new RelationalDataReader(
                                        connection,
                                        dbCommand,
                                        new DbDataReaderDecorator(dbCommand.ExecuteReader()),
                                        commandId,
                                        Logger);

                                readerOpen = true;

                                break;
                            }
                            default:
                            {
                                throw new NotSupportedException();
                            }
                        }

                        Logger.CommandExecuted(
                            dbCommand,
                            executeMethod,
                            commandId,
                            connection.ConnectionId,
                            result,
                            false,
                            startTime,
                            stopwatch.Elapsed);
                    }
                    catch (Exception exception)
                    {
                        Logger.CommandError(
                            dbCommand,
                            executeMethod,
                            commandId,
                            connection.ConnectionId,
                            exception,
                            false,
                            startTime,
                            stopwatch.Elapsed);

                        throw;
                    }
                    finally
                    {
                        if (!readerOpen)
                        {
                            dbCommand.Dispose();
                            connection.Close();
                        }

                        dbCommand.Parameters.Clear();
                    }

                    return result;
                }

                protected override async Task<object> ExecuteAsync(
                    IRelationalConnection connection,
                    DbCommandMethod executeMethod,
                    IReadOnlyDictionary<string, object> parameterValues,
                    CancellationToken cancellationToken = default)
                {
                    Check.NotNull(connection, nameof(connection));

                    var dbCommand = CreateCommand(connection, parameterValues);

                    await connection.OpenAsync(cancellationToken);

                    var commandId = Guid.NewGuid();

                    var startTime = DateTimeOffset.UtcNow;
                    var stopwatch = Stopwatch.StartNew();

                    Logger.CommandExecuting(
                        dbCommand,
                        executeMethod,
                        commandId,
                        connection.ConnectionId,
                        async: true,
                        startTime: startTime);

                    object result;
                    var readerOpen = false;

                    try
                    {
                        switch (executeMethod)
                        {
                            case DbCommandMethod.ExecuteNonQuery:
                            {
                                result = await dbCommand.ExecuteNonQueryAsync(cancellationToken);

                                break;
                            }
                            case DbCommandMethod.ExecuteScalar:
                            {
                                result = await dbCommand.ExecuteScalarAsync(cancellationToken);

                                break;
                            }
                            case DbCommandMethod.ExecuteReader:
                            {
                                result = new RelationalDataReader(
                                    connection,
                                    dbCommand,
                                    new DbDataReaderDecorator(await dbCommand.ExecuteReaderAsync(cancellationToken)),
                                    commandId,
                                    Logger);
                                readerOpen = true;

                                break;
                            }
                            default:
                            {
                                throw new NotSupportedException();
                            }
                        }

                        Logger.CommandExecuted(
                            dbCommand,
                            executeMethod,
                            commandId,
                            connection.ConnectionId,
                            result,
                            true,
                            startTime,
                            stopwatch.Elapsed);
                    }
                    catch (Exception exception)
                    {
                        Logger.CommandError(
                            dbCommand,
                            executeMethod,
                            commandId,
                            connection.ConnectionId,
                            exception,
                            true,
                            startTime,
                            stopwatch.Elapsed);

                        throw;
                    }
                    finally
                    {
                        if (!readerOpen)
                        {
                            dbCommand.Dispose();
                            connection.Close();
                        }

                        dbCommand.Parameters.Clear();
                    }

                    return result;
                }

                private DbCommand CreateCommand(
                    IRelationalConnection connection,
                    IReadOnlyDictionary<string, object> parameterValues)
                {
                    if (parameterValues != null)
                    {
                        parameterValues = AdjustParameters(parameterValues);
                    }

                    var command = connection.DbConnection.CreateCommand();

                    ((OracleCommand)command).BindByName = true;

                    command.CommandText = CommandText;

                    if (connection.CurrentTransaction != null)
                    {
                        command.Transaction = connection.CurrentTransaction.GetDbTransaction();
                    }

                    if (connection.CommandTimeout != null)
                    {
                        command.CommandTimeout = (int)connection.CommandTimeout;
                    }

                    if (Parameters.Count > 0)
                    {
                        if (parameterValues == null)
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.MissingParameterValue(
                                    Parameters[0].InvariantName));
                        }

                        foreach (var parameter in Parameters)
                        {
                            parameter.AddDbParameter(command, parameterValues);
                        }
                    } 

                    return command;
                }

                private static IReadOnlyDictionary<string, object> AdjustParameters(
                    IReadOnlyDictionary<string, object> parameterValues)
                {
                    if (parameterValues.Count == 0)
                    {
                        return parameterValues;
                    }

                    return parameterValues.ToDictionary(
                        kv => kv.Key,
                        kv =>
                            {
                                var type = kv.Value?.GetType();

                                if (type != null)
                                {
                                    type = type.UnwrapNullableType();

                                    if (type == typeof(bool))
                                    {
                                        var b = (bool)kv.Value;

                                        return b ? 1 : 0;
                                    }

                                    if (type == typeof(Guid))
                                    {
                                        var g = (Guid)kv.Value;

                                        return g.ToByteArray();
                                    }

                                    if (type.IsEnum)
                                    {
                                        var underlyingType = Enum.GetUnderlyingType(type);

                                        return Convert.ChangeType(kv.Value, underlyingType);
                                    }

                                    if (type == typeof(DateTimeOffset))
                                    {
                                        var dateTimeOffset = (DateTimeOffset)kv.Value;

                                        return new OracleTimeStampTZ(
                                            dateTimeOffset.DateTime,
                                            dateTimeOffset.Offset.ToString());
                                    }
                                }

                                return kv.Value;
                            });
                }

                private sealed class DbDataReaderDecorator : DbDataReader
                {
                    private readonly DbDataReader _reader;

                    public DbDataReaderDecorator(DbDataReader reader)
                    {
                        _reader = reader;
                    }

                    protected override void Dispose(bool disposing)
                    {
                        _reader.Dispose();

                        base.Dispose(disposing);
                    }

                    public override void Close()
                    {
                        _reader.Close();
                    }

                    public override string GetDataTypeName(int ordinal)
                    {
                        return _reader.GetDataTypeName(ordinal);
                    }

                    public override IEnumerator GetEnumerator()
                    {
                        return _reader.GetEnumerator();
                    }

                    public override Type GetFieldType(int ordinal)
                    {
                        return _reader.GetFieldType(ordinal);
                    }

                    public override string GetName(int ordinal)
                    {
                        return _reader.GetName(ordinal);
                    }

                    public override int GetOrdinal(string name)
                    {
                        return _reader.GetOrdinal(name);
                    }

                    public override DataTable GetSchemaTable()
                    {
                        return _reader.GetSchemaTable();
                    }

                    public override bool GetBoolean(int ordinal)
                    {
                        return _reader.GetInt32(ordinal) == 1;
                    }

                    public override byte GetByte(int ordinal)
                    {
                        return _reader.GetByte(ordinal);
                    }

                    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
                    {
                        return _reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
                    }

                    public override char GetChar(int ordinal)
                    {
                        return _reader.GetChar(ordinal);
                    }

                    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
                    {
                        return _reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
                    }

                    public override DateTime GetDateTime(int ordinal)
                    {
                        return _reader.GetDateTime(ordinal);
                    }

                    public override decimal GetDecimal(int ordinal)
                    {
                        return _reader.GetDecimal(ordinal);
                    }

                    public override double GetDouble(int ordinal)
                    {
                        return _reader.GetDouble(ordinal);
                    }

                    public override float GetFloat(int ordinal)
                    {
                        return _reader.GetFloat(ordinal);
                    }

                    public override Guid GetGuid(int ordinal)
                    {
                        var bytes = new byte[16];
                        _reader.GetBytes(ordinal, 0, bytes, 0, 16);

                        return new Guid(bytes);
                    }

                    public override short GetInt16(int ordinal)
                    {
                        return _reader.GetInt16(ordinal);
                    }

                    public override int GetInt32(int ordinal)
                    {
                        return _reader.GetInt32(ordinal);
                    }

                    public override long GetInt64(int ordinal)
                    {
                        return _reader.GetInt64(ordinal);
                    }

                    public override Type GetProviderSpecificFieldType(int ordinal)
                    {
                        return _reader.GetProviderSpecificFieldType(ordinal);
                    }

                    public override object GetProviderSpecificValue(int ordinal)
                    {
                        return _reader.GetProviderSpecificValue(ordinal);
                    }

                    public override int GetProviderSpecificValues(object[] values)
                    {
                        return _reader.GetProviderSpecificValues(values);
                    }

                    public override string GetString(int ordinal)
                    {
                        return _reader.GetString(ordinal);
                    }

                    public override Stream GetStream(int ordinal)
                    {
                        return _reader.GetStream(ordinal);
                    }

                    public override TextReader GetTextReader(int ordinal)
                    {
                        return _reader.GetTextReader(ordinal);
                    }

                    public override object GetValue(int ordinal)
                    {
                        return _reader.GetValue(ordinal);
                    }

                    public override T GetFieldValue<T>(int ordinal)
                    {
                        if (typeof(T) == typeof(DateTimeOffset))
                        {
                            var oracleTimeStampTz
                                = ((OracleDataReader)_reader).GetOracleTimeStampTZ(ordinal);

                            object dateTimeOffset
                                = new DateTimeOffset(oracleTimeStampTz.Value, oracleTimeStampTz.GetTimeZoneOffset());

                            return (T)dateTimeOffset;
                        }

                        return _reader.GetFieldValue<T>(ordinal);
                    }

                    public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
                    {
                        return _reader.GetFieldValueAsync<T>(ordinal, cancellationToken);
                    }

                    public override int GetValues(object[] values)
                    {
                        return _reader.GetValues(values);
                    }

                    public override bool IsDBNull(int ordinal)
                    {
                        return _reader.IsDBNull(ordinal);
                    }

                    public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
                    {
                        return _reader.IsDBNullAsync(ordinal, cancellationToken);
                    }

                    public override bool NextResult()
                    {
                        return _reader.NextResult();
                    }

                    public override bool Read()
                    {
                        return _reader.Read();
                    }

                    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
                    {
                        return _reader.ReadAsync(cancellationToken);
                    }

                    public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
                    {
                        return _reader.NextResultAsync(cancellationToken);
                    }

                    public override int Depth => _reader.Depth;
                    public override int FieldCount => _reader.FieldCount;
                    public override bool HasRows => _reader.HasRows;
                    public override bool IsClosed => _reader.IsClosed;
                    public override int RecordsAffected => _reader.RecordsAffected;
                    public override int VisibleFieldCount => _reader.VisibleFieldCount;
                    public override object this[int ordinal] => _reader[ordinal];
                    public override object this[string name] => _reader[name];
                }
            }
        }
    }
}
