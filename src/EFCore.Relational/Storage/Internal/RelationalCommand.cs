// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalCommand : IRelationalCommand
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalCommand(
            [NotNull] IDiagnosticsLogger<LoggerCategory.Database.Sql> sqlLogger,
            [NotNull] IDiagnosticsLogger<LoggerCategory.Database.DataReader> readerLogger,
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<IRelationalParameter> parameters)
        {
            Check.NotNull(sqlLogger, nameof(sqlLogger));
            Check.NotNull(readerLogger, nameof(readerLogger));
            Check.NotNull(commandText, nameof(commandText));
            Check.NotNull(parameters, nameof(parameters));

            SqlLogger = sqlLogger;
            ReaderLogger = readerLogger;
            CommandText = commandText;
            Parameters = parameters;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IDiagnosticsLogger<LoggerCategory.Database.Sql> SqlLogger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IDiagnosticsLogger<LoggerCategory.Database.DataReader> ReaderLogger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string CommandText { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IRelationalParameter> Parameters { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int ExecuteNonQuery(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
            => (int)Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteNonQuery,
                parameterValues);

        int IRelationalCommand.ExecuteNonQuery(
            IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection)
            => ExecuteNonQuery(connection, parameterValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<int> ExecuteNonQueryAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteNonQuery,
                parameterValues,
                cancellationToken: cancellationToken).Cast<object, int>();

        Task<int> IRelationalCommand.ExecuteNonQueryAsync(
            IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection, CancellationToken cancellationToken)
            => ExecuteNonQueryAsync(connection, parameterValues, cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object ExecuteScalar(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteScalar,
                parameterValues);

        object IRelationalCommand.ExecuteScalar(IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection)
            => ExecuteScalar(connection, parameterValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<object> ExecuteScalarAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteScalar,
                parameterValues,
                cancellationToken: cancellationToken);

        Task<object> IRelationalCommand.ExecuteScalarAsync(
            IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection, CancellationToken cancellationToken)
            => ExecuteScalarAsync(connection, parameterValues, cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual RelationalDataReader ExecuteReader(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
            => (RelationalDataReader)Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteReader,
                parameterValues,
                closeConnection: false);

        RelationalDataReader IRelationalCommand.ExecuteReader(
            IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection)
            => ExecuteReader(connection, parameterValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<RelationalDataReader> ExecuteReaderAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteReader,
                parameterValues,
                closeConnection: false,
                cancellationToken: cancellationToken).Cast<object, RelationalDataReader>();

        Task<RelationalDataReader> IRelationalCommand.ExecuteReaderAsync(
            IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues, bool manageConnection, CancellationToken cancellationToken)
            => ExecuteReaderAsync(connection, parameterValues, cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual object Execute(
            [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            bool closeConnection = true)
        {
            Check.NotNull(connection, nameof(connection));

            var dbCommand = CreateCommand(connection, parameterValues);

            connection.Open();

            var startTimestamp = Stopwatch.GetTimestamp();
            var commandId = Guid.NewGuid();

            SqlLogger.CommandExecuting(
                dbCommand,
                executeMethod,
                commandId,
                connection.ConnectionId,
                async: false, 
                startTimestamp: startTimestamp);

            object result;
            try
            {
                switch (executeMethod)
                {
                    case DbCommandMethod.ExecuteNonQuery:
                    {
                        using (dbCommand)
                        {
                            result = dbCommand.ExecuteNonQuery();
                        }

                        break;
                    }
                    case DbCommandMethod.ExecuteScalar:
                    {
                        using (dbCommand)
                        {
                            result = dbCommand.ExecuteScalar();
                        }

                        break;
                    }
                    case DbCommandMethod.ExecuteReader:
                    {
                        try
                        {
                            result
                                = new RelationalDataReader(
                                    connection,
                                    dbCommand,
                                    dbCommand.ExecuteReader(),
                                    commandId,
                                    ReaderLogger);
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

                SqlLogger.CommandExecuted(
                    dbCommand,
                    executeMethod,
                    commandId,
                    connection.ConnectionId,
                    result,
                    false,
                    startTimestamp,
                    currentTimestamp);

                if (closeConnection)
                {
                    connection.Close();
                }
            }
            catch (Exception exception)
            {
                var currentTimestamp = Stopwatch.GetTimestamp();

                SqlLogger.CommandError(
                    dbCommand,
                    executeMethod,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    false,
                    startTimestamp,
                    currentTimestamp);

                connection.Close();

                throw;
            }
            finally
            {
                dbCommand.Parameters.Clear();
            }

            return result;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual async Task<object> ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            bool closeConnection = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));

            var dbCommand = CreateCommand(connection, parameterValues);

            await connection.OpenAsync(cancellationToken);

            var startTimestamp = Stopwatch.GetTimestamp();
            var commandId = Guid.NewGuid();

            SqlLogger.CommandExecuting(
                dbCommand,
                executeMethod,
                commandId,
                connection.ConnectionId,
                async: true, 
                startTimestamp: startTimestamp);

            object result;
            try
            {
                switch (executeMethod)
                {
                    case DbCommandMethod.ExecuteNonQuery:
                    {
                        using (dbCommand)
                        {
                            result = await dbCommand.ExecuteNonQueryAsync(cancellationToken);
                        }

                        break;
                    }
                    case DbCommandMethod.ExecuteScalar:
                    {
                        using (dbCommand)
                        {
                            result = await dbCommand.ExecuteScalarAsync(cancellationToken);
                        }

                        break;
                    }
                    case DbCommandMethod.ExecuteReader:
                    {
                        try
                        {
                            result = new RelationalDataReader(
                                connection,
                                dbCommand,
                                await dbCommand.ExecuteReaderAsync(cancellationToken),
                                commandId,
                                ReaderLogger);
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

                SqlLogger.CommandExecuted(
                    dbCommand,
                    executeMethod,
                    commandId,
                    connection.ConnectionId,
                    result,
                    true,
                    startTimestamp,
                    currentTimestamp);

                if (closeConnection)
                {
                    connection.Close();
                }
            }
            catch (Exception exception)
            {
                var currentTimestamp = Stopwatch.GetTimestamp();

                SqlLogger.CommandError(
                    dbCommand,
                    executeMethod,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    true,
                    startTimestamp,
                    currentTimestamp);

                connection.Close();

                throw;
            }
            finally
            {
                dbCommand.Parameters.Clear();
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
                    if (parameterValues.TryGetValue(parameter.InvariantName, out object parameterValue))
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
    }
}
