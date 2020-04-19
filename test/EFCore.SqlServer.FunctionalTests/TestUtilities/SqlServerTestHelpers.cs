// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqlServerTestHelpers : TestHelpers
    {
        protected SqlServerTestHelpers()
        {
        }

        public static SqlServerTestHelpers Instance { get; } = new SqlServerTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkSqlServer();

        public override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(new SqlConnection("Database=DummyDatabase"));

        public override LoggingDefinitions LoggingDefinitions { get; } = new SqlServerLoggingDefinitions();
    }
}
