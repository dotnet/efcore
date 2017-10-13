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
                                        dbCommand.ExecuteReader(),
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
                                    await dbCommand.ExecuteReaderAsync(cancellationToken),
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

                    // HACK: Need to make it easier to add this in update pipeline.
                    if (command.CommandText.Contains(":cur"))
                    {
                        command.Parameters.Add(
                            new OracleParameter(
                                "cur",
                                OracleDbType.RefCursor,
                                DBNull.Value,
                                ParameterDirection.Output));
                    }

                    return command;
                }
            }
        }
    }
}
