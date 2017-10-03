// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The main interaction point between a context and the database provider.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalDatabase : Database
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDatabase" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for the base of this service. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this service. </param>
        public RelationalDatabase(
            [NotNull] DatabaseDependencies dependencies,
            [NotNull] RelationalDatabaseDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Parameter object containing relational dependencies for this service.
        /// </summary>
        protected virtual RelationalDatabaseDependencies RelationalDependencies { get; }

        /// <summary>
        ///     Persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries"> Entries representing the changes to be persisted. </param>
        /// <returns> The number of state entries persisted to the database. </returns>
        public override int SaveChanges(
            IReadOnlyList<IUpdateEntry> entries)
            => RelationalDependencies.BatchExecutor.Execute(
                RelationalDependencies.BatchPreparer.BatchCommands(
                    Check.NotNull(entries, nameof(entries))),
                RelationalDependencies.Connection);

        /// <summary>
        ///     Asynchronously persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries"> Entries representing the changes to be persisted. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of entries persisted to the database.
        /// </returns>
        public override Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default)
            => RelationalDependencies.BatchExecutor.ExecuteAsync(
                RelationalDependencies.BatchPreparer.BatchCommands(
                    Check.NotNull(entries, nameof(entries))),
                RelationalDependencies.Connection,
                cancellationToken);
    }
}
