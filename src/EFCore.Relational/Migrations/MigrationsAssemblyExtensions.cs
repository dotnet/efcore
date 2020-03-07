// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
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
        public static string GetMigrationId([NotNull] this IMigrationsAssembly assembly, [NotNull] string nameOrId)
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
