// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

public class HierarchyIdJsonConverterTest
{
    [ConditionalFact]
    public void Read_works()
        => Assert.Equal("/1/", JsonSerializer.Deserialize<HierarchyId>("\"/1/\"").ToString());

    [ConditionalFact]
    public void Read_works_when_null()
        => Assert.Null(JsonSerializer.Deserialize<HierarchyId>("null"));

    [ConditionalFact]
    public void Write_works()
        => Assert.Equal("\"/1/\"", JsonSerializer.Serialize(new HierarchyId("/1/")));

    [ConditionalFact]
    public void Write_works_when_null()
        => Assert.Equal("null", JsonSerializer.Serialize<HierarchyId>(null));
}
