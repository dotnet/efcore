// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    public static class MigrationsAssemblyExtensions
    {
        public static Migration GetMigration([NotNull] this IMigrationsAssembly assembly, [NotNull] string nameOrId)
        {
            Check.NotNull(assembly, nameof(assembly));
            Check.NotEmpty(nameOrId, nameof(nameOrId));

            var migration = assembly.FindMigration(nameOrId);
            if (migration == null)
            {
                throw new InvalidOperationException(Strings.MigrationNotFound(nameOrId));
            }

            return migration;
        }
    }
}
