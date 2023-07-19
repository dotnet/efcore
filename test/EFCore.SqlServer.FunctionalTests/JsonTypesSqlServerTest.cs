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
            json = """{"Prop":-1}"""; // Because ulong is converted to long on SQL Server
        }

        base.Can_read_write_ulong_enum_JSON_values(value, json);
    }

    public override void Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
    {
        if (Equals(value, ulong.MaxValue))
        {
            json = """{"Prop":-1}"""; // Because ulong is converted to long on SQL Server
        }

        base.Can_read_write_nullable_ulong_enum_JSON_values(value, json);
    }

    public override void Can_read_write_collection_of_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.EnumU64)),
            new List<EnumU64>
            {
                EnumU64.Min,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            """{"Prop":[0,-1,0,1,8]}"""); // Because ulong is converted to long on SQL Server

    public override void Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.EnumU64)),
            new List<EnumU64?>
            {
                EnumU64.Min,
                null,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            """{"Prop":[0,null,-1,0,1,8]}"""); // Because ulong is converted to long on SQL Server

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
