// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerNullKeysTest : NullKeysTestBase<SqlServerNullKeysTest.SqlServerNullKeysFixture>
    {
        public SqlServerNullKeysTest(SqlServerNullKeysFixture fixture)
            : base(fixture)
        {
        }

        public class SqlServerNullKeysFixture : NullKeysFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly DbContextOptions _options;

            public SqlServerNullKeysFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection
                    .AddSingleton(typeof(SqlServerModelSource), p => new TestSqlServerModelSource(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptions();
                _options.UseSqlServer(SqlServerTestStore.CreateConnectionString("StringsContext"));

                EnsureCreated();
            }

            public override DbContext CreateContext()
            {
                return new DbContext(_serviceProvider, _options);
            }

            public override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.ForSqlServer().UseSequence();
            }
        }
    }
}
