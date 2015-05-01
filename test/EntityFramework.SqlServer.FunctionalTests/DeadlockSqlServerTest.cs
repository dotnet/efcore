// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if NET45

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class DeadlockSqlServerTest : DeadlockTestBase<SqlServerTestStore>, IClassFixture<NorthwindQuerySqlServerFixture>
    {
        private readonly NorthwindQuerySqlServerFixture _fixture;
        public DeadlockSqlServerTest(NorthwindQuerySqlServerFixture fixture)
        {                       
            _fixture = fixture;
        }

        protected override SqlServerTestStore CreateTestDatabase()
        {
            return SqlServerTestStore.CreateScratch(createDatabase: false);
        }

        protected override DbContext CreateContext(SqlServerTestStore testDatabase)
        {
            var context = _fixture.CreateContext(testDatabase);
            context.Database.EnsureCreated();
            return context;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}

#endif
