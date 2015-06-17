// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteTestHelpers : TestHelpers
    {
        protected SqliteTestHelpers()
        {
        }

        public new static SqliteTestHelpers Instance { get; } = new SqliteTestHelpers();

        public override EntityFrameworkServicesBuilder AddProviderServices(EntityFrameworkServicesBuilder builder)
        {
            return builder.AddSqlite();
        }

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(new SqlConnection("Database=:memory:"));
        }
    }
}