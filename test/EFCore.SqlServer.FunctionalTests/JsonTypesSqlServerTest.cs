// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class JsonTypesSqlServerTest : JsonTypesTestBase<JsonTypesSqlServerTest.JsonTypesSqlServerFixture>
{
    public JsonTypesSqlServerTest(JsonTypesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
    }

    public override void Can_read_write_ulong_enum_JSON_values(EnumU64 value, string json)
    {
        if (value == EnumU64.Max)
        {
            json = "{\"Prop\":-1}"; // Because ulong is converted to long on SQL Server
        }

        base.Can_read_write_ulong_enum_JSON_values(value, json);
    }

    public override void Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
    {
        if (Equals(value, ulong.MaxValue))
        {
            json = "{\"Prop\":-1}"; // Because ulong is converted to long on SQL Server
        }

        base.Can_read_write_nullable_ulong_enum_JSON_values(value, json);
    }

    public class JsonTypesSqlServerFixture : JsonTypesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new SqlServerDbContextOptionsBuilder(builder).UseNetTopologySuite();
            var options = base.AddOptions(builder).ConfigureWarnings(
                c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

            return options;
        }

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkSqlServerNetTopologySuite());
    }
}
