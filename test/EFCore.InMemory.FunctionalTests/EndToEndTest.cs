// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class EndToEndInMemoryTest : IClassFixture<InMemoryFixture>
    {
        public EndToEndInMemoryTest(InMemoryFixture fixture) => Fixture = fixture;

        protected InMemoryFixture Fixture { get; }

        [Fact]
        public void Can_use_different_entity_types_end_to_end()
        {
            Can_add_update_delete_end_to_end<Private>();
            Can_add_update_delete_end_to_end<object>();
            Can_add_update_delete_end_to_end<List<Private>>();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Private
        {
        }

        private void Can_add_update_delete_end_to_end<T>()
            where T : class, new()
        {
            var type = typeof(T);
            var model = new Model();

            var entityType = model.AddEntityType(type);
            var idProperty = entityType.AddProperty("Id", typeof(int));
            var nameProperty = entityType.AddProperty("Name", typeof(string));
            entityType.GetOrSetPrimaryKey(idProperty);

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model)
                .UseInMemoryDatabase(nameof(EndToEndInMemoryTest))
                .UseInternalServiceProvider(Fixture.ServiceProvider);

            T entity;
            using (var context = new DbContext(optionsBuilder.Options))
            {
                var entry = context.ChangeTracker.GetInfrastructure().GetOrCreateEntry(new T());
                entity = (T)entry.Entity;

                entry[idProperty] = 42;
                entry[nameProperty] = "The";

                entry.SetEntityState(EntityState.Added);

                context.SaveChanges();
            }

            using (var context = new DbContext(optionsBuilder.Options))
            {
                var entityFromStore = context.Set<T>().Single();
                var entityEntry = context.Entry(entityFromStore);

                Assert.NotSame(entity, entityFromStore);
                Assert.Equal(42, entityEntry.Property(idProperty.Name).CurrentValue);
                Assert.Equal("The", entityEntry.Property(nameProperty.Name).CurrentValue);

                entityEntry.GetInfrastructure()[nameProperty] = "A";

                context.Update(entityFromStore);

                context.SaveChanges();
            }

            using (var context = new DbContext(optionsBuilder.Options))
            {
                var entityFromStore = context.Set<T>().Single();
                var entry = context.Entry(entityFromStore);

                Assert.Equal("A", entry.Property(nameProperty.Name).CurrentValue);

                context.Remove(entityFromStore);

                context.SaveChanges();
            }

            using (var context = new DbContext(optionsBuilder.Options))
            {
                Assert.Equal(0, context.Set<T>().Count());
            }
        }
    }
}
