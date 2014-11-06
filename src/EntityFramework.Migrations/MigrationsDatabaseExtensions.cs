// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    public static class MigrationsDatabaseExtensions
    {
        public static MigrationsEnabledDatabase AsMigrationsEnabled([NotNull] this Database database)
        {
            Check.NotNull(database, "database");

            var migrationsEnabledDatabase = database as MigrationsEnabledDatabase;

            if (migrationsEnabledDatabase == null)
            {
                throw new InvalidOperationException(Strings.MigrationsNotInUse);
            }

            return migrationsEnabledDatabase;
        }
    }
}
