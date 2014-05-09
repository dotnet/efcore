// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override ImmutableDbContextOptions Configuration
        {
            get { return _fixture.Configuration; }
        }
    }

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly ImmutableDbContextOptions _configuration;
        private readonly TestDatabase _testDatabase;

        public NorthwindQueryFixture()
        {
            _testDatabase = TestDatabase.Northwind().Result;

            _configuration
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSqlServer(_testDatabase.Connection.ConnectionString)
                    .BuildConfiguration();
        }

        public override ImmutableDbContextOptions Configuration
        {
            get { return _configuration; }
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }
    }
}
