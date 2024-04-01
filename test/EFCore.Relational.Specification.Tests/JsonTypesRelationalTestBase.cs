// Licensed to the .NET Foundation under one or more agreements.
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
