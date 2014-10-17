// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if NET45

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class DeadlockTest : DeadlockTestBase<DeadlockTest.AtsTestStore>, IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;

        public DeadlockTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override AtsTestStore CreateTestDatabase()
        {
            var tableSuffix = Guid.NewGuid().ToString().Replace("-", "");
            return new AtsTestStore(_fixture.CreateContext(tableSuffix), tableSuffix);
        }

        protected override DbContext CreateContext(AtsTestStore testDatabase)
        {
            return _fixture.CreateContext(testDatabase.TableSuffix);
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }

        public class AtsTestStore : TestStore
        {
            private readonly DbContext _context;
            public readonly string TableSuffix;

            public AtsTestStore(DbContext context, string tableSuffix)
            {
                _context = context;
                context.Database.EnsureCreated();
                TableSuffix = tableSuffix;
            }

            public override void Dispose()
            {
                using (_context)
                {
                    _context.Database.EnsureDeleted();
                }
            }
        }
    }
}

#endif
