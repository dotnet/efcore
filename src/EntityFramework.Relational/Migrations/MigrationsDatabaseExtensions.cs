// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
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
