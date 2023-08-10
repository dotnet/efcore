// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

[SpatialiteRequired]
public class JsonTypesSqliteTest : JsonTypesRelationalTestBase
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.UseSqlite(b => b.UseNetTopologySuite()));
}
