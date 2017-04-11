// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Update
{
    public class CommandBatchPreparerTest
    {
        [Fact]
        public void BatchCommands_creates_valid_batch_for_added_entities()
        {
            var stateManager = CreateContextServices(CreateSimpleFKModel()).GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });

            entry.SetEntityState(EntityState.Added);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count());

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Added, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Id", columnMod.ColumnName);
            Assert.Same(entry, columnMod.Entry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Value", columnMod.ColumnName);
            Assert.Same(entry, columnMod.Entry);
            Assert.Equal("Value", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [Fact]
        public void BatchCommands_creates_valid_batch_for_modified_entities()
        {
            var stateManager = CreateContextServices(CreateSimpleFKModel()).GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });

            entry.SetEntityState(EntityState.Modified);
            entry.SetPropertyModified(entry.EntityType.FindPrimaryKey().Properties.Single(), isModified: false);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count());

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Modified, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Id", columnMod.ColumnName);
            Assert.Same(entry, columnMod.Entry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Value", columnMod.ColumnName);
            Assert.Same(entry, columnMod.Entry);
            Assert.Equal("Value", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [Fact]
        public void BatchCommands_creates_valid_batch_for_deleted_entities()
        {
            var stateManager = CreateContextServices(CreateSimpleFKModel()).GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });

            entry.SetEntityState(EntityState.Deleted);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }).ToArray();
            Assert.Equal(1, commandBatches.Count());
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count());

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Deleted, command.EntityState);
            Assert.Equal(1, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Id", columnMod.ColumnName);
            Assert.Same(entry, columnMod.Entry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);
        }

        [Fact]
        public void BatchCommands_sorts_related_added_entities()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });
            entry.SetEntityState(EntityState.Added);

            var relatedentry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 42 });
            relatedentry.SetEntityState(EntityState.Added);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedentry, entry }).ToArray();

            Assert.Equal(
                new[] { entry, relatedentry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [Fact]
        public void BatchCommands_sorts_added_and_related_modified_entities()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();
            var model = configuration.GetRequiredService<IModel>();

            var entry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });
            entry.SetEntityState(EntityState.Added);

            var relatedentry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 42 });
            relatedentry.SetEntityState(EntityState.Modified);
            relatedentry.SetPropertyModified(relatedentry.EntityType.FindProperty(nameof(RelatedFakeEntity.RelatedId)));

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedentry, entry }).ToArray();

            Assert.Equal(
                new[] { entry, relatedentry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [Fact]
        public void BatchCommands_sorts_unrelated_entities()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var firstentry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });
            firstentry.SetEntityState(EntityState.Added);

            var secondentry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 1 });
            secondentry.SetEntityState(EntityState.Added);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { secondentry, firstentry }).ToArray();

            Assert.Equal(
                new[] { firstentry, secondentry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [Fact]
        public void BatchCommands_sorts_entities_when_reparenting()
        {
            var configuration = CreateContextServices(CreateCyclicFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var previousParent = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });
            previousParent.SetEntityState(EntityState.Deleted);

            var newParent = stateManager.GetOrCreateEntry(new FakeEntity { Id = 3, Value = "Test" });
            newParent.SetEntityState(EntityState.Added);

            var relatedEntry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 1, RelatedId = 3 });
            relatedEntry.SetEntityState(EntityState.Modified);
            relatedEntry.SetOriginalValue(relatedEntry.EntityType.FindProperty("RelatedId"), 42);
            relatedEntry.SetPropertyModified(relatedEntry.EntityType.FindPrimaryKey().Properties.Single(), isModified: false);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedEntry, previousParent, newParent }).ToArray();

            Assert.Equal(
                new[] { newParent, relatedEntry, previousParent },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [Fact]
        public void BatchCommands_sorts_when_reassigning_child()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var parentEntity = stateManager.GetOrCreateEntry(new FakeEntity { Id = 1, Value = "Test" });
            parentEntity.SetEntityState(EntityState.Unchanged);

            var previousChild = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 42, RelatedId = 1 });
            previousChild.SetEntityState(EntityState.Deleted);

            var newChild = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 23, RelatedId = 1 });
            newChild.SetEntityState(EntityState.Added);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { newChild, previousChild }).ToArray();

            Assert.Equal(
                new[] { previousChild, newChild },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [Fact]
        public void BatchCommands_sorts_entities_while_reassigning_child_tree()
        {
            var configuration = CreateContextServices(CreateTwoLevelFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var parentEntity = stateManager.GetOrCreateEntry(new FakeEntity { Id = 1, Value = "Test" });
            parentEntity.SetEntityState(EntityState.Unchanged);

            var oldEntity = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 2, RelatedId = 1 });
            oldEntity.SetEntityState(EntityState.Deleted);

            var oldChildEntity = stateManager.GetOrCreateEntry(new AnotherFakeEntity { Id = 3, AnotherId = 2 });
            oldChildEntity.SetEntityState(EntityState.Deleted);

            var newEntity = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 4, RelatedId = 1 });
            newEntity.SetEntityState(EntityState.Added);

            var newChildEntity = stateManager.GetOrCreateEntry(new AnotherFakeEntity { Id = 5, AnotherId = 4 });
            newChildEntity.SetEntityState(EntityState.Added);

            var sortedEntities = CreateCommandBatchPreparer()
                .BatchCommands(new[] { newEntity, newChildEntity, oldEntity, oldChildEntity })
                .Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()).ToArray();

            Assert.Equal(
                new[] { oldChildEntity, oldEntity, newEntity, newChildEntity },
                sortedEntities);
        }

        [Fact]
        public void BatchCommands_creates_batches_lazily()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntity = new FakeEntity { Id = 42, Value = "Test" };
            var entry = stateManager.GetOrCreateEntry(fakeEntity);
            entry.SetEntityState(EntityState.Added);

            var relatedentry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 42 });
            relatedentry.SetEntityState(EntityState.Added);

            var modificationCommandBatchFactoryMock = new Mock<IModificationCommandBatchFactory>();
            modificationCommandBatchFactoryMock.Setup(f => f.Create()).Returns(Mock.Of<ModificationCommandBatch>());

            var commandBatches = CreateCommandBatchPreparer(modificationCommandBatchFactoryMock.Object).BatchCommands(new[] { relatedentry, entry });

            var commandBatchesEnumerator = commandBatches.GetEnumerator();
            commandBatchesEnumerator.MoveNext();

            modificationCommandBatchFactoryMock.Verify(
                mcb => mcb.Create(),
                Times.Once);

            commandBatchesEnumerator.MoveNext();

            modificationCommandBatchFactoryMock.Verify(
                mcb => mcb.Create(),
                Times.Exactly(2));
        }

        [Fact]
        public void Batch_command_does_not_order_non_unique_index_values()
        {
            var model = CreateCyclicFKModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });
            fakeEntry.SetEntityState(EntityState.Added);

            var relatedFakeEntry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 1, RelatedId = 42 });
            relatedFakeEntry.SetEntityState(EntityState.Added);

            var fakeEntry2 = stateManager.GetOrCreateEntry(new FakeEntity { Id = 2, RelatedId = 1, Value = "Test2" });
            fakeEntry2.SetEntityState(EntityState.Modified);
            fakeEntry2.SetOriginalValue(fakeEntry2.EntityType.FindProperty(nameof(FakeEntity.Value)), "Test");
            fakeEntry2.SetPropertyModified(fakeEntry2.EntityType.FindPrimaryKey().Properties.Single(), isModified: false);

            var sortedEntities = CreateCommandBatchPreparer()
                .BatchCommands(new[] { fakeEntry, fakeEntry2, relatedFakeEntry })
                .Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()).ToArray();

            Assert.Equal(
                new[] { fakeEntry, relatedFakeEntry, fakeEntry2 },
                sortedEntities);
        }

        [Fact]
        public void BatchCommands_throws_on_non_store_generated_temporary_values()
        {
            var configuration = CreateContextServices(CreateTwoLevelFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 1, Value = "Test" });
            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(entry.EntityType.FindProperty(nameof(FakeEntity.Value)));

            Assert.Equal(
                CoreStrings.TempValue(nameof(FakeEntity.Value), nameof(FakeEntity)),
                Assert.Throws<InvalidOperationException>(() => CreateCommandBatchPreparer().BatchCommands(new[] { entry }).ToList()).Message);
        }

        [Fact]
        public void Batch_command_throws_on_commands_with_circular_dependencies()
        {
            var model = CreateCyclicFKModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, RelatedId = 1 });
            fakeEntry.SetEntityState(EntityState.Added);

            var relatedFakeEntry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 1, RelatedId = 42 });
            relatedFakeEntry.SetEntityState(EntityState.Added);

            Assert.Equal(
                CoreStrings.CircularDependency(
                    string.Join(", ",
                        model.FindEntityType(typeof(RelatedFakeEntity)).GetForeignKeys().First(),
                        model.FindEntityType(typeof(FakeEntity)).GetForeignKeys().First())),
                Assert.Throws<InvalidOperationException>(
                    () => CreateCommandBatchPreparer().BatchCommands(new[] { fakeEntry, relatedFakeEntry }).ToArray()).Message);
        }

        [Fact]
        public void Batch_command_throws_on_commands_with_circular_dependencies_including_indexes()
        {
            var model = CreateCyclicFKModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, UniqueValue = "Test" });
            fakeEntry.SetEntityState(EntityState.Added);

            var relatedFakeEntry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 1, RelatedId = 42 });
            relatedFakeEntry.SetEntityState(EntityState.Added);

            var fakeEntry2 = stateManager.GetOrCreateEntry(new FakeEntity { Id = 2, RelatedId = 1, UniqueValue = "Test2" });
            fakeEntry2.SetEntityState(EntityState.Modified);
            fakeEntry2.SetOriginalValue(fakeEntry2.EntityType.FindProperty(nameof(FakeEntity.UniqueValue)), "Test");
            fakeEntry2.SetPropertyModified(fakeEntry2.EntityType.FindPrimaryKey().Properties.Single(), isModified: false);

            Assert.Equal(
                CoreStrings.CircularDependency(
                    string.Join(", ",
                        model.FindEntityType(typeof(RelatedFakeEntity)).GetForeignKeys().Single(),
                        model.FindEntityType(typeof(FakeEntity)).GetForeignKeys().Single(),
                        model.FindEntityType(typeof(FakeEntity)).GetIndexes().
                            Single(i => i.Properties.Any(p => p.Name == nameof(FakeEntity.UniqueValue))))),
                Assert.Throws<InvalidOperationException>(
                    () => CreateCommandBatchPreparer().BatchCommands(new[] { fakeEntry, relatedFakeEntry, fakeEntry2 }).ToArray()).Message);
        }

        [Fact]
        public void Batch_command_shows_correct_cycle_when_circular_dependencies()
        {
            var model = CreateCyclicFkWithTailModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 1, RelatedId = 2 });
            fakeEntry.SetEntityState(EntityState.Added);

            var relatedFakeEntry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 2, RelatedId = 1 });
            relatedFakeEntry.SetEntityState(EntityState.Added);

            var anotherFakeEntry = stateManager.GetOrCreateEntry(new AnotherFakeEntity { Id = 3, AnotherId = 2 });
            anotherFakeEntry.SetEntityState(EntityState.Added);

            Assert.Equal(
                CoreStrings.CircularDependency(
                    string.Join(", ",
                        model.FindEntityType(typeof(FakeEntity)).GetForeignKeys().First(),
                        model.FindEntityType(typeof(RelatedFakeEntity)).GetForeignKeys().First())),
                Assert.Throws<InvalidOperationException>(
                    () => CreateCommandBatchPreparer().BatchCommands(
                        // Order is important for this test. Entry which is not part of cycle but tail should come first.
                        new[] { anotherFakeEntry, fakeEntry, relatedFakeEntry }).ToArray()).Message);
        }

        private static IServiceProvider CreateContextServices(IModel model)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model);
            optionsBuilder.UseTransientInMemoryDatabase();

            return new DbContext(optionsBuilder.Options).GetInfrastructure<IServiceProvider>();
        }

        private static ICommandBatchPreparer CreateCommandBatchPreparer(IModificationCommandBatchFactory modificationCommandBatchFactory = null)
        {
            modificationCommandBatchFactory =
                modificationCommandBatchFactory ?? new TestModificationCommandBatchFactory(
                    Mock.Of<IRelationalCommandBuilderFactory>(),
                    Mock.Of<ISqlGenerationHelper>(),
                    Mock.Of<IUpdateSqlGenerator>(),
                    Mock.Of<IRelationalValueBufferFactoryFactory>());

            return new CommandBatchPreparer(modificationCommandBatchFactory,
                new ParameterNameGeneratorFactory(new ParameterNameGeneratorDependencies()),
                new ModificationCommandComparer(),
                new TestAnnotationProvider(),
                new KeyValueIndexFactorySource());
        }

        private static IModel CreateSimpleFKModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>(b =>
                {
                    b.Ignore(c => c.UniqueValue);
                    b.Ignore(c => c.RelatedId);
                });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.HasOne<FakeEntity>()
                        .WithOne()
                        .HasForeignKey<RelatedFakeEntity>(c => c.Id);
                });

            return modelBuilder.Model;
        }

        private static IModel CreateCyclicFKModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>(b =>
            {
                b.HasIndex(c => c.Value);
                b.HasIndex(c => c.UniqueValue).IsUnique();
            });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.HasOne<FakeEntity>()
                        .WithOne()
                        .HasForeignKey<RelatedFakeEntity>(c => c.RelatedId);
                });

            modelBuilder
                .Entity<FakeEntity>()
                .HasOne<RelatedFakeEntity>()
                .WithOne()
                .HasForeignKey<FakeEntity>(c => c.RelatedId);

            return modelBuilder.Model;
        }

        private static IModel CreateCyclicFkWithTailModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>(b =>
                {
                    b.HasIndex(c => c.Value);
                    b.HasIndex(c => c.UniqueValue).IsUnique();
                });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.HasOne<FakeEntity>()
                        .WithOne()
                        .HasForeignKey<RelatedFakeEntity>(c => c.RelatedId);
                });

            modelBuilder
                .Entity<FakeEntity>()
                .HasOne<RelatedFakeEntity>()
                .WithOne()
                .HasForeignKey<FakeEntity>(c => c.RelatedId);

            modelBuilder.Entity<AnotherFakeEntity>(b =>
                {
                    b.HasOne<RelatedFakeEntity>()
                        .WithOne()
                        .HasForeignKey<AnotherFakeEntity>(e => e.AnotherId);
                });

            return modelBuilder.Model;
        }

        private static IModel CreateTwoLevelFKModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>();

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.HasOne<FakeEntity>()
                        .WithOne()
                        .HasForeignKey<RelatedFakeEntity>(c => c.RelatedId);
                });

            modelBuilder.Entity<AnotherFakeEntity>(b =>
                {
                    b.HasOne<RelatedFakeEntity>()
                        .WithOne()
                        .HasForeignKey<AnotherFakeEntity>(c => c.AnotherId);
                });

            return modelBuilder.Model;
        }

        private class FakeEntity
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public string UniqueValue { get; set; }
            public int? RelatedId { get; set; }
        }

        private class RelatedFakeEntity
        {
            public int Id { get; set; }
            public int? RelatedId { get; set; }
        }

        private class AnotherFakeEntity
        {
            public int Id { get; set; }
            public int? AnotherId { get; set; }
        }

        private class TestModificationCommandBatchFactory : IModificationCommandBatchFactory
        {
            private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
            private readonly ISqlGenerationHelper _sqlGenerationHelper;
            private readonly IUpdateSqlGenerator _updateSqlGenerator;
            private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;

            public TestModificationCommandBatchFactory(
                IRelationalCommandBuilderFactory commandBuilderfactory,
                ISqlGenerationHelper sqlGenerationHelper,
                IUpdateSqlGenerator updateSqlGenerator,
                IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
            {
                _commandBuilderFactory = commandBuilderfactory;
                _sqlGenerationHelper = sqlGenerationHelper;
                _updateSqlGenerator = updateSqlGenerator;
                _valueBufferFactoryFactory = valueBufferFactoryFactory;
            }

            public ModificationCommandBatch Create()
                => new SingularModificationCommandBatch(
                    _commandBuilderFactory,
                    _sqlGenerationHelper,
                    _updateSqlGenerator,
                    _valueBufferFactoryFactory);
        }
    }
}
