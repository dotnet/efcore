// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsSqlServerTest2 : MigrationsTestBase2<MigrationsSqlServerTest2.MigrationsSqlServerFixture2>
    {
        public MigrationsSqlServerTest2(MigrationsSqlServerFixture2 fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class MigrationsSqlServerFixture2 : MigrationsFixtureBase2
        {
            protected override string StoreName { get; } = nameof(MigrationsSqlServerTest2);
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            public override TestHelpers TestHelpers => SqlServerTestHelpers.Instance;

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddScoped<IDatabaseModelFactory, SqlServerDatabaseModelFactory>();
        }
    }
}
