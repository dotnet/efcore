// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class StructuredJsonPathTest
{
    [ConditionalFact]
    public void StructuredJsonPathSegment_property_name()
    {
        var segment = new StructuredJsonPathSegment("Name");
        Assert.Equal("Name", segment.PropertyName);
        Assert.False(segment.IsArray);
        Assert.Equal("Name", segment.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPathSegment_array()
    {
        var segment = StructuredJsonPathSegment.Array;
        Assert.Null(segment.PropertyName);
        Assert.True(segment.IsArray);
        Assert.Equal("[]", segment.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPathSegment_empty_name_throws()
    {
        Assert.Throws<ArgumentException>(() => new StructuredJsonPathSegment(""));
    }

    [ConditionalFact]
    public void StructuredJsonPath_root()
    {
        var path = StructuredJsonPath.Root;
        Assert.True(path.IsRoot);
        Assert.Empty(path.Segments);
        Assert.Empty(path.Indices);
        Assert.Equal("$", path.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPath_simple_property()
    {
        var path = new StructuredJsonPath([new StructuredJsonPathSegment("Address")], []);
        Assert.False(path.IsRoot);
        Assert.Equal("$.Address", path.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPath_nested_properties()
    {
        var path = new StructuredJsonPath(
            [new StructuredJsonPathSegment("Address"), new StructuredJsonPathSegment("City")],
            []);
        Assert.Equal("$.Address.City", path.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPath_root_array_index()
    {
        var path = new StructuredJsonPath(
            [StructuredJsonPathSegment.Array, new StructuredJsonPathSegment("Name")],
            [3]);
        Assert.Equal("$[3].Name", path.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPath_with_array_index()
    {
        var path = new StructuredJsonPath(
            [new StructuredJsonPathSegment("Items"), StructuredJsonPathSegment.Array, new StructuredJsonPathSegment("Name")],
            [3]);
        Assert.Equal("$.Items[3].Name", path.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPath_with_multiple_array_indices()
    {
        var path = new StructuredJsonPath(
            [
                new StructuredJsonPathSegment("Orders"),
                StructuredJsonPathSegment.Array,
                new StructuredJsonPathSegment("Items"),
                StructuredJsonPathSegment.Array,
                new StructuredJsonPathSegment("Name")
            ],
            [1, 2]);
        Assert.Equal("$.Orders[1].Items[2].Name", path.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPath_array_only()
    {
        var path = new StructuredJsonPath(
            [new StructuredJsonPathSegment("Items"), StructuredJsonPathSegment.Array],
            [0]);
        Assert.Equal("$.Items[0]", path.ToString());
    }

    [ConditionalFact]
    public void StructuredJsonPath_AppendTo_builds_same_as_ToString()
    {
        var path = new StructuredJsonPath(
            [new StructuredJsonPathSegment("A"), StructuredJsonPathSegment.Array, new StructuredJsonPathSegment("B")],
            [5]);

        var sb = new System.Text.StringBuilder();
        path.AppendTo(sb);
        Assert.Equal(path.ToString(), sb.ToString());
    }
}
