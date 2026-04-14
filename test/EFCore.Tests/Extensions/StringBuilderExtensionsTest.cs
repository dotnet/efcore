// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class StringBuilderExtensionsTest
{
    [ConditionalFact]
    public void AppendJoin_joins_values()
    {
        Assert.Equal("a:b:c", new StringBuilder().AppendJoin(new[] { "a", "b", "c" }, ":").ToString());
        Assert.Equal("abc", new StringBuilder().AppendJoin(new[] { "a", "b", "c" }, string.Empty).ToString());
        Assert.Empty(new StringBuilder().AppendJoin(Array.Empty<string>(), ":").ToString());

        Assert.Equal(
            "11, 22, 33",
            new StringBuilder().AppendJoin(new[] { 1, 2, 3 }, (sb, v) => sb.Append(v).Append(v)).ToString());
    }
}
