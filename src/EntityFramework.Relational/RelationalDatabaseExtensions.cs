// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalDatabaseExtensions
    {
        public static void ApplyMigrations([NotNull] this Database database)
            => Check.NotNull(database, nameof(database)).GetService<IMigrator>().ApplyMigrations();

        public static DbConnection GetDbConnection([NotNull] this Database database)
            => Check.NotNull(database, nameof(database)).GetService<IRelationalConnection>().DbConnection;

        // TODO: Remove this following further refactoring
        public static IRelationalConnection GetRelationalConnection([NotNull] this Database database)
            => Check.NotNull(database, nameof(database)).GetService<IRelationalConnection>();
    }
}
