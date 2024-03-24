// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class BadDataJsonDeserializationSqliteTest : BadDataJsonDeserializationTestBase
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.UseSqlite(b => b.UseNetTopologySuite()));
}
