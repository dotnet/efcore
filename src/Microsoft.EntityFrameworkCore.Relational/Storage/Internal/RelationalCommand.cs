// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class RelationalCommand : IRelationalCommand
    {
        public RelationalCommand(
            [NotNull] ISensitiveDataLogger logger,
            [NotNull] DiagnosticSource diagnosticSource,
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<IRelationalParameter> parameters)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(diagnosticSource, nameof(diagnosticSource));
            Check.NotNull(commandText, nameof(commandText));
            Check.NotNull(parameters, nameof(parameters));

            Logger = logger;
            DiagnosticSource = diagnosticSource;
            CommandText = commandText;
            Parameters = parameters;
        }

        protected virtual ISensitiveDataLogger Logger { get; }
        protected virtual DiagnosticSource DiagnosticSource { get; }

        public virtual string CommandText { get; }

        public virtual IReadOnlyList<IRelationalParameter> Parameters { get; }

        public virtual int ExecuteNonQuery(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues = null,
            bool manageConnection = true)
            => (int)Execute(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteNonQuery),
                parameterValues,
                openConnection: manageConnection,
                closeConnection: manageConnection);

        public virtual Task<int> ExecuteNonQueryAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues = null,
            bool manageConnection = true,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteNonQuery),
                parameterValues,
                openConnection: manageConnection,
                closeConnection: manageConnection,
                cancellationToken: cancellationToken).Cast<object, int>();

        public virtual object ExecuteScalar(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues = null,
            bool manageConnection = true)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteScalar),
                parameterValues,
                openConnection: manageConnection,
                closeConnection: manageConnection);

        public virtual Task<object> ExecuteScalarAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues = null,
            bool manageConnection = true,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteScalar),
                parameterValues,
                openConnection: manageConnection,
                closeConnection: manageConnection,
                cancellationToken: cancellationToken);

        public virtual RelationalDataReader ExecuteReader(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues = null,
            bool manageConnection = true)
            => (RelationalDataReader)Execute(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteReader),
                parameterValues,
                openConnection: manageConnection,
                closeConnection: false);

        public virtual Task<RelationalDataReader> ExecuteReaderAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues = null,
            bool manageConnection = true,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteReader),
                parameterValues,
                openConnection: manageConnection,
                closeConnection: false,
                cancellationToken: cancellationToken).Cast<object, RelationalDataReader>();

        protected virtual object Execute(
            [NotNull] IRelationalConnection connection,
            [NotNull] string executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            bool openConnection,
            bool closeConnection)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(executeMethod, nameof(executeMethod));

            var dbCommand = CreateCommand(connection, parameterValues);

            object result;

            if (openConnection)
            {
                connection.Open();
            }

            var startTimestamp = Stopwatch.GetTimestamp();
            var instanceId = Guid.NewGuid();

            DiagnosticSource.WriteCommandBefore(
                dbCommand,
                executeMethod,
                instanceId,
                startTimestamp,
                async: false);

            try
            {
                switch (executeMethod)
                {
                    case nameof(ExecuteNonQuery):
                    {
                        using (dbCommand)
                        {
                            result = dbCommand.ExecuteNonQuery();
                        }

                        break;
                    }
                    case nameof(ExecuteScalar):
                    {
                        using (dbCommand)
                        {
                            result = dbCommand.ExecuteScalar();
                        }

                        break;
                    }
                    case nameof(ExecuteReader):
                    {
                        try
                        {
                            result
                                = new RelationalDataReader(
                                    openConnection ? connection : null,
                                    dbCommand,
                                    dbCommand.ExecuteReader());
                        }
                        catch
                        {
                            dbCommand.Dispose();

                            throw;
                        }

                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                var currentTimestamp = Stopwatch.GetTimestamp();

                LogCommandExecuted(dbCommand, startTimestamp, currentTimestamp);

                DiagnosticSource.WriteCommandAfter(
                    dbCommand,
                    executeMethod,
                    instanceId,
                    startTimestamp,
                    currentTimestamp);
            }
            catch (Exception exception)
            {
                var currentTimestamp = Stopwatch.GetTimestamp();

                LogCommandExecuted(dbCommand, startTimestamp, currentTimestamp);

                DiagnosticSource.WriteCommandError(
                    dbCommand,
                    executeMethod,
                    instanceId,
                    startTimestamp,
                    currentTimestamp,
                    exception,
                    async: false);

                if (openConnection && !closeConnection)
                {
                    connection.Close();
                }

                throw;
            }
            finally
            {
                if (closeConnection)
                {
                    connection.Close();
                }
            }

            return result;
        }

        protected virtual async Task<object> ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] string executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            bool openConnection,
            bool closeConnection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(executeMethod, nameof(executeMethod));

            var dbCommand = CreateCommand(connection, parameterValues);

            object result;

            if (openConnection)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var startTimestamp = Stopwatch.GetTimestamp();
            var instanceId = Guid.NewGuid();

            DiagnosticSource.WriteCommandBefore(
                dbCommand,
                executeMethod,
                instanceId,
                startTimestamp,
                async: true);

            try
            {
                switch (executeMethod)
                {
                    case nameof(ExecuteNonQuery):
                    {
                        using (dbCommand)
                        {
                            result = await dbCommand.ExecuteNonQueryAsync(cancellationToken);
                        }

                        break;
                    }
                    case nameof(ExecuteScalar):
                    {
                        using (dbCommand)
                        {
                            result = await dbCommand.ExecuteScalarAsync(cancellationToken);
                        }

                        break;
                    }
                    case nameof(ExecuteReader):
                    {
                        try
                        {
                            result
                                = new RelationalDataReader(
                                    openConnection ? connection : null,
                                    dbCommand,
                                    await dbCommand.ExecuteReaderAsync(cancellationToken));
                        }
                        catch
                        {
                            dbCommand.Dispose();

                            throw;
                        }

                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                var currentTimestamp = Stopwatch.GetTimestamp();

                LogCommandExecuted(dbCommand, startTimestamp, currentTimestamp);

                DiagnosticSource.WriteCommandAfter(
                    dbCommand,
                    executeMethod,
                    instanceId,
                    startTimestamp,
                    currentTimestamp,
                    async: true);
            }
            catch (Exception exception)
            {
                var currentTimestamp = Stopwatch.GetTimestamp();

                LogCommandExecuted(dbCommand, startTimestamp, currentTimestamp);

                DiagnosticSource.WriteCommandError(
                    dbCommand,
                    executeMethod,
                    instanceId,
                    startTimestamp,
                    currentTimestamp,
                    exception,
                    async: true);

                if (openConnection && !closeConnection)
                {
                    connection.Close();
                }

                throw;
            }
            finally
            {
                if (closeConnection)
                {
                    connection.Close();
                }
            }

            return result;
        }

        private DbCommand CreateCommand(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
        {
            var command = connection.DbConnection.CreateCommand();

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
                    object parameterValue;

                    if (parameterValues.TryGetValue(parameter.InvariantName, out parameterValue))
                    {
                        parameter.AddDbParameter(command, parameterValue);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.MissingParameterValue(parameter.InvariantName));
                    }
                }
            }

            return command;
        }

        private void LogCommandExecuted(
            DbCommand command,
            long startTimestamp,
            long currentTimestamp)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                var logParameterValues
                    = command.Parameters.Count > 0
                      && Logger.LogSensitiveData;

                var logData = new DbCommandLogData(
                    command.CommandText.TrimEnd(),
                    command.CommandType,
                    command.CommandTimeout,
                    command.Parameters
                        .Cast<DbParameter>()
                        .ToDictionary(p => p.ParameterName, p => logParameterValues ? p.Value : "?"),
                    DeriveTimespan(startTimestamp, currentTimestamp));

                Logger.Log(
                    LogLevel.Information,
                    (int)RelationalLoggingEventId.ExecutedCommand,
                    logData,
                    null,
                    (state, _) =>
                        {
                            var elapsedMilliseconds = DeriveTimespan(startTimestamp, currentTimestamp);

                            return RelationalStrings.RelationalLoggerExecutedCommand(
                                string.Format($"{elapsedMilliseconds:N0}"),
                                state.Parameters
                                    .Select(kv => $"{kv.Key}='{FormatParameterValue(kv.Value)}'")
                                    .Join(),
                                state.CommandType,
                                state.CommandTimeout,
                                Environment.NewLine,
                                state.CommandText);
                        });
            }
        }

        private static long DeriveTimespan(long startTimestamp, long currentTimestamp)
            => (currentTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;

        internal static object FormatParameterValue(object parameterValue)
        {
            if (parameterValue.GetType() != typeof(byte[]))
            {
                return Convert.ToString(parameterValue, CultureInfo.InvariantCulture);
            }
            var stringValueBuilder = new StringBuilder();
            var buffer = (byte[])parameterValue;
            stringValueBuilder.Append("0x");

            for (var i = 0; i < buffer.Length; i++)
            {
                if (i > 31)
                {
                    stringValueBuilder.Append("...");
                    break;
                }
                stringValueBuilder.Append(buffer[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return stringValueBuilder.ToString();
        }
    }
}
