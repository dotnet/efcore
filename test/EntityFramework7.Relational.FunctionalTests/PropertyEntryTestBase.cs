// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Relational.Internal;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class PropertyEntryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : F1FixtureBase<TTestStore>, new()
    {
        [Fact]
        public virtual void Property_entry_original_value_is_set()
        {
            using (var context = CreateF1Context())
            {
                var engine = context.Engines.First();
                var trackedEntry = context.ChangeTracker.Entries<Engine>().First();
                trackedEntry.Property(e => e.Name).OriginalValue = "ChangedEngine";

                Assert.Equal(Strings.UpdateConcurrencyException("1", "0"),
                    Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges()).Message);
            }
        }

        protected F1Context CreateF1Context()
        {
            return Fixture.CreateContext(TestStore);
        }

        protected PropertyEntryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose()
        {
            TestStore.Dispose();
        }
    }
}
