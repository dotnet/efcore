// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class CommandBatchPreparerTest
    {
        [Fact]
        public void Constructor_checks_arguments()
        {
            Assert.Equal(
                "modificationCommandBatchFactory",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    new TestCommandBatchPreparer(
                        null,
                        new ParameterNameGeneratorFactory(),
                        new BidirectionalAdjacencyListGraphFactory(),
                        new ModificationCommandComparer())).ParamName);

            Assert.Equal(
                "parameterNameGeneratorFactory",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    new TestCommandBatchPreparer(
                        new Mock<ModificationCommandBatchFactory>().Object,
                        null,
                        new BidirectionalAdjacencyListGraphFactory(),
                        new ModificationCommandComparer())).ParamName);

            Assert.Equal(
                "graphFactory",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    new TestCommandBatchPreparer(
                        new Mock<ModificationCommandBatchFactory>().Object,
                        new ParameterNameGeneratorFactory(),
                        null,
                        new ModificationCommandComparer())).ParamName);

            Assert.Equal(
                "modificationCommandComparer",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    new TestCommandBatchPreparer(
                        new Mock<ModificationCommandBatchFactory>().Object,
                        new ParameterNameGeneratorFactory(),
                        new BidirectionalAdjacencyListGraphFactory(),
                        null)).ParamName);
        }

        [Fact]
        public async Task BatchCommands_creates_valid_batch_for_added_entities()
        {
            var factory = CreateConfiguration().ScopedServiceProvider.GetRequiredService<StateEntryFactory>();

            var stateEntry = factory.Create(
                CreateSimpleFKModel().GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Added);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { stateEntry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count());

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Added, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Id", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
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
        public async Task BatchCommands_creates_valid_batch_for_modified_entities()
        {
            var factory = CreateConfiguration().ScopedServiceProvider.GetRequiredService<StateEntryFactory>();

            var stateEntry = factory.Create(
                CreateSimpleFKModel().GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Modified);
            stateEntry.SetPropertyModified(stateEntry.EntityType.GetPrimaryKey().Properties.Single(), isModified: false);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { stateEntry }).ToArray();
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
            var factory = CreateConfiguration().ScopedServiceProvider.GetRequiredService<StateEntryFactory>();

            var stateEntry = factory.Create(
                CreateSimpleFKModel().GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });

            await stateEntry.SetEntityStateAsync(EntityState.Deleted);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { stateEntry }).ToArray();
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

        [Fact]
        public async Task BatchCommands_sorts_related_added_entities()
        {
            var configuration = CreateConfiguration();
            var model = CreateSimpleFKModel();
            var factory = configuration.ScopedServiceProvider.GetRequiredService<StateEntryFactory>();

            var stateEntry = factory.Create(
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });
            await stateEntry.SetEntityStateAsync(EntityState.Added);

            var relatedStateEntry = factory.Create(
                model.GetEntityType(typeof(RelatedFakeEntity)), new RelatedFakeEntity { Id = 42 });
            await relatedStateEntry.SetEntityStateAsync(EntityState.Added);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedStateEntry, stateEntry }).ToArray();

            Assert.Equal(
                new[] { stateEntry, relatedStateEntry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.StateEntries.Single()));
        }

        [Fact]
        public async Task BatchCommands_sorts_added_and_related_modified_entities()
        {
            var configuration = CreateConfiguration();
            var model = CreateSimpleFKModel();
            var factory = configuration.ScopedServiceProvider.GetRequiredService<StateEntryFactory>();

            var stateEntry = factory.Create(
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });
            await stateEntry.SetEntityStateAsync(EntityState.Added);

            var relatedStateEntry = factory.Create(
                model.GetEntityType(typeof(RelatedFakeEntity)), new RelatedFakeEntity { Id = 42 });
            await relatedStateEntry.SetEntityStateAsync(EntityState.Modified);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedStateEntry, stateEntry }).ToArray();

            Assert.Equal(
                new[] { stateEntry, relatedStateEntry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.StateEntries.Single()));
        }

        [Fact]
        public async Task BatchCommands_sorts_unrelated_entities()
        {
            var configuration = CreateConfiguration();
            var model = CreateSimpleFKModel();
            var factory = configuration.ScopedServiceProvider.GetRequiredService<StateEntryFactory>();

            var firstStateEntry = factory.Create(
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });
            await firstStateEntry.SetEntityStateAsync(EntityState.Added);

            var secondStateEntry = factory.Create(
                model.GetEntityType(typeof(RelatedFakeEntity)), new RelatedFakeEntity { Id = 1 });
            await secondStateEntry.SetEntityStateAsync(EntityState.Added);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { secondStateEntry, firstStateEntry }).ToArray();

            Assert.Equal(
                new[] { firstStateEntry, secondStateEntry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.StateEntries.Single()));
        }

        [Fact]
        public async Task BatchCommands_sorts_entities_when_reparenting()
        {
            var configuration = CreateConfiguration();
            var model = CreateCyclicFKModel();
            var factory = configuration.ScopedServiceProvider.GetRequiredService<StateEntryFactory>();

            var previousParent = factory.Create(
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 42, Value = "Test" });
            await previousParent.SetEntityStateAsync(EntityState.Deleted);

            var newParent = factory.Create(
                model.GetEntityType(typeof(FakeEntity)), new FakeEntity { Id = 3, Value = "Test" });
            await newParent.SetEntityStateAsync(EntityState.Added);

            var relatedStateEntry = factory.Create(
                model.GetEntityType(typeof(RelatedFakeEntity)), new RelatedFakeEntity { Id = 1, RelatedId = 3 });
            await relatedStateEntry.SetEntityStateAsync(EntityState.Modified);
            relatedStateEntry.OriginalValues[relatedStateEntry.EntityType.GetProperty("RelatedId")] = 42;
            relatedStateEntry.SetPropertyModified(relatedStateEntry.EntityType.GetPrimaryKey().Properties.Single(), isModified: false);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedStateEntry, previousParent, newParent }).ToArray();

            Assert.Equal(
                new[] { newParent, relatedStateEntry, previousParent },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.StateEntries.Single()));
        }

        [Fact]
        public async Task BatchCommands_creates_batches_lazily()
        {
            var configuration = CreateConfiguration();
            var model = CreateSimpleFKModel();
            var factory = configuration.ScopedServiceProvider.GetRequiredService<StateEntryFactory>();

            var fakeEntity = new FakeEntity { Id = 42, Value = "Test" };
            var stateEntry = factory.Create(
                model.GetEntityType(typeof(FakeEntity)), fakeEntity);
            await stateEntry.SetEntityStateAsync(EntityState.Added);

            var relatedStateEntry = factory.Create(
                model.GetEntityType(typeof(RelatedFakeEntity)), new RelatedFakeEntity { Id = 42 });
            await relatedStateEntry.SetEntityStateAsync(EntityState.Added);

            var modificationCommandBatchFactoryMock = new Mock<ModificationCommandBatchFactory>();

            var commandBatches = CreateCommandBatchPreparer(modificationCommandBatchFactoryMock.Object).BatchCommands(new[] { relatedStateEntry, stateEntry });

            var commandBatchesEnumerator = commandBatches.GetEnumerator();
            commandBatchesEnumerator.MoveNext();

            modificationCommandBatchFactoryMock.Verify(mcb => mcb.Create(), Times.Once);

            commandBatchesEnumerator.MoveNext();

            modificationCommandBatchFactoryMock.Verify(mcb => mcb.Create(), Times.Exactly(2));
        }

        private static DbContextConfiguration CreateConfiguration()
        {
            return new DbContext(new DbContextOptions().UseInMemoryStore(persist: false)).Configuration;
        }

        private static CommandBatchPreparer CreateCommandBatchPreparer(ModificationCommandBatchFactory modificationCommandBatchFactory = null)
        {
            modificationCommandBatchFactory =
                modificationCommandBatchFactory ?? new TestModificationCommandBatchFactory(new Mock<SqlGenerator>().Object);

            return new TestCommandBatchPreparer(modificationCommandBatchFactory,
                new ParameterNameGeneratorFactory(),
                new BidirectionalAdjacencyListGraphFactory(),
                new ModificationCommandComparer());
        }

        private static IModel CreateSimpleFKModel()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<FakeEntity>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(c => c.Value);
                });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.Key(c => c.Id);
                    b.ForeignKey<FakeEntity>(c => c.Id);
                });

            return model;
        }

        private static IModel CreateCyclicFKModel()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<FakeEntity>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(c => c.Value);
                });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.Key(c => c.Id);
                    b.ForeignKey<FakeEntity>(c => c.RelatedId);
                });

            modelBuilder
                .Entity<FakeEntity>()
                .ForeignKey<RelatedFakeEntity>(c => c.RelatedId);

            return model;
        }

        private class FakeEntity
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public int? RelatedId { get; set; }
        }

        private class RelatedFakeEntity
        {
            public int Id { get; set; }
            public int? RelatedId { get; set; }
        }

        private class TestCommandBatchPreparer : CommandBatchPreparer
        {
            public TestCommandBatchPreparer(
                ModificationCommandBatchFactory modificationCommandBatchFactory,
                ParameterNameGeneratorFactory parameterNameGeneratorFactory,
                GraphFactory graphFactory,
                ModificationCommandComparer modificationCommandComparer)
                : base(modificationCommandBatchFactory, parameterNameGeneratorFactory, graphFactory, modificationCommandComparer)
            {
            }

            public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property)
            {
                return property.Relational();
            }

            public override IRelationalEntityTypeExtensions GetEntityTypeExtensions(IEntityType entityType)
            {
                return entityType.Relational();
            }
        }

        private class TestModificationCommandBatchFactory : ModificationCommandBatchFactory
        {
            public TestModificationCommandBatchFactory(SqlGenerator sqlGenerator)
                : base(sqlGenerator)
            {
            }

            public override ModificationCommandBatch Create()
            {
                return new TestModificationCommandBatch(SqlGenerator);
            }
        }

        private class TestModificationCommandBatch : SingularModificationCommandBatch
        {
            public TestModificationCommandBatch(SqlGenerator sqlGenerator)
                : base(sqlGenerator)
            {
            }

            public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property)
            {
                return property.Relational();
            }
        }
    }
}
