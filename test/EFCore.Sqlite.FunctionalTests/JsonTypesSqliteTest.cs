// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

[SpatialiteRequired]
public class JsonTypesSqliteTest : JsonTypesTestBase<JsonTypesSqliteTest.JsonTypesSqliteFixture>
{
    public JsonTypesSqliteTest(JsonTypesSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
    }

    public class JsonTypesSqliteFixture : JsonTypesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new SqliteDbContextOptionsBuilder(builder).UseNetTopologySuite();

            return base.AddOptions(builder);
        }

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkSqliteNetTopologySuite());
    }
}
