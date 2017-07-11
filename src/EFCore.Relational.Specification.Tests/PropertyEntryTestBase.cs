// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class PropertyEntryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>
        where TTestStore : TestStore
        where TFixture : F1FixtureBase<TTestStore>, new()
    {
        [Fact]
        public virtual void Property_entry_original_value_is_set()
        {
            using (var c = CreateF1Context())
            {
                c.Database.CreateExecutionStrategy().Execute(c, context =>
                    {
                        using (context.Database.BeginTransaction())
                        {
                            var engine = context.Engines.OrderBy(e => e.Id).First();
                            var trackedEntry = context.ChangeTracker.Entries<Engine>().First();
                            trackedEntry.Property(e => e.Name).OriginalValue = "ChangedEngine";

                            Assert.Equal(RelationalStrings.UpdateConcurrencyException("1", "0"),
                                Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges()).Message);
                        }
                    });
            }
        }

        protected F1Context CreateF1Context() => Fixture.CreateContext();

        protected PropertyEntryTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }
    }
}
