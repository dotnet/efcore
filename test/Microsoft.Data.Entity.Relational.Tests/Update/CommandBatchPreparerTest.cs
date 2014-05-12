// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class CommandBatchPreparerTest
    {
        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_added_entities()
        {
            var model = CreateModel();

            var stateEntry = new MixedStateEntry(
                CreateConfiguration(),
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Added);

            var commandBatches = new CommandBatchPreparer(new ParameterNameGeneratorFactory()).BatchCommands(new[] { stateEntry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count());

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Added, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Id", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Value", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Value", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_updated_entities()
        {
            var model = CreateModel();

            var stateEntry = new MixedStateEntry(
                CreateConfiguration(),
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Modified);

            var commandBatches = new CommandBatchPreparer(new ParameterNameGeneratorFactory()).BatchCommands(new[] { stateEntry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count());

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Modified, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Id", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Value", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Value", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_deleted_entities()
        {
            var model = CreateModel();

            var stateEntry = new MixedStateEntry(
                CreateConfiguration(),
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Deleted);

            var commandBatches = new CommandBatchPreparer(new ParameterNameGeneratorFactory()).BatchCommands(new[] { stateEntry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count());

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Deleted, command.EntityState);
            Assert.Equal(1, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Id", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);
        }

        private static DbContextConfiguration CreateConfiguration()
        {
            return new DbContext(new DbContextOptions().BuildConfiguration()).Configuration;
        }

        private static IModel CreateModel()
        {
            var model = new Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<FakeEntity>()
                .Key(c => c.Id)
                .Properties(ps => ps.Property(c => c.Value));

            return model;
        }

        private class FakeEntity
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}
