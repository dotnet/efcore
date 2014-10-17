// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if NET45

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class DeadlockTest : DeadlockTestBase<SqlServerTestDatabase>, IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;
        public DeadlockTest(NorthwindQueryFixture fixture)
        {                       
            _fixture = fixture;
            _fixture.InitLogger();
        }

        protected override SqlServerTestDatabase CreateTestDatabase()
        {
            return SqlServerTestDatabase.Scratch(createDatabase: false).Result;
        }

        protected override DbContext CreateContext(SqlServerTestDatabase testDatabase)
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
