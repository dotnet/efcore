// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

#nullable disable

public abstract class TPCInheritanceQuerySqlServerFixtureBase : TPCInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // Start TPC sequences past the explicit seed key range to avoid PK collisions on insert.
        if (!UseGeneratedKeys)
        {
            modelBuilder.HasSequence("AnimalSequence").StartsAt(10);
            modelBuilder.HasSequence("DrinkSequence").StartsAt(10);
        }
    }
}
