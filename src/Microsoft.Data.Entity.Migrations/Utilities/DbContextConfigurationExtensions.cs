// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.Migrations.Utilities
{
    internal static class DbContextConfigurationExtensions
    {
        public static Assembly GetMigrationAssembly(this DbContextConfiguration configuration)
        {
            return RelationalOptionsExtension.Extract(configuration).MigrationAssembly
                ?? configuration.Context.GetType().GetTypeInfo().Assembly;
        }

        public static string GetMigrationNamespace(this DbContextConfiguration configuration)
        {
            return RelationalOptionsExtension.Extract(configuration).MigrationNamespace
                ?? configuration.Context.GetType().Namespace + ".Migrations";
        }
    }
}
