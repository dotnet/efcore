// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update
{
    public class CommandBatchPreparerTest
    {
        [ConditionalFact]
        public void BatchCommands_creates_valid_batch_for_added_entities()
        {
            var stateManager = CreateContextServices(CreateSimpleFKModel()).GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, Value = "Test" });

            entry.SetEntityState(EntityState.Added);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }, modelData).ToArray();
            Assert.Single(commandBatches);
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count);

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

        [ConditionalFact]
        public void BatchCommands_creates_valid_batch_for_modified_entities()
        {
            var stateManager = CreateContextServices(CreateSimpleFKModel()).GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, Value = "Test" });

            entry.SetEntityState(EntityState.Modified);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }, modelData).ToArray();
            Assert.Single(commandBatches);
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count);

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

        [ConditionalFact]
        public void BatchCommands_creates_valid_batch_for_deleted_entities()
        {
            var stateManager = CreateContextServices(CreateSimpleFKModel()).GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, Value = "Test" });

            entry.SetEntityState(EntityState.Deleted);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }, modelData).ToArray();
            Assert.Single(commandBatches);
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count);

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

        [ConditionalFact]
        public void BatchCommands_sorts_related_added_entities()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, Value = "Test" });
            entry.SetEntityState(EntityState.Added);

            var modelData = new UpdateAdapter(stateManager);

            var relatedEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 42 });
            relatedEntry.SetEntityState(EntityState.Added);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedEntry, entry }, modelData).ToArray();

            Assert.Equal(
                new[] { entry, relatedEntry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [ConditionalFact]
        public void BatchCommands_sorts_added_and_related_modified_entities()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, Value = "Test" });
            entry.SetEntityState(EntityState.Added);

            var modelData = new UpdateAdapter(stateManager);

            var relatedEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 42 });
            relatedEntry.SetEntityState(EntityState.Modified);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedEntry, entry }, modelData).ToArray();

            Assert.Equal(
                new[] { entry, relatedEntry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [ConditionalFact]
        public void BatchCommands_sorts_unrelated_entities()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var firstEntry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, Value = "Test" });
            firstEntry.SetEntityState(EntityState.Added);

            var secondEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 1 });
            secondEntry.SetEntityState(EntityState.Added);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { secondEntry, firstEntry }, modelData).ToArray();

            Assert.Equal(
                new[] { firstEntry, secondEntry },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [ConditionalFact]
        public void BatchCommands_sorts_entities_when_reparenting()
        {
            var configuration = CreateContextServices(CreateCyclicFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var previousParent = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, Value = "Test" });
            previousParent.SetEntityState(EntityState.Deleted);

            var newParent = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 3, Value = "Test" });
            newParent.SetEntityState(EntityState.Added);

            var relatedEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 1, RelatedId = 3 });
            relatedEntry.SetEntityState(EntityState.Modified);
            relatedEntry.SetOriginalValue(relatedEntry.EntityType.FindProperty("RelatedId"), 42);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedEntry, previousParent, newParent }, modelData)
                .ToArray();

            Assert.Equal(
                new[] { newParent, relatedEntry, previousParent },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [ConditionalFact]
        public void BatchCommands_sorts_when_reassigning_child()
        {
            var configuration = CreateContextServices(CreateSimpleFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var parentEntity = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 1, Value = "Test" });
            parentEntity.SetEntityState(EntityState.Unchanged);

            var previousChild = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 42, RelatedId = 1 });
            previousChild.SetEntityState(EntityState.Deleted);

            var newChild = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 23, RelatedId = 1 });
            newChild.SetEntityState(EntityState.Added);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { newChild, previousChild }, modelData).ToArray();

            Assert.Equal(
                new[] { previousChild, newChild },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
        }

        [ConditionalFact]
        public void BatchCommands_sorts_entities_while_reassigning_child_tree()
        {
            var configuration = CreateContextServices(CreateTwoLevelFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var parentEntity = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 1, Value = "Test" });
            parentEntity.SetEntityState(EntityState.Unchanged);

            var oldEntity = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 2, RelatedId = 1 });
            oldEntity.SetEntityState(EntityState.Deleted);

            var oldChildEntity = stateManager.GetOrCreateEntry(
                new AnotherFakeEntity { Id = 3, AnotherId = 2 });
            oldChildEntity.SetEntityState(EntityState.Deleted);

            var newEntity = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 4, RelatedId = 1 });
            newEntity.SetEntityState(EntityState.Added);

            var newChildEntity = stateManager.GetOrCreateEntry(
                new AnotherFakeEntity { Id = 5, AnotherId = 4 });
            newChildEntity.SetEntityState(EntityState.Added);

            var modelData = new UpdateAdapter(stateManager);

            var sortedEntities = CreateCommandBatchPreparer()
                .BatchCommands(new[] { newEntity, newChildEntity, oldEntity, oldChildEntity }, modelData)
                .Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()).ToArray();

            Assert.Equal(
                new IUpdateEntry[] { oldChildEntity, oldEntity, newEntity, newChildEntity },
                sortedEntities);
        }

        [ConditionalFact]
        public void BatchCommands_creates_batches_lazily()
        {
            var configuration = RelationalTestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IModificationCommandBatchFactory, TestModificationCommandBatchFactory>(),
                CreateSimpleFKModel());

            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntity = new FakeEntity { Id = 42, Value = "Test" };
            var entry = stateManager.GetOrCreateEntry(fakeEntity);
            entry.SetEntityState(EntityState.Added);

            var relatedEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 42 });
            relatedEntry.SetEntityState(EntityState.Added);

            var factory = (TestModificationCommandBatchFactory)configuration.GetService<IModificationCommandBatchFactory>();

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer(factory).BatchCommands(new[] { relatedEntry, entry }, modelData);

            using var commandBatchesEnumerator = commandBatches.GetEnumerator();
            commandBatchesEnumerator.MoveNext();

            Assert.Equal(1, factory.CreateCount);

            commandBatchesEnumerator.MoveNext();

            Assert.Equal(2, factory.CreateCount);
        }

        [ConditionalFact]
        public void Batch_command_does_not_order_non_unique_index_values()
        {
            var model = CreateCyclicFKModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, Value = "Test" });
            fakeEntry.SetEntityState(EntityState.Added);

            var relatedFakeEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 1, RelatedId = 42 });
            relatedFakeEntry.SetEntityState(EntityState.Added);

            var fakeEntry2 = stateManager.GetOrCreateEntry(
                new FakeEntity
                {
                    Id = 2,
                    RelatedId = 1,
                    Value = "Test2"
                });
            fakeEntry2.SetEntityState(EntityState.Modified);
            fakeEntry2.SetOriginalValue(fakeEntry2.EntityType.FindProperty(nameof(FakeEntity.Value)), "Test");

            var modelData = new UpdateAdapter(stateManager);

            var sortedEntities = CreateCommandBatchPreparer()
                .BatchCommands(new[] { fakeEntry, fakeEntry2, relatedFakeEntry }, modelData)
                .Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()).ToArray();

            Assert.Equal(
                new IUpdateEntry[] { fakeEntry, relatedFakeEntry, fakeEntry2 },
                sortedEntities);
        }

        [ConditionalFact]
        public void BatchCommands_throws_on_non_store_generated_temporary_values()
        {
            var configuration = CreateContextServices(CreateTwoLevelFKModel());
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 1, Value = "Test" });
            entry.SetEntityState(EntityState.Added);

            Assert.Equal(
                CoreStrings.TempValue(nameof(FakeEntity.Value), nameof(FakeEntity)),
                Assert.Throws<InvalidOperationException>(
                    () => entry.SetTemporaryValue(entry.EntityType.FindProperty(nameof(FakeEntity.Value)), "Test")).Message);
        }

        [InlineData(true)]
        [InlineData(false)]
        [ConditionalTheory]
        public void Batch_command_throws_on_commands_with_circular_dependencies(bool sensitiveLogging)
        {
            var model = CreateCyclicFKModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, RelatedId = 1 });
            fakeEntry.SetEntityState(EntityState.Added);

            var relatedFakeEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 1, RelatedId = 42 });
            relatedFakeEntry.SetEntityState(EntityState.Added);

            var modelData = new UpdateAdapter(stateManager);

            var expectedCycle = sensitiveLogging
                ? @"FakeEntity { 'Id': 42 } [Added] <-
ForeignKey { 'RelatedId': 42 } RelatedFakeEntity { 'Id': 1 } [Added] <-
ForeignKey { 'RelatedId': 1 } FakeEntity { 'Id': 42 } [Added]"
                : @"FakeEntity [Added] <-
ForeignKey { 'RelatedId' } RelatedFakeEntity [Added] <-
ForeignKey { 'RelatedId' } FakeEntity [Added]" + CoreStrings.SensitiveDataDisabled;
 
            Assert.Equal(
                CoreStrings.CircularDependency(ListLoggerFactory.NormalizeLineEndings(expectedCycle)),
                Assert.Throws<InvalidOperationException>(
                    () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: sensitiveLogging)
                        .BatchCommands(new[] { fakeEntry, relatedFakeEntry }, modelData).ToArray()).Message);
        }

        [InlineData(true)]
        [InlineData(false)]
        [ConditionalTheory]
        public void Batch_command_throws_on_commands_with_circular_dependencies_including_indexes(bool sensitiveLogging)
        {
            var model = CreateCyclicFKModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 42, UniqueValue = "Test" });
            fakeEntry.SetEntityState(EntityState.Added);

            var relatedFakeEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 1, RelatedId = 42 });
            relatedFakeEntry.SetEntityState(EntityState.Added);

            var fakeEntry2 = stateManager.GetOrCreateEntry(
                new FakeEntity
                {
                    Id = 2,
                    RelatedId = 1,
                    UniqueValue = "Test2"
                });
            fakeEntry2.SetEntityState(EntityState.Modified);
            fakeEntry2.SetOriginalValue(fakeEntry2.EntityType.FindProperty(nameof(FakeEntity.UniqueValue)), "Test");

            var modelData = new UpdateAdapter(stateManager);

            var expectedCycle = sensitiveLogging
                ? @"FakeEntity { 'Id': 42 } [Added] <-
ForeignKey { 'RelatedId': 42 } RelatedFakeEntity { 'Id': 1 } [Added] <-
ForeignKey { 'RelatedId': 1 } FakeEntity { 'Id': 2 } [Modified] <-
Index { 'UniqueValue': Test } FakeEntity { 'Id': 42 } [Added]"
                : @"FakeEntity [Added] <-
ForeignKey { 'RelatedId' } RelatedFakeEntity [Added] <-
ForeignKey { 'RelatedId' } FakeEntity [Modified] <-
Index { 'UniqueValue' } FakeEntity [Added]" + CoreStrings.SensitiveDataDisabled;

            Assert.Equal(
                CoreStrings.CircularDependency(ListLoggerFactory.NormalizeLineEndings(expectedCycle)),
                Assert.Throws<InvalidOperationException>(
                    () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: sensitiveLogging)
                        .BatchCommands(new[] { fakeEntry, relatedFakeEntry, fakeEntry2 }, modelData).ToArray()).Message);
        }

        [InlineData(true)]
        [InlineData(false)]
        [ConditionalTheory]
        public void Batch_command_throws_on_delete_commands_with_circular_dependencies(bool sensitiveLogging)
        {
            var model = CreateCyclicFkWithTailModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 1, RelatedId = 2 });
            fakeEntry.SetEntityState(EntityState.Deleted);

            var relatedFakeEntry = stateManager.GetOrCreateEntry(
                new RelatedFakeEntity { Id = 2, RelatedId = 1 });
            relatedFakeEntry.SetEntityState(EntityState.Deleted);

            var anotherFakeEntry = stateManager.GetOrCreateEntry(
                new AnotherFakeEntity { Id = 3, AnotherId = 2 });
            anotherFakeEntry.SetEntityState(EntityState.Deleted);

            var modelData = new UpdateAdapter(stateManager);

            var expectedCycle = sensitiveLogging
                ? @"FakeEntity { 'Id': 1 } [Deleted] ForeignKey { 'RelatedId': 2 } <-
RelatedFakeEntity { 'Id': 2 } [Deleted] ForeignKey { 'RelatedId': 1 } <-
FakeEntity { 'Id': 1 } [Deleted]"
                : @"FakeEntity [Deleted] ForeignKey { 'RelatedId' } <-
RelatedFakeEntity [Deleted] ForeignKey { 'RelatedId' } <-
FakeEntity [Deleted]" + CoreStrings.SensitiveDataDisabled;

            Assert.Equal(
                CoreStrings.CircularDependency(ListLoggerFactory.NormalizeLineEndings(expectedCycle)),
                Assert.Throws<InvalidOperationException>(
                    () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: sensitiveLogging).BatchCommands(
                        // Order is important for this test. Entry which is not part of cycle but tail should come first.
                        new[] { anotherFakeEntry, fakeEntry, relatedFakeEntry }, modelData).ToArray()).Message);
        }

        [ConditionalFact]
        public void BatchCommands_works_with_duplicate_values_for_unique_indexes()
        {
            var model = CreateCyclicFKModel();
            var configuration = CreateContextServices(model);
            var stateManager = configuration.GetRequiredService<IStateManager>();

            var fakeEntry = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 1, UniqueValue = "Test" });
            fakeEntry.SetEntityState(EntityState.Deleted);

            var fakeEntry2 = stateManager.GetOrCreateEntry(
                new FakeEntity { Id = 2, UniqueValue = "Test2" });
            fakeEntry2.SetEntityState(EntityState.Modified);
            fakeEntry2.SetOriginalValue(fakeEntry.EntityType.FindProperty(nameof(FakeEntity.UniqueValue)), "Test");

            var modelData = new UpdateAdapter(stateManager);

            var batches = CreateCommandBatchPreparer(updateAdapter: modelData)
                .BatchCommands(new[] { fakeEntry, fakeEntry2 }, modelData).ToArray();

            Assert.Equal(2, batches.Length);
        }

        [ConditionalFact]
        public void BatchCommands_creates_valid_batch_for_shared_table_added_entities()
        {
            var currentDbContext = CreateContextServices(CreateSharedTableModel()).GetRequiredService<ICurrentDbContext>();
            var stateManager = currentDbContext.GetDependencies().StateManager;

            var first = new FakeEntity { Id = 42, Value = "Test" };
            var firstEntry = stateManager.GetOrCreateEntry(first);
            firstEntry.SetEntityState(EntityState.Added);
            var second = new RelatedFakeEntity { Id = 42 };
            var secondEntry = stateManager.GetOrCreateEntry(second);
            secondEntry.SetEntityState(EntityState.Added);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer(updateAdapter: modelData)
                .BatchCommands(new[] { firstEntry, secondEntry }, modelData)
                .ToArray();
            Assert.Single(commandBatches);
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count);

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Added, command.EntityState);
            Assert.Equal(4, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal(nameof(FakeEntity.Id), columnMod.ColumnName);
            Assert.Equal(first.Id, columnMod.Value);
            Assert.Equal(first.Id, columnMod.OriginalValue);
            Assert.False(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal(nameof(FakeEntity.RelatedId), columnMod.ColumnName);
            Assert.Equal(first.RelatedId, columnMod.Value);
            Assert.Equal(first.RelatedId, columnMod.OriginalValue);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[2];

            Assert.Equal(nameof(FakeEntity.Value), columnMod.ColumnName);
            Assert.Equal(first.Value, columnMod.Value);
            Assert.Equal(first.Value, columnMod.OriginalValue);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [ConditionalFact]
        public void BatchCommands_creates_valid_batch_for_shared_table_modified_entities()
        {
            var currentDbContext = CreateContextServices(CreateSharedTableModel()).GetRequiredService<ICurrentDbContext>();
            var stateManager = currentDbContext.GetDependencies().StateManager;

            var entity = new FakeEntity { Id = 42, Value = "Null" };
            var entry = stateManager.GetOrCreateEntry(entity);

            entry.SetEntityState(EntityState.Modified);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer(updateAdapter: modelData)
                .BatchCommands(new[] { entry }, modelData)
                .ToArray();

            Assert.Single(commandBatches);
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count);

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Modified, command.EntityState);
            Assert.Equal(3, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal(nameof(FakeEntity.Id), columnMod.ColumnName);
            Assert.Equal(entity.Id, columnMod.Value);
            Assert.Equal(entity.Id, columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal(nameof(FakeEntity.RelatedId), columnMod.ColumnName);
            Assert.Equal(entity.RelatedId, columnMod.Value);
            Assert.Equal(entity.RelatedId, columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[2];

            Assert.Equal(nameof(FakeEntity.Value), columnMod.ColumnName);
            Assert.Equal(entity.Value, columnMod.Value);
            Assert.Equal(entity.Value, columnMod.OriginalValue);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [ConditionalFact]
        public void BatchCommands_creates_valid_batch_for_shared_table_deleted_entities()
        {
            var currentDbContext = CreateContextServices(CreateSharedTableModel()).GetRequiredService<ICurrentDbContext>();
            var stateManager = currentDbContext.GetDependencies().StateManager;

            var first = new FakeEntity { Id = 42, Value = "Test" };
            var firstEntry = stateManager.GetOrCreateEntry(first);
            firstEntry.SetEntityState(EntityState.Deleted);
            var second = new RelatedFakeEntity { Id = 42 };
            var secondEntry = stateManager.GetOrCreateEntry(second);
            secondEntry.SetEntityState(EntityState.Deleted);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer(updateAdapter: modelData)
                .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray();

            Assert.Single(commandBatches);
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count);

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Deleted, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal(nameof(FakeEntity.Id), columnMod.ColumnName);
            Assert.False(columnMod.UseCurrentValueParameter);
            Assert.True(columnMod.UseOriginalValueParameter);
            Assert.Equal(first.Id, columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal(nameof(FakeEntity.RelatedId), columnMod.ColumnName);
            Assert.Equal(first.RelatedId, columnMod.Value);
            Assert.Equal(first.RelatedId, columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);
        }

        [InlineData(true)]
        [InlineData(false)]
        [ConditionalTheory]
        public void BatchCommands_throws_on_conflicting_updates_for_shared_table_added_entities(bool sensitiveLogging)
        {
            var currentDbContext = CreateContextServices(CreateSharedTableModel()).GetRequiredService<ICurrentDbContext>();
            var stateManager = currentDbContext.GetDependencies().StateManager;

            var first = new FakeEntity { Id = 42, Value = "Test" };
            var firstEntry = stateManager.GetOrCreateEntry(first);
            firstEntry.SetEntityState(EntityState.Added);
            var second = new RelatedFakeEntity { Id = 42 };
            var secondEntry = stateManager.GetOrCreateEntry(second);
            secondEntry.SetEntityState(EntityState.Deleted);

            var modelData = new UpdateAdapter(stateManager);

            if (sensitiveLogging)
            {
                Assert.Equal(
                    RelationalStrings.ConflictingRowUpdateTypesSensitive(
                        nameof(RelatedFakeEntity), "{Id: 42}", EntityState.Deleted,
                        nameof(FakeEntity), "{Id: 42}", EntityState.Added),
                    Assert.Throws<InvalidOperationException>(
                        () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: true)
                            .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray()).Message);
            }
            else
            {
                Assert.Equal(
                    RelationalStrings.ConflictingRowUpdateTypes(
                        nameof(RelatedFakeEntity), EntityState.Deleted,
                        nameof(FakeEntity), EntityState.Added),
                    Assert.Throws<InvalidOperationException>(
                        () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: false)
                            .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray()).Message);
            }
        }

        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [ConditionalTheory]
        public void BatchCommands_throws_on_conflicting_values_for_shared_table_added_entities(bool useCurrentValues, bool sensitiveLogging)
        {
            var currentDbContext = CreateContextServices(CreateSharedTableModel()).GetRequiredService<ICurrentDbContext>();
            var stateManager = currentDbContext.GetDependencies().StateManager;

            var first = new FakeEntity { Id = 42, Value = "Test" };
            var firstEntry = stateManager.GetOrCreateEntry(first);
            firstEntry.SetEntityState(EntityState.Modified);
            var second = new RelatedFakeEntity { Id = 42 };
            var secondEntry = stateManager.GetOrCreateEntry(second);
            secondEntry.SetEntityState(EntityState.Modified);

            if (useCurrentValues)
            {
                first.RelatedId = 1;
                second.RelatedId = 2;
            }
            else
            {
                new EntityEntry<FakeEntity>(firstEntry).Property(e => e.RelatedId).OriginalValue = 1;
                new EntityEntry<RelatedFakeEntity>(secondEntry).Property(e => e.RelatedId).OriginalValue = 2;
            }

            var modelData = new UpdateAdapter(stateManager);

            if (useCurrentValues)
            {
                if (sensitiveLogging)
                {
                    Assert.Equal(
                        RelationalStrings.ConflictingRowValuesSensitive(
                            nameof(FakeEntity), nameof(RelatedFakeEntity),
                            "{Id: 42}", "{RelatedId: 1}", "{RelatedId: 2}", "RelatedId"),
                        Assert.Throws<InvalidOperationException>(
                            () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: true)
                                .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray()).Message);
                }
                else
                {
                    Assert.Equal(
                        RelationalStrings.ConflictingRowValues(
                            nameof(FakeEntity), nameof(RelatedFakeEntity),
                            "{'RelatedId'}", "{'RelatedId'}", "RelatedId"),
                        Assert.Throws<InvalidOperationException>(
                            () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: false)
                                .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray()).Message);
                }
            }
            else
            {
                if (sensitiveLogging)
                {
                    Assert.Equal(
                        RelationalStrings.ConflictingOriginalRowValuesSensitive(
                            nameof(FakeEntity), nameof(RelatedFakeEntity),
                            "{Id: 42}", "{RelatedId: 1}", "{RelatedId: 2}", "RelatedId"),
                        Assert.Throws<InvalidOperationException>(
                            () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: true)
                                .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray()).Message);
                }
                else
                {
                    Assert.Equal(
                        RelationalStrings.ConflictingOriginalRowValues(
                            nameof(FakeEntity), nameof(RelatedFakeEntity),
                            "{'RelatedId'}", "{'RelatedId'}", "RelatedId"),
                        Assert.Throws<InvalidOperationException>(
                            () => CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: false)
                                .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray()).Message);
                }
            }
        }

        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Deleted)]
        [ConditionalTheory(Skip = "Issue #17947")]
        public void BatchCommands_creates_batch_on_incomplete_updates_for_shared_table_no_principal(EntityState state)
        {
            var currentDbContext = CreateContextServices(CreateSharedTableModel()).GetRequiredService<ICurrentDbContext>();
            var stateManager = currentDbContext.GetDependencies().StateManager;

            var first = new DerivedRelatedFakeEntity { Id = 42 };
            var firstEntry = stateManager.GetOrCreateEntry(first);
            firstEntry.SetEntityState(state);

            var second = new AnotherFakeEntity { Id = 42 };
            var secondEntry = stateManager.GetOrCreateEntry(second);
            secondEntry.SetEntityState(state);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: true)
                .BatchCommands(new[] { firstEntry }, modelData).ToArray();

            Assert.Single(commandBatches);
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count);

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Modified, command.EntityState);
            Assert.Equal(4, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal(nameof(DerivedRelatedFakeEntity.Id), columnMod.ColumnName);
            Assert.True(columnMod.UseOriginalValueParameter);
            Assert.False(columnMod.UseCurrentValueParameter);
            Assert.Equal(first.Id, columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Discriminator", columnMod.ColumnName);
            Assert.Equal(nameof(DerivedRelatedFakeEntity), columnMod.Value);
            Assert.Equal(nameof(DerivedRelatedFakeEntity), columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[2];

            Assert.Equal(nameof(DerivedRelatedFakeEntity.RelatedId), columnMod.ColumnName);
            Assert.Equal(first.RelatedId, columnMod.Value);
            Assert.Equal(first.RelatedId, columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[3];

            Assert.Equal(nameof(AnotherFakeEntity.AnotherId), columnMod.ColumnName);
            Assert.Equal(second.AnotherId, columnMod.Value);
            Assert.Equal(second.AnotherId, columnMod.OriginalValue);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Deleted)]
        [ConditionalTheory]
        public void BatchCommands_works_with_incomplete_updates_for_shared_table_no_leaf_dependent(EntityState state)
        {
            var currentDbContext = CreateContextServices(CreateSharedTableModel()).GetRequiredService<ICurrentDbContext>();
            var stateManager = currentDbContext.GetDependencies().StateManager;

            var first = new FakeEntity { Id = 42 };
            var firstEntry = stateManager.GetOrCreateEntry(first);
            firstEntry.SetEntityState(state);

            var second = new DerivedRelatedFakeEntity { Id = 42 };
            var secondEntry = stateManager.GetOrCreateEntry(second);
            secondEntry.SetEntityState(state);

            var modelData = new UpdateAdapter(stateManager);

            var batches = CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: false)
                .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray();

            Assert.Single(batches);
        }

        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Deleted)]
        [ConditionalTheory(Skip = "Issue #17947")]
        public void BatchCommands_creates_batch_on_incomplete_updates_for_shared_table_no_middle_dependent(EntityState state)
        {
            var currentDbContext = CreateContextServices(CreateSharedTableModel()).GetRequiredService<ICurrentDbContext>();
            var stateManager = currentDbContext.GetDependencies().StateManager;

            var first = new FakeEntity { Id = 42 };
            var firstEntry = stateManager.GetOrCreateEntry(first);
            firstEntry.SetEntityState(state);

            var second = new AnotherFakeEntity { Id = 42 };
            var secondEntry = stateManager.GetOrCreateEntry(second);
            secondEntry.SetEntityState(state);

            var modelData = new UpdateAdapter(stateManager);

            var commandBatches = CreateCommandBatchPreparer(updateAdapter: modelData, sensitiveLogging: true)
                .BatchCommands(new[] { firstEntry, secondEntry }, modelData).ToArray();

            Assert.Equal(2, commandBatches.Length);
            Assert.Equal(1, commandBatches.First().ModificationCommands.Count);

            var command = commandBatches.First().ModificationCommands.Single();
            Assert.Equal(EntityState.Modified, command.EntityState);
            Assert.Equal(3, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal(nameof(DerivedRelatedFakeEntity.Id), columnMod.ColumnName);
            Assert.Equal(first.Id, columnMod.Value);
            Assert.Equal(first.Id, columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal(nameof(DerivedRelatedFakeEntity.RelatedId), columnMod.ColumnName);
            Assert.Equal(first.RelatedId, columnMod.Value);
            Assert.Equal(first.RelatedId, columnMod.OriginalValue);
            Assert.True(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[2];

            Assert.Equal(nameof(AnotherFakeEntity.AnotherId), columnMod.ColumnName);
            Assert.Equal(second.AnotherId, columnMod.Value);
            Assert.Equal(second.AnotherId, columnMod.OriginalValue);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        private static IServiceProvider CreateContextServices(IModel model)
            => RelationalTestHelpers.Instance.CreateContextServices(model);

        public ICommandBatchPreparer CreateCommandBatchPreparer(
            IModificationCommandBatchFactory modificationCommandBatchFactory = null,
            IUpdateAdapter updateAdapter = null,
            bool sensitiveLogging = false)
        {
            modificationCommandBatchFactory ??=
                RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<IModificationCommandBatchFactory>();

            var loggingOptions = new LoggingOptions();
            if (sensitiveLogging)
            {
                loggingOptions.Initialize(new DbContextOptionsBuilder<DbContext>().EnableSensitiveDataLogging().Options);
            }

            return new CommandBatchPreparer(
                new CommandBatchPreparerDependencies(
                    modificationCommandBatchFactory,
                    new ParameterNameGeneratorFactory(new ParameterNameGeneratorDependencies()),
                    new ModificationCommandComparer(),
                    new KeyValueIndexFactorySource(),
                    loggingOptions,
                    new FakeDiagnosticsLogger<DbLoggerCategory.Update>(),
                    new DbContextOptionsBuilder().Options));
        }

        private static IModel CreateSimpleFKModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>(
                b =>
                {
                    b.Ignore(c => c.UniqueValue);
                    b.Ignore(c => c.RelatedId);
                });

            modelBuilder.Entity<RelatedFakeEntity>(
                b =>
                {
                    b.HasOne<FakeEntity>()
                        .WithOne()
                        .HasForeignKey<RelatedFakeEntity>(c => c.Id);
                });

            return modelBuilder.Model.FinalizeModel();
        }

        private static IModel CreateCyclicFKModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>(
                b =>
                {
                    b.HasIndex(c => c.Value);
                    b.HasIndex(c => c.UniqueValue).IsUnique();
                });

            modelBuilder.Entity<RelatedFakeEntity>(
                b =>
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

            return modelBuilder.Model.FinalizeModel();
        }

        private static IModel CreateCyclicFkWithTailModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>(
                b =>
                {
                    b.HasIndex(c => c.Value);
                    b.HasIndex(c => c.UniqueValue).IsUnique();
                });

            modelBuilder.Entity<RelatedFakeEntity>(
                b =>
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

            modelBuilder.Entity<AnotherFakeEntity>(
                b =>
                {
                    b.HasOne<RelatedFakeEntity>()
                        .WithOne()
                        .HasForeignKey<AnotherFakeEntity>(e => e.AnotherId);
                });

            return modelBuilder.Model.FinalizeModel();
        }

        private static IModel CreateTwoLevelFKModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>();

            modelBuilder.Entity<RelatedFakeEntity>(
                b =>
                {
                    b.HasOne<FakeEntity>()
                        .WithOne()
                        .HasForeignKey<RelatedFakeEntity>(c => c.RelatedId);
                });

            modelBuilder.Entity<AnotherFakeEntity>(
                b =>
                {
                    b.HasOne<RelatedFakeEntity>()
                        .WithOne()
                        .HasForeignKey<AnotherFakeEntity>(c => c.AnotherId);
                });

            return modelBuilder.Model.FinalizeModel();
        }

        private static IModel CreateSharedTableModel()
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<FakeEntity>(
                b =>
                {
                    b.Ignore(c => c.UniqueValue);
                    b.Property(c => c.RelatedId).IsConcurrencyToken().HasColumnName("RelatedId");
                });

            modelBuilder.Entity<RelatedFakeEntity>(
                b =>
                {
                    b.Property(c => c.RelatedId).IsConcurrencyToken().HasColumnName("RelatedId");
                    b.HasOne<FakeEntity>()
                        .WithOne()
                        .HasForeignKey<RelatedFakeEntity>(c => c.Id);
                    b.ToTable(nameof(FakeEntity));
                });

            modelBuilder.Entity<DerivedRelatedFakeEntity>(
                b =>
                {
                    b.HasOne<AnotherFakeEntity>()
                        .WithOne()
                        .HasForeignKey<AnotherFakeEntity>(c => c.Id);
                });

            modelBuilder.Entity<AnotherFakeEntity>().ToTable(nameof(FakeEntity));

            return modelBuilder.Model.FinalizeModel();
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

        private class DerivedRelatedFakeEntity : RelatedFakeEntity
        {
            public string DerivedValue { get; set; }
        }

        private class AnotherFakeEntity
        {
            public int Id { get; set; }
            public int? AnotherId { get; set; }
        }
    }
}
