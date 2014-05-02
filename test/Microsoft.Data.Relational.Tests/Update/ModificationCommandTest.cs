// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.Relational.Update;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Update
{
    public class ModificationCommandTest
    {
        [Fact]
        public void ModificationCommand_initialized_correctly_for_added_entities_with_identity_key()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationStrategy.StoreIdentity);

            var table = new Table("T1",
                new[]
                    {
                        new Column("Col1", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Identity },
                        new Column("Col2", "_")
                    });

            var modificationCommand = new ModificationCommand(stateEntry, table);

            Assert.Equal("T1", modificationCommand.Table.Name);
            Assert.Equal(ModificationOperation.Insert, modificationCommand.Operation);
            Assert.Equal("Col2", modificationCommand.ColumnValues.Single().Key.Name);
            Assert.Equal("Test", modificationCommand.ColumnValues.Single().Value);
            Assert.Null(modificationCommand.WhereClauses);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_added_entities_with_client_generated_key()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);

            var table = new Table("T1", new[] { new Column("Col1", "_"), new Column("Col2", "_") });

            var modificationCommand = new ModificationCommand(stateEntry, table);

            Assert.Equal("T1", modificationCommand.Table.Name);
            Assert.Equal(ModificationOperation.Insert, modificationCommand.Operation);
            Assert.Equal(
                new Dictionary<string, object> { { "Col1", 1 }, { "Col2", "Test" } },
                modificationCommand.ColumnValues.Select(v => new KeyValuePair<string, object>(v.Key.Name, v.Value)));
            Assert.Null(modificationCommand.WhereClauses);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_modified_entities_with_identity_key()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified, ValueGenerationStrategy.StoreIdentity);

            var table = new Table("T1",
                new[]
                    {
                        new Column("Col1", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Identity },
                        new Column("Col2", "_")
                    });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            var modificationCommand = new ModificationCommand(stateEntry, table);

            Assert.Equal("T1", modificationCommand.Table.Name);
            Assert.Equal(ModificationOperation.Update, modificationCommand.Operation);
            Assert.Equal(1, modificationCommand.ColumnValues.Count());
            Assert.True(modificationCommand.ColumnValues.Any(v => v.Key.Name == "Col2" && (string)v.Value == "Test"));
            Assert.Equal(1, modificationCommand.WhereClauses.Count());
            Assert.True(modificationCommand.WhereClauses.Any(v => v.Key.Name == "Col1" && (int)v.Value == 1));
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_modified_entities_with_client_generated_key()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified);

            var table = new Table("T1", new[] { new Column("Col1", "_"), new Column("Col2", "_") });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            var modificationCommand = new ModificationCommand(stateEntry, table);

            Assert.Equal("T1", modificationCommand.Table.Name);
            Assert.Equal(ModificationOperation.Update, modificationCommand.Operation);

            Assert.Equal(1, modificationCommand.ColumnValues.Count());
            Assert.True(modificationCommand.ColumnValues.Any(v => v.Key.Name == "Col2" && (string)v.Value == "Test"));

            Assert.Equal(1, modificationCommand.WhereClauses.Count());
            Assert.True(modificationCommand.WhereClauses.Any(v => v.Key.Name == "Col1" && (int)v.Value == 1));
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_deleted_entities()
        {
            var stateEntry = CreateStateEntry(EntityState.Deleted);

            var table = new Table("T1", new[] { new Column("Col1", "_"), new Column("Col2", "_") });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            var modificationCommand = new ModificationCommand(stateEntry, table);

            Assert.Equal("T1", modificationCommand.Table.Name);
            Assert.Equal(ModificationOperation.Delete, modificationCommand.Operation);
            Assert.Null(modificationCommand.ColumnValues);

            Assert.Equal(1, modificationCommand.WhereClauses.Count());
            Assert.True(modificationCommand.WhereClauses.Any(v => v.Key.Name == "Col1" && (int)v.Value == 1));
        }

        [Fact]
        public void ModificationCommand_throws_for_unchanged_entities()
        {
            var stateEntry = CreateStateEntry(EntityState.Unchanged);

            Assert.Equal(
                Strings.FormatModificationFunctionInvalidEntityState(EntityState.Unchanged),
                Assert.Throws<NotSupportedException>(() => new ModificationCommand(stateEntry, new Table("Table"))).Message);
        }

        [Fact]
        public void ModificationCommand_throws_for_unknown_entities()
        {
            var stateEntry = CreateStateEntry(EntityState.Unknown);

            Assert.Equal(
                Strings.FormatModificationFunctionInvalidEntityState(EntityState.Unknown),
                Assert.Throws<NotSupportedException>(() => new ModificationCommand(stateEntry, new Table("Table"))).Message);
        }

        [Fact]
        public void RequiresResultPropagation_false_for_Delete_operation()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Deleted, ValueGenerationStrategy.StoreIdentity, ValueGenerationStrategy.StoreComputed);

            var table = new Table("T1",
                new[]
                    {
                        new Column("Col1", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Identity },
                        new Column("Col2", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Computed }
                    });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            Assert.False(new ModificationCommand(stateEntry, table).RequiresResultPropagation);
        }

        [Fact]
        public void RequiresResultPropagation_true_for_Insert_operation_if_store_generated_columns_exist()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Added, ValueGenerationStrategy.StoreIdentity, ValueGenerationStrategy.StoreComputed);

            var table = new Table("T1",
                new[]
                    {
                        new Column("Col1", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Identity },
                        new Column("Col2", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Computed }
                    });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            Assert.True(new ModificationCommand(stateEntry, table).RequiresResultPropagation);
        }

        [Fact]
        public void RequiresResultPropagation_false_for_Insert_operation_if_no_store_generated_columns_exist()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);

            var table = new Table("T1", new[] { new Column("Col1", "_"), new Column("Col2", "_") });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            Assert.False(new ModificationCommand(stateEntry, table).RequiresResultPropagation);
        }

        [Fact]
        public void RequiresResultPropagation_true_for_Update_operation_if_non_key_store_generated_columns_exist()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Modified, ValueGenerationStrategy.StoreIdentity, ValueGenerationStrategy.StoreComputed);

            var table = new Table("T1",
                new[]
                    {
                        new Column("Col1", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Identity },
                        new Column("Col2", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Computed }
                    });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            Assert.True(new ModificationCommand(stateEntry, table).RequiresResultPropagation);
        }

        [Fact]
        public void RequiresResultPropagation_false_for_Update_operation_if_no_non_key_store_generated_columns_exist()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified, ValueGenerationStrategy.StoreIdentity);

            var table = new Table("T1",
                new[]
                    {
                        new Column("Col1", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Identity },
                        new Column("Col2", "_")
                    });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            Assert.False(new ModificationCommand(stateEntry, table).RequiresResultPropagation);
        }

        private class T1
        {
            public int Col1 { get; set; }
            public string Col2 { get; set; }
        }

        private static IModel BuildModel(ValueGenerationStrategy keyStrategy, ValueGenerationStrategy nonKeyStrategy)
        {
            var model = new Entity.Metadata.Model();

            var entityType = new EntityType(typeof(T1));

            var key = entityType.AddProperty("Col1", typeof(int));
            key.ValueGenerationStrategy = keyStrategy;
            entityType.SetKey(key);

            var nonKey = entityType.AddProperty("Col2", typeof(string));
            nonKey.ValueGenerationStrategy = nonKeyStrategy;

            model.AddEntityType(entityType);

            return model;
        }

        private static ContextConfiguration CreateConfiguration(IModel model)
        {
            return new DbContext(
                new ServiceCollection()
                    .AddEntityFramework()
                    .BuildServiceProvider(),
                new EntityConfigurationBuilder()
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
            var stateEntry = CreateConfiguration(model).Services.StateEntryFactory.Create(model.GetEntityType("T1"), new T1 { Col1 = 1, Col2 = "Test" });
            stateEntry.EntityState = entityState;
            return stateEntry;
        }
    }
}
