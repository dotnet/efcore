// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Microsoft.Data.FunctionalTests;
using Xunit;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override EntityConfiguration Configuration
        {
            get { return _fixture.Configuration; }
        }
    }

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly EntityConfiguration _configuration;
        private readonly TestDatabase _testDatabase;

        public NorthwindQueryFixture()
        {
            _testDatabase = TestDatabase.Northwind().Result;

            _configuration
                = new EntityConfigurationBuilder()
                    .UseModel(CreateModel())
                    .SqlServerConnectionString(_testDatabase.Connection.ConnectionString)
                    .BuildConfiguration();
        }

        public override EntityConfiguration Configuration
        {
            get { return _configuration; }
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }
    }
}
