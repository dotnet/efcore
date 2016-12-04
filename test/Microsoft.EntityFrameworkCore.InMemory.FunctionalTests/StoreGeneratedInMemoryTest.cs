// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class StoreGeneratedInMemoryTest
        : StoreGeneratedTestBase<InMemoryTestStore, StoreGeneratedInMemoryTest.StoreGeneratedInMemoryFixture>
    {
        public StoreGeneratedInMemoryTest(StoreGeneratedInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public override void Identity_key_with_read_only_before_save_throws_if_explicit_values_set()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Identity_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Identity_property_on_Modified_entity_is_included_in_update_when_modified()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Identity_property_on_Modified_entity_is_not_included_in_update_when_not_modified()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Computed_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Computed_property_on_Modified_entity_is_included_in_update_when_modified()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_computed_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_computed_property_on_Added_entity_cannot_have_value_set_explicitly()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_computed_property_on_Modified_entity_is_not_included_in_update_even_when_modified()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_identity_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_identity_property_on_Added_entity_gets_store_value_even_when_set_explicitly()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_identity_property_on_Modified_entity_is_not_included_in_update_when_modified()
        {
            // In-memory store does not support store generation
        }

        [Fact]
        public override void Always_identity_property_on_Modified_entity_is_not_included_in_the_update_when_not_modified()
        {
            // In-memory store does not support store generation
        }

        public class StoreGeneratedInMemoryFixture : StoreGeneratedFixtureBase
        {
            private const string DatabaseName = "StoreGeneratedTest";

            private readonly IServiceProvider _serviceProvider;

            public StoreGeneratedInMemoryFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override InMemoryTestStore CreateTestStore()
                => InMemoryTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseInMemoryDatabase()
                            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new StoreGeneratedContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureDeleted();
                            context.Database.EnsureCreated();
                        }
                    });

            public override DbContext CreateContext(InMemoryTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseInMemoryDatabase()
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new StoreGeneratedContext(optionsBuilder.Options);

                return context;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Gumball>(b =>
                    {
                        // In-memory store does not support store generationof keys
                        b.Property(e => e.Id).Metadata.IsReadOnlyBeforeSave = false;
                    });
            }
        }
    }
}
