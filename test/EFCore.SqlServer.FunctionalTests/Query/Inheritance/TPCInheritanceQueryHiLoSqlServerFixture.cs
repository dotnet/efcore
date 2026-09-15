// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCInheritanceQueryHiLoSqlServerFixture : TPCInheritanceQuerySqlServerFixtureBase
{
    protected override string StoreName
        => "TPCHiLoInheritanceTest";

    public override bool UseGeneratedKeys
        => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseHiLo();

        base.OnModelCreating(modelBuilder, context);
    }
}
