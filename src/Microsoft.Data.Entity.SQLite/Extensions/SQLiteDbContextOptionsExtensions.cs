// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.SQLite;
using Microsoft.Data.Entity.SQLite.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity
{
    public static class SQLiteDbContextOptionsExtensions
    {
        public static DbContextOptions UseSQLite(
            [NotNull] this DbContextOptions options,
            [NotNull] string connectionString)
        {
            Check.NotNull(options, "options");
            Check.NotEmpty(connectionString, "connectionString");

            options.AddBuildAction(
                c => c.AddOrUpdateExtension<SQLiteConfigurationExtension>(x => x.ConnectionString = connectionString));

            return options;
        }
    }
}
