// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Relational.Update
{
    public class CommandBatchPreparerTest
    {
        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_added_entities()
        {
            var model = CreateModel();
            var stateManager =
                new StateManager(model, Mock.Of<ActiveIdentityGenerators>(), new IEntityStateListener[0], new EntityKeyFactorySource(), new StateEntryFactory());
            var stateEntry = new MixedStateEntry(stateManager, model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });
            await stateEntry.SetEntityStateAsync(EntityState.Added, new CancellationToken());

            var commandBatches = new CommandBatchPreparer().BatchCommands(new[] { stateEntry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().BatchCommands.Count());

            var command = commandBatches.First().BatchCommands.Single();
            Assert.Equal(ModificationOperation.Insert, command.Operation);
            Assert.Equal(
                new[]
                    {
                        new KeyValuePair<string, object>("Id", 42),
                        new KeyValuePair<string, object>("Value", "Test"),
                    },
                command.ColumnValues);
        }

        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_updated_entities()
        {
            var model = CreateModel();
            var stateManager =
                new StateManager(model, Mock.Of<ActiveIdentityGenerators>(), new IEntityStateListener[0], new EntityKeyFactorySource(), new StateEntryFactory());
            var stateEntry = new MixedStateEntry(stateManager, model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });
            await stateEntry.SetEntityStateAsync(EntityState.Modified, new CancellationToken());

            var commandBatches = new CommandBatchPreparer().BatchCommands(new[] { stateEntry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().BatchCommands.Count());

            var command = commandBatches.First().BatchCommands.Single();
            Assert.Equal(ModificationOperation.Update, command.Operation);

            Assert.Equal(
                new[]
                    {
                        new KeyValuePair<string, object>("Value", "Test"),
                    },
                command.ColumnValues);

            Assert.Equal(
                new[]
                    {
                        new KeyValuePair<string, object>("Id", 42),
                    },
                command.WhereClauses);
        }

        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_deleted_entities()
        {
            var model = CreateModel();
            var stateManager =
                new StateManager(model, Mock.Of<ActiveIdentityGenerators>(), new IEntityStateListener[0], new EntityKeyFactorySource(), new StateEntryFactory());
            var stateEntry = new MixedStateEntry(stateManager, model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });
            await stateEntry.SetEntityStateAsync(EntityState.Deleted, new CancellationToken());

            var commandBatches = new CommandBatchPreparer().BatchCommands(new[] { stateEntry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().BatchCommands.Count());

            var command = commandBatches.First().BatchCommands.Single();
            Assert.Equal(ModificationOperation.Delete, command.Operation);

            Assert.Equal(null, command.ColumnValues);

            Assert.Equal(
                new[]
                    {
                        new KeyValuePair<string, object>("Id", 42),
                    },
                command.WhereClauses);
        }

        private static Entity.Metadata.Model CreateModel()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<FakeEntity>()
                .Key(c => c.Id)
                .Properties(ps => ps.Property(c => c.Value));

            return model;
        }

        public class FakeEntity
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}
