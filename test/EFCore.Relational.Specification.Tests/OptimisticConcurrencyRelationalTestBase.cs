// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class OptimisticConcurrencyRelationalTestBase<TFixture, TRowVersion>
    : OptimisticConcurrencyTestBase<TFixture, TRowVersion>
    where TFixture : F1RelationalFixture<TRowVersion>, new()
{
    protected OptimisticConcurrencyRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public virtual void Property_entry_original_value_is_set()
    {
        using var c = CreateF1Context();
        c.Database.CreateExecutionStrategy().Execute(
            c, context =>
            {
                using (context.Database.BeginTransaction())
                {
                    // ReSharper disable once UnusedVariable
                    var engine = context.Engines.OrderBy(e => e.Id).First();
                    var trackedEntry = context.ChangeTracker.Entries<Engine>().First();
                    trackedEntry.Property(e => e.Name).OriginalValue = "ChangedEngine";

                    Assert.Equal(
                        RelationalStrings.UpdateConcurrencyException("1", "0"),
                        Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges()).Message);
                }
            });
    }
}
