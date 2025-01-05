// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class InheritanceQueryInMemoryFixture : InheritanceQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    public override bool EnableComplexTypes
        => false;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            c => c.Log(InMemoryEventId.TransactionIgnoredWarning));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<AnimalQuery>().ToInMemoryQuery(() => context.Set<Bird>().Select(b => MaterializeView(b)));
    }

    private static AnimalQuery MaterializeView(Bird bird)
    {
        switch (bird)
        {
            case Kiwi kiwi:
                return new KiwiQuery
                {
                    Name = kiwi.Name,
                    CountryId = kiwi.CountryId,
                    EagleId = kiwi.EagleId,
                    FoundOn = kiwi.FoundOn,
                    IsFlightless = kiwi.IsFlightless
                };
            case Eagle eagle:
                return new EagleQuery
                {
                    Name = eagle.Name,
                    CountryId = eagle.CountryId,
                    EagleId = eagle.EagleId,
                    Group = eagle.Group,
                    IsFlightless = eagle.IsFlightless
                };
        }

        throw new InvalidOperationException();
    }
}
