// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     Extension methods for <see cref="IMigrationsAssembly" />.
    /// </summary>
    public static class MigrationsAssemblyExtensions
    {
        /// <summary>
        ///     <para>
        ///         Gets a migration identifier in the assembly with the given a full migration name or
        ///         just its identifier.
        ///     </para>
        ///     <para>
        ///         An exception is thrown if the migration was not found--use
        ///         <see cref="IMigrationsAssembly.FindMigrationId" /> if the migration may not exist.
        ///     </para>
        /// </summary>
        /// <param name="assembly"> The assembly. </param>
        /// <param name="nameOrId"> The name or identifier to lookup. </param>
        /// <returns> The identifier of the migration. </returns>
        public static string GetMigrationId(this IMigrationsAssembly assembly, string nameOrId)
        {
            Check.NotNull(assembly, nameof(assembly));
            Check.NotEmpty(nameOrId, nameof(nameOrId));

            var id = assembly.FindMigrationId(nameOrId);
            if (id == null)
            {
                throw new InvalidOperationException(RelationalStrings.MigrationNotFound(nameOrId));
            }

            return id;
        }
    }
}
