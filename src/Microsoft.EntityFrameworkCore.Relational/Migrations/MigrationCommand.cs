// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class MigrationCommand
    {
        private readonly IRelationalCommand _relationalCommand;

        public MigrationCommand(
            [NotNull] IRelationalCommand relationalCommand,
            bool transactionSuppressed = false)
        {
            Check.NotNull(relationalCommand, nameof(relationalCommand));

            _relationalCommand = relationalCommand;
            TransactionSuppressed = transactionSuppressed;
        }

        public virtual bool TransactionSuppressed { get; }

        public virtual string CommandText => _relationalCommand.CommandText;

        public virtual int ExecuteNonQuery(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues = null)
            => _relationalCommand.ExecuteNonQuery(
                Check.NotNull(connection, nameof(connection)),
                parameterValues);

        public virtual async Task<int> ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues = null,
            CancellationToken cancellationToken = default(CancellationToken))
            => await _relationalCommand.ExecuteNonQueryAsync(
                Check.NotNull(connection, nameof(connection)),
                parameterValues,
                cancellationToken);
    }
}
