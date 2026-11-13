// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class JsonTypesSqlServerTestBase(NonSharedFixture fixture) : JsonTypesRelationalTestBase(fixture)
{
    public override Task Can_read_write_collection_of_fixed_length_string_JSON_values(object? storeType)
        => base.Can_read_write_collection_of_fixed_length_string_JSON_values("nchar(32)");

    public override Task Can_read_write_collection_of_ASCII_string_JSON_values(object? storeType)
        => base.Can_read_write_collection_of_ASCII_string_JSON_values("varchar(max)");

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        builder = base.AddOptions(builder)
            .ConfigureWarnings(w => w.Ignore(SqlServerEventId.DecimalTypeDefaultWarning));
        new SqlServerDbContextOptionsBuilder(builder).UseNetTopologySuite();
        return builder;
    }
}
