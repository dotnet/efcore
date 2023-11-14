// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public abstract class JsonTypesSqlServerTestBase : JsonTypesRelationalTestBase
{
    public override void Can_read_write_collection_of_fixed_length_string_JSON_values(object? storeType)
        => base.Can_read_write_collection_of_fixed_length_string_JSON_values("nchar(32)");

    public override void Can_read_write_collection_of_ASCII_string_JSON_values(object? storeType)
        => base.Can_read_write_collection_of_ASCII_string_JSON_values("varchar(max)");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.UseSqlServer(b => b.UseNetTopologySuite()));
}
