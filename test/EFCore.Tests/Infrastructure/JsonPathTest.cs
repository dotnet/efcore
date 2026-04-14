// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class JsonPathTest
{
    [ConditionalFact]
    public void JsonPathSegment_property_name()
    {
        var segment = new JsonPathSegment("Name");
        Assert.Equal("Name", segment.PropertyName);
        Assert.False(segment.IsArray);
        Assert.Equal("Name", segment.ToString());
    }

    [ConditionalFact]
    public void JsonPathSegment_array()
    {
        var segment = JsonPathSegment.Array;
        Assert.Null(segment.PropertyName);
        Assert.True(segment.IsArray);
        Assert.Equal("[]", segment.ToString());
    }

    [ConditionalFact]
    public void JsonPathSegment_empty_name_throws()
    {
        Assert.Throws<ArgumentException>(() => new JsonPathSegment(""));
    }

    [ConditionalFact]
    public void JsonPath_root()
    {
        var path = JsonPath.Root;
        Assert.True(path.IsRoot);
        Assert.Empty(path.Segments);
        Assert.Empty(path.Ordinals);
        Assert.Equal("$", path.ToString());
    }

    [ConditionalFact]
    public void JsonPath_simple_property()
    {
        var path = new JsonPath([new JsonPathSegment("Address")], []);
        Assert.False(path.IsRoot);
        Assert.Equal("$.Address", path.ToString());
    }

    [ConditionalFact]
    public void JsonPath_nested_properties()
    {
        var path = new JsonPath(
            [new JsonPathSegment("Address"), new JsonPathSegment("City")],
            []);
        Assert.Equal("$.Address.City", path.ToString());
    }

    [ConditionalFact]
    public void JsonPath_root_array_index()
    {
        var path = new JsonPath(
            [JsonPathSegment.Array, new JsonPathSegment("Name")],
            [3]);
        Assert.Equal("$[3].Name", path.ToString());
    }

    [ConditionalFact]
    public void JsonPath_with_array_index()
    {
        var path = new JsonPath(
            [new JsonPathSegment("Items"), JsonPathSegment.Array, new JsonPathSegment("Name")],
            [3]);
        Assert.Equal("$.Items[3].Name", path.ToString());
    }

    [ConditionalFact]
    public void JsonPath_with_multiple_array_indices()
    {
        var path = new JsonPath(
            [
                new JsonPathSegment("Orders"),
                JsonPathSegment.Array,
                new JsonPathSegment("Items"),
                JsonPathSegment.Array,
                new JsonPathSegment("Name")
            ],
            [1, 2]);
        Assert.Equal("$.Orders[1].Items[2].Name", path.ToString());
    }

    [ConditionalFact]
    public void JsonPath_array_only()
    {
        var path = new JsonPath(
            [new JsonPathSegment("Items"), JsonPathSegment.Array],
            [0]);
        Assert.Equal("$.Items[0]", path.ToString());
    }

    [ConditionalFact]
    public void JsonPath_AppendTo_builds_same_as_ToString()
    {
        var path = new JsonPath(
            [new JsonPathSegment("A"), JsonPathSegment.Array, new JsonPathSegment("B")],
            [5]);

        var sb = new System.Text.StringBuilder();
        path.AppendTo(sb);
        Assert.Equal(path.ToString(), sb.ToString());
    }
}
