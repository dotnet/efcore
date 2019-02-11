// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         The main service used to generated an EF Core Migrations script or
    ///         migrate a database directly.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IMigrator
    {
        /// <summary>
        ///     Migrates the database to either a specified target migration or up to the latest
        ///     migration that exists in the <see cref="IMigrationsAssembly" />.
        /// </summary>
        /// <param name="targetMigration">
        ///     The target migration to migrate the database to, or <c>null</c> to migrate to the latest.
        /// </param>
        void Migrate([CanBeNull] string targetMigration = null);

        /// <summary>
        ///     Migrates the database to either a specified target migration or up to the latest
        ///     migration that exists in the <see cref="IMigrationsAssembly" />.
        /// </summary>
        /// <param name="targetMigration">
        ///     The target migration to migrate the database to, or <c>null</c> to migrate to the latest.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> A task that represents the asynchronous operation </returns>
        Task MigrateAsync(
            [CanBeNull] string targetMigration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Generates a SQL script to migrate a database either in its entirety, or starting and
        ///     ending at specified migrations.
        /// </summary>
        /// <param name="fromMigration">
        ///     The migration to start from, or <c>null</c> to start from the empty database.
        /// </param>
        /// <param name="toMigration">
        ///     The target migration to migrate the database to, or <c>null</c> to migrate to the latest.
        /// </param>
        /// <param name="idempotent">
        ///     If <c>true</c>, then idempotent scripts will be generated, otherwise
        ///     scripts will be generated that assume none of the migrations in the range specified have
        ///     already been applied to the database.
        /// </param>
        /// <returns> The generated script. </returns>
        string GenerateScript([CanBeNull] string fromMigration = null, [CanBeNull] string toMigration = null, bool idempotent = false);
    }
}
