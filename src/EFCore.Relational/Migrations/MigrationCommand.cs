// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     Represents a command ready to be sent to the database to migrate it.
    /// </summary>
    public class MigrationCommand
    {
        private readonly IRelationalCommand _relationalCommand;
        private readonly DbContext _context;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _logger;

        /// <summary>
        ///     Creates a new instance of the command.
        /// </summary>
        /// <param name="relationalCommand"> The underlying <see cref="IRelationalCommand" /> that will be used to execute the command. </param>
        /// <param name="context"> The current <see cref="DbContext"/> or null if not known. </param>
        /// <param name="logger"> The command logger. </param>
        /// <param name="transactionSuppressed"> Indicates whether or not transactions should be suppressed while executing the command. </param>
        public MigrationCommand(
            [NotNull] IRelationalCommand relationalCommand,
            [CanBeNull] DbContext context,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            bool transactionSuppressed = false)
        {
            Check.NotNull(relationalCommand, nameof(relationalCommand));

            _relationalCommand = relationalCommand;
            _context = context;
            _logger = logger;
            TransactionSuppressed = transactionSuppressed;
        }

        /// <summary>
        ///     Indicates whether or not transactions should be suppressed while executing the command.
        /// </summary>
        public virtual bool TransactionSuppressed { get; }

        /// <summary>
        ///     The SQL command text that will be executed against the database.
        /// </summary>
        public virtual string CommandText => _relationalCommand.CommandText;

        /// <summary>
        ///     Executes the command and returns the number of rows affected.
        /// </summary>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="parameterValues"> The values for the parameters, or <c>null</c> if the command has no parameters. </param>
        /// <returns> The number of rows affected. </returns>
        public virtual int ExecuteNonQuery(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues = null)
            => _relationalCommand.ExecuteNonQuery(
                new RelationalCommandParameterObject(
                    connection,
                    parameterValues,
                    _context,
                    _logger));

        /// <summary>
        ///     Executes the command and returns the number of rows affected.
        /// </summary>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="parameterValues"> The values for the parameters, or <c>null</c> if the command has no parameters. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> A task that represents the asynchronous operation. The task result contains the number of rows affected.  </returns>
        public virtual Task<int> ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues = null,
            CancellationToken cancellationToken = default)
            => _relationalCommand.ExecuteNonQueryAsync(
                new RelationalCommandParameterObject(
                    connection,
                    parameterValues,
                    _context,
                    _logger),
                cancellationToken);
    }
}
