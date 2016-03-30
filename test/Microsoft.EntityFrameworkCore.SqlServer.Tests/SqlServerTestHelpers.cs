// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
{
    public class SqlServerTestHelpers : RelationalTestHelpers
    {
        protected SqlServerTestHelpers()
        {
        }

        public new static SqlServerTestHelpers Instance { get; } = new SqlServerTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkSqlServer();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(new SqlConnection("Database=DummyDatabase"));
    }
}
