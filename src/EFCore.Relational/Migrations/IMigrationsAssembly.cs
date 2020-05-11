// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         A service representing an assembly containing EF Core Migrations.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IMigrationsAssembly
    {
        /// <summary>
        ///     A dictionary mapping migration identifiers to the <see cref="TypeInfo" /> of the class
        ///     that represents the migration.
        /// </summary>
        IReadOnlyDictionary<string, TypeInfo> Migrations { get; }

        /// <summary>
        ///     The snapshot of the <see cref="IModel" /> contained in the assembly.
        /// </summary>
        ModelSnapshot ModelSnapshot { get; }

        /// <summary>
        ///     The assembly that contains the migrations, snapshot, etc.
        /// </summary>
        Assembly Assembly { get; }

        /// <summary>
        ///     Finds a migration identifier in the assembly with the given a full migration name or
        ///     just its identifier.
        /// </summary>
        /// <param name="nameOrId"> The name or identifier to lookup. </param>
        /// <returns> The identifier of the migration, or <see langword="null" /> if none was found. </returns>
        string FindMigrationId([NotNull] string nameOrId);

        /// <summary>
        ///     Creates an instance of the migration class.
        /// </summary>
        /// <param name="migrationClass">
        ///     The <see cref="TypeInfo" /> for the migration class, as obtained from the <see cref="Migrations" /> dictionary.
        /// </param>
        /// <param name="activeProvider"> The name of the current database provider. </param>
        /// <returns> The migration instance. </returns>
        Migration CreateMigration([NotNull] TypeInfo migrationClass, [NotNull] string activeProvider);
    }
}
