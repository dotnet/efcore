// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata;

public class RelationalJsonIndexTest
{
    [Fact]
    public void Constructor_throws_when_collectionIndices_count_does_not_match_elements()
    {
        var elements = new IRelationalJsonElement[] { null!, null! };
        var collectionIndices = new IReadOnlyList<int?>?[] { [null] }; // 1 entry, but 2 elements

        var exception = Assert.Throws<ArgumentException>(
            () => new RelationalJsonIndex(elements, collectionIndices));

        Assert.Equal(
            RelationalStrings.JsonPathIndexElementsCollectionIndicesMismatch(2, 1)
            + " (Parameter 'collectionIndices')",
            exception.Message);
    }

    [Fact]
    public void Constructor_succeeds_when_collectionIndices_is_null()
    {
        var elements = new IRelationalJsonElement[] { null! };

        var index = new RelationalJsonIndex(elements, collectionIndices: null);

        Assert.Same(elements, index.Elements);
        Assert.Null(index.CollectionIndices);
    }

    [Fact]
    public void Constructor_succeeds_when_counts_match()
    {
        var elements = new IRelationalJsonElement[] { null!, null! };
        var collectionIndices = new IReadOnlyList<int?>?[] { [null], [0] };

        var index = new RelationalJsonIndex(elements, collectionIndices);

        Assert.Same(elements, index.Elements);
        Assert.Same(collectionIndices, index.CollectionIndices);
    }
}
