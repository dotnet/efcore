// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ConcurrencyModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class PropertyEntryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : F1FixtureBase<TTestStore>, new()
    {
        [Fact]
        public virtual void Property_entry_original_value_is_set()
        {
            using (var c = CreateF1Context())
            {
                c.Database.CreateExecutionStrategy().Execute(context =>
                    {
                        using (context.Database.BeginTransaction())
                        {
                            var engine = context.Engines.First();
                            var trackedEntry = context.ChangeTracker.Entries<Engine>().First();
                            trackedEntry.Property(e => e.Name).OriginalValue = "ChangedEngine";

                            Assert.Equal(RelationalStrings.UpdateConcurrencyException("1", "0"),
                                Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges()).Message);
                        }
                    }, c);
            }
        }

        protected F1Context CreateF1Context() => Fixture.CreateContext(TestStore);

        protected PropertyEntryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose() => TestStore.Dispose();
    }
}
