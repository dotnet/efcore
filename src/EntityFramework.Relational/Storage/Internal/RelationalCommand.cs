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


        public virtual void ExecuteNonQuery([NotNull] IRelationalConnection connection)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                c =>
                {
                    using (c)
                    {
                        return c.ExecuteNonQuery();
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteNonQuery);

        public virtual async Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
            => await ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                async (c, ct) =>
                {
                    using (c)
                    {
                        return await c.ExecuteNonQueryAsync(ct);
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteNonQuery,
                cancellationToken);

        public virtual object ExecuteScalar([NotNull] IRelationalConnection connection)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                c =>
                {
                    using (c)
                    {
                        return c.ExecuteScalar();
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteScalar);

        public virtual async Task<object> ExecuteScalarAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
            => await ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                async (c, ct) =>
                {
                    using (c)
                    {
                        return await c.ExecuteScalarAsync(ct);
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteScalar,
                cancellationToken);

        public virtual RelationalDataReader ExecuteReader([NotNull] IRelationalConnection connection)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                c =>
                {
                    try
                    {
                        return new RelationalDataReader(c, c.ExecuteReader());
                    }
                    catch
                    {
                        c.Dispose();
                        throw;
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteReader);

        public virtual async Task<RelationalDataReader> ExecuteReaderAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
            => await ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                async (c, ct) =>
                {
                    try
                    {
                        return new RelationalDataReader(c, await c.ExecuteReaderAsync(ct));
                    }
                    catch
                    {
                        c.Dispose();
                        throw;
                    }
                },
                RelationalDiagnostics.ExecuteMethod.ExecuteReader,
                cancellationToken);

        protected virtual T Execute<T>(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<DbCommand, T> action,
            [NotNull] string executeMethod)
        {
            var dbCommand = CreateCommand(connection);

            WriteDiagnostic(
                RelationalDiagnostics.BeforeExecuteCommand,
                dbCommand,
                executeMethod);

            T result;

            try
            {
                result = action(dbCommand);
            }
            catch (Exception exception)
            {
                DiagnosticSource
                    .WriteCommandError(
                        dbCommand,
                        executeMethod,
                        async: false,
                        exception: exception);

                throw;
            }

            WriteDiagnostic(
                RelationalDiagnostics.AfterExecuteCommand,
                dbCommand,
                executeMethod);

            return result;
        }

        protected virtual async Task<T> ExecuteAsync<T>(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<DbCommand, CancellationToken, Task<T>> action,
            [NotNull] string executeMethod,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbCommand = CreateCommand(connection);

            WriteDiagnostic(
                RelationalDiagnostics.BeforeExecuteCommand,
                dbCommand,
                executeMethod,
                async: true);

            T result;

            try
            {
                result = await action(dbCommand, cancellationToken);
            }
            catch (Exception exception)
            {
                DiagnosticSource
                    .WriteCommandError(
                        dbCommand,
                        executeMethod,
                        async: true,
                        exception: exception);

                throw;
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

        public virtual DbCommand CreateCommand([NotNull] IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

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

            Logger.LogCommand(command);

            return command;
        }
    }
}
