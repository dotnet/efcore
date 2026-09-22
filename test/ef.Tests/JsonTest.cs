// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public class JsonTest
{
    [Fact]
    public void Literal_escapes()
        => Assert.Equal("\"test\\\\test\\\"test\"", Json.Literal("test\\test\"test"));

    [Fact]
    public void Literal_handles_null()
        => Assert.Equal("null", Json.Literal((string?)null));

    [Fact]
    public void Literal_handles_bool()
    {
        Assert.Equal("true", Json.Literal(true));
        Assert.Equal("false", Json.Literal(false));
        Assert.Equal("null", Json.Literal((bool?)null));
    }
}
