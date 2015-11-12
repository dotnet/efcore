// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Storage.Internal
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
            bool manageConnection = true)
            => (int)Execute(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteNonQuery),
                openConnection: manageConnection,
                closeConnection: manageConnection);

        public virtual Task<int> ExecuteNonQueryAsync(
            IRelationalConnection connection,
            bool manageConnection = true,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteNonQuery),
                openConnection: manageConnection,
                closeConnection: manageConnection,
                cancellationToken: cancellationToken).Cast<object, int>();

        public virtual object ExecuteScalar(
            IRelationalConnection connection,
            bool manageConnection = true)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteScalar),
                openConnection: manageConnection,
                closeConnection: manageConnection);

        public virtual Task<object> ExecuteScalarAsync(
            IRelationalConnection connection,
            bool manageConnection = true,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteScalar),
                openConnection: manageConnection,
                closeConnection: manageConnection,
                cancellationToken: cancellationToken);

        public virtual RelationalDataReader ExecuteReader(
            IRelationalConnection connection,
            bool manageConnection = true,
            IReadOnlyDictionary<string, object> parameterValues = null)
            => (RelationalDataReader)Execute(
                Check.NotNull(connection, nameof(connection)),
                nameof(ExecuteReader),
                openConnection: manageConnection,
                closeConnection: false,
                parameterValues: parameterValues);

        public virtual Task<RelationalDataReader> ExecuteReaderAsync(
            IRelationalConnection connection,
            bool manageConnection = true,
            IReadOnlyDictionary<string, object> parameterValues = null,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(
                    Check.NotNull(connection, nameof(connection)),
                    nameof(ExecuteReader),
                    openConnection: manageConnection,
                    closeConnection: false,
                    cancellationToken: cancellationToken,
                    parameterValues: parameterValues).Cast<object, RelationalDataReader>();

        protected virtual object Execute(
            [NotNull] IRelationalConnection connection,
            [NotNull] string executeMethod,
            bool openConnection,
            bool closeConnection,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues = null)
        {
            var dbCommand = CreateCommand(connection, parameterValues);

            WriteDiagnostic(
                RelationalDiagnostics.BeforeExecuteCommand,
                dbCommand,
                executeMethod);

            object result;

            if (openConnection)
            {
                connection.Open();
            }

            Stopwatch stopwatch = null;

            try
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    stopwatch = Stopwatch.StartNew();
                }

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

                stopwatch?.Stop();

                Logger.LogCommandExecuted(dbCommand, stopwatch?.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                stopwatch?.Stop();

                Logger.LogCommandExecuted(dbCommand, stopwatch?.ElapsedMilliseconds);

                DiagnosticSource
                    .WriteCommandError(
                        dbCommand,
                        executeMethod,
                        async: false,
                        exception: exception);

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

            WriteDiagnostic(
                RelationalDiagnostics.AfterExecuteCommand,
                dbCommand,
                executeMethod);

            return result;
        }

        protected virtual async Task<object> ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] string executeMethod,
            bool openConnection,
            bool closeConnection,
            CancellationToken cancellationToken = default(CancellationToken),
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues = null)
        {
            var dbCommand = CreateCommand(connection, parameterValues);

            WriteDiagnostic(
                RelationalDiagnostics.BeforeExecuteCommand,
                dbCommand,
                executeMethod,
                async: true);

            object result;

            if (openConnection)
            {
                await connection.OpenAsync(cancellationToken);
            }

            Stopwatch stopwatch = null;

            try
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    stopwatch = Stopwatch.StartNew();
                }

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

                stopwatch?.Stop();

                Logger.LogCommandExecuted(dbCommand, stopwatch?.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                stopwatch?.Stop();

                Logger.LogCommandExecuted(dbCommand, stopwatch?.ElapsedMilliseconds);

                DiagnosticSource
                    .WriteCommandError(
                        dbCommand,
                        executeMethod,
                        async: true,
                        exception: exception);

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

            WriteDiagnostic(
                RelationalDiagnostics.AfterExecuteCommand,
                dbCommand,
                executeMethod,
                async: true);

            return result;
        }

        private void WriteDiagnostic(
            string name,
            DbCommand command,
            string executeMethod,
            bool async = false)
            => DiagnosticSource.WriteCommand(
                name,
                command,
                executeMethod,
                async: async);

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

            foreach (var parameter in Parameters)
            {
                parameter.AddDbParameter(
                    command,
                    parameterValues?.Count > 0
                        ? parameterValues[parameter.InvariantName]
                        : null);
            }

            return command;
        }
    }
}
