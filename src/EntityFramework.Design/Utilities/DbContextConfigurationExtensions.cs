// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.Design.Utilities
{
    internal static class DbContextConfigurationExtensions
    {
        public static string GetMigrationNamespace(this DbContextConfiguration configuration)
        {
            return RelationalOptionsExtension.Extract(configuration).MigrationNamespace
                   ?? configuration.Context.GetType().Namespace + "." + configuration.Context.GetType().Name + "Migrations";
        }
    }
}
