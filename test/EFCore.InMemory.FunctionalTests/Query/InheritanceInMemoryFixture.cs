// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceInMemoryFixture : InheritanceFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                c => c.Log(InMemoryEventId.TransactionIgnoredWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Query<AnimalQuery>()
                .ToQuery(
                    () => context.Set<Bird>()
                        .Select(b => MaterializeView(b)));
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
}
