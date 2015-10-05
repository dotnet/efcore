// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Update
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
            entry.SetPropertyModified(entry.EntityType.GetPrimaryKey().Properties.Single(), isModified: false);

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

            var entry = stateManager.GetOrCreateEntry(new FakeEntity { Id = 42, Value = "Test" });
            entry.SetEntityState(EntityState.Added);

            var relatedentry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 42 });
            relatedentry.SetEntityState(EntityState.Modified);

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

            var relatedentry = stateManager.GetOrCreateEntry(new RelatedFakeEntity { Id = 1, RelatedId = 3 });
            relatedentry.SetEntityState(EntityState.Modified);
            relatedentry.OriginalValues[relatedentry.EntityType.GetProperty("RelatedId")] = 42;
            relatedentry.SetPropertyModified(relatedentry.EntityType.GetPrimaryKey().Properties.Single(), isModified: false);

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedentry, previousParent, newParent }).ToArray();

            Assert.Equal(
                new[] { newParent, relatedentry, previousParent },
                commandBatches.Select(cb => cb.ModificationCommands.Single()).Select(mc => mc.Entries.Single()));
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
                        model.GetEntityType(typeof(RelatedFakeEntity)).GetForeignKeys().First(),
                        model.GetEntityType(typeof(FakeEntity)).GetForeignKeys().First())),
                Assert.Throws<InvalidOperationException>(
                    () => { var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { fakeEntry, relatedFakeEntry }).ToArray(); }).Message);
        }

        private static IServiceProvider CreateContextServices(IModel model)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model);
            optionsBuilder.UseInMemoryDatabase();

            return new DbContext(optionsBuilder.Options).GetService();
        }

        private static ICommandBatchPreparer CreateCommandBatchPreparer(IModificationCommandBatchFactory modificationCommandBatchFactory = null)
        {
            modificationCommandBatchFactory =
                modificationCommandBatchFactory ?? new TestModificationCommandBatchFactory(
                    Mock.Of<IRelationalCommandBuilderFactory>(),
                    Mock.Of<ISqlGenerator>(),
                    Mock.Of<IUpdateSqlGenerator>(),
                    Mock.Of<IRelationalValueBufferFactoryFactory>());

            return new CommandBatchPreparer(modificationCommandBatchFactory,
                new ParameterNameGeneratorFactory(),
                new ModificationCommandComparer(),
                new TestAnnotationProvider());
        }

        private static IModel CreateSimpleFKModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<FakeEntity>(b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Value);
                });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.HasKey(c => c.Id);
                    b.HasOne<FakeEntity>()
                        .WithOne()
                        .HasForeignKey<RelatedFakeEntity>(c => c.Id);
                });

            return modelBuilder.Model;
        }

        private static IModel CreateCyclicFKModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<FakeEntity>(b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Value);
                });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.HasKey(c => c.Id);
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

        private class TestModificationCommandBatchFactory : IModificationCommandBatchFactory
        {
            private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
            private readonly ISqlGenerator _sqlGenerator;
            private readonly IUpdateSqlGenerator _updateSqlGenerator;
            private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;

            public TestModificationCommandBatchFactory(
                IRelationalCommandBuilderFactory commandBuilderfactory,
                ISqlGenerator sqlGenerator,
                IUpdateSqlGenerator updateSqlGenerator,
                IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
            {
                _commandBuilderFactory = commandBuilderfactory;
                _sqlGenerator = sqlGenerator;
                _updateSqlGenerator = updateSqlGenerator;
                _valueBufferFactoryFactory = valueBufferFactoryFactory;
            }

            public ModificationCommandBatch Create()
                => new SingularModificationCommandBatch(
                    _commandBuilderFactory,
                    _sqlGenerator,
                    _updateSqlGenerator,
                    _valueBufferFactoryFactory,
                    new Mock<ISensitiveDataLogger>().Object,
                    new TelemetryListener("Fake"));
        }
    }
}
