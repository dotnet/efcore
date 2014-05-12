// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class ModificationCommandTest
    {
        [Fact]
        public void ModificationCommand_initialized_correctly_for_added_entities_with_identity_key()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationStrategy.StoreIdentity);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.Equal("T1", command.TableName);
            Assert.Equal(EntityState.Added, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Col1", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.True(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Col2", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Name", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_added_entities_with_client_generated_key()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.Equal("T1", command.TableName);
            Assert.Equal(EntityState.Added, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Col1", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Col2", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Name", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_modified_entities_with_identity_key()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified, ValueGenerationStrategy.StoreIdentity);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.Equal("T1", command.TableName);
            Assert.Equal(EntityState.Modified, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Col1", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Col2", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Name", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_modified_entities_with_client_generated_key()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.Equal("T1", command.TableName);
            Assert.Equal(EntityState.Modified, command.EntityState);
            Assert.Equal(2, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Col1", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);

            columnMod = command.ColumnModifications[1];

            Assert.Equal("Col2", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Name", columnMod.Property.Name);
            Assert.False(columnMod.IsCondition);
            Assert.False(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.True(columnMod.IsWrite);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_deleted_entities()
        {
            var stateEntry = CreateStateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.Equal("T1", command.TableName);
            Assert.Equal(EntityState.Deleted, command.EntityState);
            Assert.Equal(1, command.ColumnModifications.Count);

            var columnMod = command.ColumnModifications[0];

            Assert.Equal("Col1", columnMod.ColumnName);
            Assert.Same(stateEntry, columnMod.StateEntry);
            Assert.Equal("Id", columnMod.Property.Name);
            Assert.True(columnMod.IsCondition);
            Assert.True(columnMod.IsKey);
            Assert.False(columnMod.IsRead);
            Assert.False(columnMod.IsWrite);
        }

        [Fact]
        public void ModificationCommand_throws_for_unchanged_entities()
        {
            var stateEntry = CreateStateEntry(EntityState.Unchanged);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());

            Assert.Equal(
                Strings.FormatModificationFunctionInvalidEntityState(EntityState.Unchanged),
                Assert.Throws<NotSupportedException>(() => command.AddStateEntry(stateEntry)).Message);
        }

        [Fact]
        public void ModificationCommand_throws_for_unknown_entities()
        {
            var stateEntry = CreateStateEntry(EntityState.Unknown);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());

            Assert.Equal(
                Strings.FormatModificationFunctionInvalidEntityState(EntityState.Unknown),
                Assert.Throws<NotSupportedException>(() => command.AddStateEntry(stateEntry)).Message);
        }

        [Fact]
        public void RequiresResultPropagation_false_for_Delete_operation()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Deleted, ValueGenerationStrategy.StoreIdentity, ValueGenerationStrategy.StoreComputed);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.False(command.RequiresResultPropagation);
        }

        [Fact]
        public void RequiresResultPropagation_true_for_Insert_operation_if_store_generated_columns_exist()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Added, ValueGenerationStrategy.StoreIdentity, ValueGenerationStrategy.StoreComputed);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.True(command.RequiresResultPropagation);
        }

        [Fact]
        public void RequiresResultPropagation_false_for_Insert_operation_if_no_store_generated_columns_exist()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.False(command.RequiresResultPropagation);
        }

        [Fact]
        public void RequiresResultPropagation_true_for_Update_operation_if_non_key_store_generated_columns_exist()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Modified, ValueGenerationStrategy.StoreIdentity, ValueGenerationStrategy.StoreComputed);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.True(command.RequiresResultPropagation);
        }

        [Fact]
        public void RequiresResultPropagation_false_for_Update_operation_if_no_non_key_store_generated_columns_exist()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified, ValueGenerationStrategy.StoreIdentity);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            Assert.False(command.RequiresResultPropagation);
        }

        private class T1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static IModel BuildModel(ValueGenerationStrategy keyStrategy, ValueGenerationStrategy nonKeyStrategy)
        {
            var model = new Metadata.Model();

            var entityType = new EntityType(typeof(T1));

            var key = entityType.AddProperty("Id", typeof(int));
            key.ValueGenerationStrategy = keyStrategy;
            key.StorageName = "Col1";
            entityType.SetKey(key);

            var nonKey = entityType.AddProperty("Name", typeof(string));
            nonKey.StorageName = "Col2";
            nonKey.ValueGenerationStrategy = nonKeyStrategy;

            model.AddEntityType(entityType);

            return model;
        }

        private static DbContextConfiguration CreateConfiguration(IModel model)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework();
            return new DbContext(serviceCollection.BuildServiceProvider(),
                new DbContextOptions()
                    .UseModel(model)
                    .BuildConfiguration())
                .Configuration;
        }

        private static StateEntry CreateStateEntry(
            EntityState entityState,
            ValueGenerationStrategy keyStrategy = ValueGenerationStrategy.None,
            ValueGenerationStrategy nonKeyStrategy = ValueGenerationStrategy.None)
        {
            var model = BuildModel(keyStrategy, nonKeyStrategy);
            var stateEntry = CreateConfiguration(model).Services.StateEntryFactory.Create(
                model.GetEntityType("T1"), new T1 { Id = 1, Name = "Test" });
            stateEntry.EntityState = entityState;
            return stateEntry;
        }
    }
}
