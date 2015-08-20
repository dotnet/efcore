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
        public static string GetMigrationId([NotNull] this IMigrationsAssembly assembly, [NotNull] string nameOrId)
        {
            Check.NotNull(assembly, nameof(assembly));
            Check.NotEmpty(nameOrId, nameof(nameOrId));

            var id = assembly.FindMigrationId(nameOrId);
            if (id == null)
            {
                throw new InvalidOperationException(Strings.MigrationNotFound(nameOrId));
            }

            return id;
        }
    }
}
