// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class PropertyEntryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : F1FixtureBase, new()
    {
        protected PropertyEntryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Property_entry_original_value_is_set()
        {
            using (var c = CreateF1Context())
            {
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

        protected F1Context CreateF1Context() => Fixture.CreateContext();
    }
}
