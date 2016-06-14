// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Values that are used as the eventId when logging messages from a relational database provider via <see cref="ILogger" />.
    /// </summary>
    public enum RelationalEventId
    {
        /// <summary>
        ///     A command was executed against the database.
        /// </summary>
        ExecutedCommand = 1,

        /// <summary>
        ///     A database is being created.
        /// </summary>
        CreatingDatabase,

        /// <summary>
        ///     A connection is being opened.
        /// </summary>
        OpeningConnection,

        /// <summary>
        ///     A connection is being closed.
        /// </summary>
        ClosingConnection,

        /// <summary>
        ///     A transaction is beginning.
        /// </summary>
        BeginningTransaction,

        /// <summary>
        ///     A transaction is being committed.
        /// </summary>
        CommittingTransaction,

        /// <summary>
        ///     A transaction is being rolled back.
        /// </summary>
        RollingbackTransaction,

        /// <summary>
        ///     A LINQ query is being executed where some of the query will be evaluated on the client
        ///     (i.e. part of the query can not be translated to SQL).
        /// </summary>
        QueryClientEvaluationWarning,

        /// <summary>
        ///     Two entities were compared for equality in a LINQ query, which may not produce the desired result.
        /// </summary>
        PossibleUnintendedUseOfEqualsWarning,

        /// <summary>
        ///     An ambient transaction is present, which is not fully supported by Entity Framework Core.
        /// </summary>
        AmbientTransactionWarning
    }
}
