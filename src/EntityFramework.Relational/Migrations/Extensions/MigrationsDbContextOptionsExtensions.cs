// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class MigrationsDbContextOptionsExtensions
    {
        public static DbContextOptions UseMigrationAssembly(
            [NotNull] this DbContextOptions options, [NotNull] Assembly migrationAssembly)
        {
            Check.NotNull(options, "options");
            Check.NotNull(migrationAssembly, "migrationAssembly");

            ((IDbContextOptions)options).AddOrUpdateExtension<MigrationsOptionsExtension>(
                x => x.MigrationAssembly = migrationAssembly);

            return options;
        }

        public static DbContextOptions<T> UseMigrationAssembly<T>(
            [NotNull] this DbContextOptions<T> options, [NotNull] Assembly migrationAssembly)
        {
            return (DbContextOptions<T>)UseMigrationAssembly((DbContextOptions)options, migrationAssembly);
        }
    }
}
