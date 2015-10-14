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

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RelationalCommand : IRelationalCommand
    {
        public RelationalCommand(
            [NotNull] ISensitiveDataLogger logger,
            [NotNull] DiagnosticSource diagnosticSource,
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<RelationalParameter> parameters)
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

        public virtual IReadOnlyList<RelationalParameter> Parameters { get; }


        public virtual void ExecuteNonQuery(
            [NotNull] IRelationalConnection connection,
            bool manageConnection = true)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                (cmd, con) =>
                {
                    using (cmd)
                    {
                        return cmd.ExecuteNonQuery();
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteNonQuery,
                openConnection: manageConnection,
                closeConnection: manageConnection);

        public virtual async Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken),
            bool manageConnection = true)
            => await ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                async (cmd, con, ct) =>
                {
                    using (cmd)
                    {
                        return await cmd.ExecuteNonQueryAsync(ct);
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteNonQuery,
                openConnection: manageConnection,
                closeConnection: manageConnection,
                cancellationToken: cancellationToken);

        public virtual object ExecuteScalar(
            [NotNull] IRelationalConnection connection,
            bool manageConnection = true)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                (cmd, con) =>
                {
                    using (cmd)
                    {
                        return cmd.ExecuteScalar();
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteScalar,
                openConnection: manageConnection,
                closeConnection: manageConnection);

        public virtual async Task<object> ExecuteScalarAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken),
            bool manageConnection = true)
            => await ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                async (cmd, con, ct) =>
                {
                    using (cmd)
                    {
                        return await cmd.ExecuteScalarAsync(ct);
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteScalar,
                openConnection: manageConnection,
                closeConnection: manageConnection,
                cancellationToken: cancellationToken);

        public virtual RelationalDataReader ExecuteReader(
            [NotNull] IRelationalConnection connection,
            bool manageConnection = true)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                (cmd, con) =>
                {
                    try
                    {
                        return new RelationalDataReader(
                            manageConnection
                                ? connection
                                : null,
                            cmd,
                            cmd.ExecuteReader());
                    }
                    catch
                    {
                        cmd.Dispose();
                        throw;
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteReader,
                openConnection: manageConnection,
                closeConnection: false);

        public virtual async Task<RelationalDataReader> ExecuteReaderAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken),
            bool manageConnection = true)
            => await ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                async (cmd, con, ct) =>
                {
                    try
                    {
                        return new RelationalDataReader(
                            manageConnection
                                ? con
                                : null,
                            cmd,
                            await cmd.ExecuteReaderAsync(ct));
                    }
                    catch
                    {
                        cmd.Dispose();
                        throw;
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteReader,
                openConnection: manageConnection,
                closeConnection: false,
                cancellationToken: cancellationToken);

        protected virtual T Execute<T>(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<DbCommand, IRelationalConnection, T> action,
            [NotNull] string executeMethod,
            bool openConnection,
            bool closeConnection)
        {
            var dbCommand = CreateCommand(connection);

            WriteDiagnostic(
                RelationalDiagnostics.BeforeExecuteCommand,
                dbCommand,
                executeMethod);

            T result;

            if(openConnection)
            {
                connection.Open();
            }

            try
            {
                Logger.LogCommand(dbCommand);

                result = action(dbCommand, connection);
            }
            catch (Exception exception)
            {
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
                if(closeConnection)
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

        protected virtual async Task<T> ExecuteAsync<T>(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<DbCommand, IRelationalConnection, CancellationToken, Task<T>> action,
            [NotNull] string executeMethod,
            bool openConnection,
            bool closeConnection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbCommand = CreateCommand(connection);

            WriteDiagnostic(
                RelationalDiagnostics.BeforeExecuteCommand,
                dbCommand,
                executeMethod,
                async: true);

            T result;

            if (openConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                Logger.LogCommand(dbCommand);

                result = await action(dbCommand, connection, cancellationToken);
            }
            catch (Exception exception)
            {
                DiagnosticSource
                    .WriteCommandError(
                        dbCommand,
                        executeMethod,
                        async: true,
                        exception: exception);

                if(openConnection && !closeConnection)
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

        private DbCommand CreateCommand(IRelationalConnection connection)
        {
            var command = connection.DbConnection.CreateCommand();
            command.CommandText = CommandText;

            if (connection.Transaction != null)
            {
                command.Transaction = connection.Transaction.GetService();
            }

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int)connection.CommandTimeout;
            }

            foreach (var parameter in Parameters)
            {
                command.Parameters.Add(
                    parameter.RelationalTypeMapping.CreateParameter(
                        command,
                        parameter.Name,
                        parameter.Value,
                        parameter.Nullable));
            }

            return command;
        }
    }
}
