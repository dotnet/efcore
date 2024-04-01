// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class ManyToManySplitQuerySqliteFixture : ManyToManyQuerySqliteFixture
{
    protected override string StoreName
        => "ManyToManySplitQuery";

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder.UseSqlite(b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
}
