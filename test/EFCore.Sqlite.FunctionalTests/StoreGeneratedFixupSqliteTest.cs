// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class StoreGeneratedFixupSqliteTest : StoreGeneratedFixupRelationalTestBase<
    StoreGeneratedFixupSqliteTest.StoreGeneratedFixupSqliteFixture>
{
    public StoreGeneratedFixupSqliteTest(StoreGeneratedFixupSqliteFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public void Temp_values_can_be_made_permanent()
    {
        using var context = CreateContext();
        var entry = context.Add(new TestTemp());

        Assert.True(entry.Property(e => e.Id).IsTemporary);
        Assert.False(entry.Property(e => e.NotId).IsTemporary);

        var tempValue = entry.Property(e => e.Id).CurrentValue;

        entry.Property(e => e.Id).IsTemporary = false;

        context.SaveChanges();

        Assert.False(entry.Property(e => e.Id).IsTemporary);
        Assert.Equal(tempValue, entry.Property(e => e.Id).CurrentValue);
    }

    protected override bool EnforcesFKs
        => true;

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class StoreGeneratedFixupSqliteFixture : StoreGeneratedFixupRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
