// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.Migrations.Utilities
{
    public static class DbContextConfigurationExtensions
    {
        public static Assembly GetMigrationAssembly([NotNull] this DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return RelationalOptionsExtension.Extract(configuration).MigrationAssembly
                   ?? configuration.Context.GetType().GetTypeInfo().Assembly;
        }

        public static string GetMigrationNamespace([NotNull] this DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return RelationalOptionsExtension.Extract(configuration).MigrationNamespace;
        }
    }
}
