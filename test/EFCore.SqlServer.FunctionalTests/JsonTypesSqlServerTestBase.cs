// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public abstract class JsonTypesSqlServerTestBase : JsonTypesRelationalTestBase
{
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
        => Can_read_and_write_JSON_value<EnumU64CollectionType, List<EnumU64>>(nameof(EnumU64CollectionType.EnumU64),
            new List<EnumU64>
            {
                EnumU64.Min,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            """{"Prop":[0,-1,0,1,8]}""", // Because ulong is converted to long on SQL Server
            mappedCollection: true);

    public override void Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnumU64CollectionType, List<EnumU64?>>(nameof(NullableEnumU64CollectionType.EnumU64),
            new List<EnumU64?>
            {
                EnumU64.Min,
                null,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64?)8
            },
            """{"Prop":[0,null,-1,0,1,8]}""", // Because ulong is converted to long on SQL Server
            mappedCollection: true);

    public override void Can_read_write_collection_of_fixed_length_string_JSON_values(object? storeType)
        => base.Can_read_write_collection_of_fixed_length_string_JSON_values("nchar(32)");

    public override void Can_read_write_collection_of_ASCII_string_JSON_values(object? storeType)
        => base.Can_read_write_collection_of_ASCII_string_JSON_values("varchar(max)");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.UseSqlServer(b => b.UseNetTopologySuite()));
}
