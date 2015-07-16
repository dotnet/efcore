// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

using CoreStrings = Microsoft.Data.Entity.Internal.Strings;

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

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }, new DbContextOptions<DbContext>()).ToArray();
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

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }, new DbContextOptions<DbContext>()).ToArray();
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

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { entry }, new DbContextOptions<DbContext>()).ToArray();
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

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedentry, entry }, new DbContextOptions<DbContext>()).ToArray();

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

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedentry, entry }, new DbContextOptions<DbContext>()).ToArray();

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

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { secondentry, firstentry }, new DbContextOptions<DbContext>()).ToArray();

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

            var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { relatedentry, previousParent, newParent }, new DbContextOptions<DbContext>()).ToArray();

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
            var options = new Mock<IDbContextOptions>().Object;

            var commandBatches = CreateCommandBatchPreparer(modificationCommandBatchFactoryMock.Object).BatchCommands(new[] { relatedentry, entry }, options);

            var commandBatchesEnumerator = commandBatches.GetEnumerator();
            commandBatchesEnumerator.MoveNext();

            modificationCommandBatchFactoryMock.Verify(
                mcb => mcb.Create(options, It.IsAny<IRelationalMetadataExtensionProvider>()),
                Times.Once);

            commandBatchesEnumerator.MoveNext();

            modificationCommandBatchFactoryMock.Verify(
                mcb => mcb.Create(options, It.IsAny<IRelationalMetadataExtensionProvider>()),
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
                    () => { var commandBatches = CreateCommandBatchPreparer().BatchCommands(new[] { fakeEntry, relatedFakeEntry }, new DbContextOptions<DbContext>()).ToArray(); }).Message);
        }

        private static IServiceProvider CreateContextServices(IModel model)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model);
            optionsBuilder.UseInMemoryDatabase(persist: false);

            return new DbContext(optionsBuilder.Options).GetService();
        }

        private static ICommandBatchPreparer CreateCommandBatchPreparer(IModificationCommandBatchFactory modificationCommandBatchFactory = null)
        {
            modificationCommandBatchFactory =
                modificationCommandBatchFactory ?? new TestModificationCommandBatchFactory(
                    Mock.Of<IUpdateSqlGenerator>());

            return new TestCommandBatchPreparer(modificationCommandBatchFactory,
                new ParameterNameGeneratorFactory(),
                new ModificationCommandComparer(),
                Mock.Of<IRelationalValueBufferFactoryFactory>(),
                new TestMetadataExtensionProvider());
        }

        private static IModel CreateSimpleFKModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<FakeEntity>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(c => c.Value);
                });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.Key(c => c.Id);
                    b.Reference<FakeEntity>()
                        .InverseReference()
                        .ForeignKey<RelatedFakeEntity>(c => c.Id);
                });

            return modelBuilder.Model;
        }

        private static IModel CreateCyclicFKModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<FakeEntity>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(c => c.Value);
                });

            modelBuilder.Entity<RelatedFakeEntity>(b =>
                {
                    b.Key(c => c.Id);
                    b.Reference<FakeEntity>()
                        .InverseReference()
                        .ForeignKey<RelatedFakeEntity>(c => c.RelatedId);
                });

            modelBuilder
                .Entity<FakeEntity>()
                .Reference<RelatedFakeEntity>()
                .InverseReference()
                .ForeignKey<FakeEntity>(c => c.RelatedId);

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

        private class TestCommandBatchPreparer : CommandBatchPreparer
        {
            public TestCommandBatchPreparer(
                IModificationCommandBatchFactory modificationCommandBatchFactory,
                IParameterNameGeneratorFactory parameterNameGeneratorFactory,
                IComparer<ModificationCommand> modificationCommandComparer,
                IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
                IRelationalMetadataExtensionProvider metadataExtensionProvider)
                : base(
                      modificationCommandBatchFactory,
                      parameterNameGeneratorFactory,
                      modificationCommandComparer,
                      valueBufferFactoryFactory,
                      metadataExtensionProvider)
            {
            }
        }

        private class TestMetadataExtensionProvider : IRelationalMetadataExtensionProvider
        {
            public IRelationalEntityTypeAnnotations Extensions(IEntityType entityType) => entityType.Relational();
            public IRelationalForeignKeyAnnotations Extensions(IForeignKey foreignKey) => foreignKey.Relational();
            public IRelationalIndexAnnotations Extensions(IIndex index) => index.Relational();
            public IRelationalKeyAnnotations Extensions(IKey key) => key.Relational();
            public IRelationalModelAnnotations Extensions(IModel model) => model.Relational();
            public IRelationalPropertyAnnotations Extensions(IProperty property) => property.Relational();
        }

        private class TestModificationCommandBatchFactory : ModificationCommandBatchFactory
        {
            public TestModificationCommandBatchFactory(
                IUpdateSqlGenerator sqlGenerator)
                : base(sqlGenerator)
            {
            }

            public override ModificationCommandBatch Create(
                IDbContextOptions options,
                IRelationalMetadataExtensionProvider metadataExtensionProvider)
            {
                return new SingularModificationCommandBatch(UpdateSqlGenerator);
            }
        }
    }
}
