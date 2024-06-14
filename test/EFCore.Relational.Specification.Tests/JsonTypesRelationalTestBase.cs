﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class JsonTypesRelationalTestBase : JsonTypesTestBase
{
    [ConditionalTheory]
    [InlineData(null)]
    public virtual Task Can_read_write_collection_of_fixed_length_string_JSON_values(object? storeType)
        => Can_read_and_write_JSON_collection_value<StringCollectionType, List<string>>(
            b => b.ElementType().IsFixedLength().HasMaxLength(32),
            nameof(StringCollectionType.String),
            [
                "MinValue",
                "Value",
                "MaxValue"
            ],
            """{"Prop":["MinValue","Value","MaxValue"]}""",
            facets: new Dictionary<string, object?>
            {
                { RelationalAnnotationNames.IsFixedLength, true },
                { RelationalAnnotationNames.StoreType, storeType },
                { CoreAnnotationNames.MaxLength, 32 }
            });

    [ConditionalTheory]
    [InlineData(null)]
    public virtual Task Can_read_write_collection_of_ASCII_string_JSON_values(object? storeType)
        => Can_read_and_write_JSON_collection_value<StringCollectionType, List<string>>(
            b => b.ElementType().IsUnicode(false),
            nameof(StringCollectionType.String),
            [
                "MinValue",
                "Value",
                "MaxValue"
            ],
            """{"Prop":["MinValue","Value","MaxValue"]}""",
            facets: new Dictionary<string, object?>
            {
                { RelationalAnnotationNames.StoreType, storeType }, { CoreAnnotationNames.Unicode, false }
            });

    public override Task Can_read_write_ulong_enum_JSON_values(EnumU64 value, string json)
    {
        if (value == EnumU64.Max)
        {
            json = """{"Prop":-1}"""; // Because ulong is converted to long on SQL Server
        }

        return base.Can_read_write_ulong_enum_JSON_values(value, json);
    }

    public override Task Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
    {
        if (Equals(value, ulong.MaxValue))
        {
            json = """{"Prop":-1}"""; // Because ulong is converted to long on SQL Server
        }

        return base.Can_read_write_nullable_ulong_enum_JSON_values(value, json);
    }

    public override Task Can_read_write_collection_of_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value<EnumU64CollectionType, List<EnumU64>>(
            nameof(EnumU64CollectionType.EnumU64),
            [
                EnumU64.Min,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            ],
            """{"Prop":[0,-1,0,1,8]}""", // Because ulong is converted to long on SQL Server
            mappedCollection: true);

    public override Task Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnumU64CollectionType, List<EnumU64?>>(
            nameof(NullableEnumU64CollectionType.EnumU64),
            [
                EnumU64.Min,
                null,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64?)8
            ],
            """{"Prop":[0,null,-1,0,1,8]}""", // Because ulong is converted to long on SQL Server
            mappedCollection: true);

    protected override void AssertElementFacets(IElementType element, Dictionary<string, object?>? facets)
    {
        base.AssertElementFacets(element, facets);

        Assert.Same(element.FindTypeMapping(), element.FindRelationalTypeMapping());
        Assert.Equal(FacetValue(RelationalAnnotationNames.IsFixedLength), element.IsFixedLength());

        var expectedStoreType = FacetValue(RelationalAnnotationNames.StoreType)
            ?? element.FindRelationalTypeMapping()!.StoreType;

        Assert.Equal(expectedStoreType, element.GetStoreType());

        object? FacetValue(string facetName)
            => facets?.TryGetValue(facetName, out var facet) == true ? facet : null;
    }
}
