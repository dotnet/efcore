// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Update;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Update
{
    public class ModificationCommandTest
    {
        [Fact]
        public void ModificationCommand_initialized_correctly_for_added_entities_with_identity_key()
        {
            var properties = new Dictionary<string, object> { { "Col1", 1 }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Added, properties, new[] { "Col1" });
            var table = new Table("T1", 
                new[]
                    {
                        new Column("Col1", "_") { GenerationStrategy = StoreValueGenerationStrategy.Identity}, 
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
            var properties = new Dictionary<string, object> { { "Col1", "ALFKI" }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Added, properties, new[] { "Col1" });
            var table = new Table("T1", new[] { new Column("Col1", "_"), new Column("Col2", "_") });

            var modificationCommand = new ModificationCommand(stateEntry, table);

            Assert.Equal("T1", modificationCommand.Table.Name);
            Assert.Equal(ModificationOperation.Insert, modificationCommand.Operation);
            Assert.Equal(
                properties, 
                modificationCommand.ColumnValues.Select(v => new KeyValuePair<string, object>(v.Key.Name, v.Value)));
            Assert.Null(modificationCommand.WhereClauses);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_modified_entities_with_identity_key()
        {
            var properties = new Dictionary<string, object> { { "Col1", 1 }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Modified, properties, new[] { "Col1" });
            var table = new Table("T1",
                new[]
                    {
                        new Column("Col1", "_") { GenerationStrategy = StoreValueGenerationStrategy.Identity}, 
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
            var properties = new Dictionary<string, object> { { "Col1", "ALFKI" }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Modified, properties, new[] { "Col1" });
            var table = new Table("T1", new[] { new Column("Col1", "_"), new Column("Col2", "_") });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Col1").ToArray());

            var modificationCommand = new ModificationCommand(stateEntry, table);

            Assert.Equal("T1", modificationCommand.Table.Name);
            Assert.Equal(ModificationOperation.Update, modificationCommand.Operation);

            Assert.Equal(1, modificationCommand.ColumnValues.Count());
            Assert.True(modificationCommand.ColumnValues.Any(v => v.Key.Name == "Col2" && (string)v.Value == "Test"));

            Assert.Equal(1, modificationCommand.WhereClauses.Count());
            Assert.True(modificationCommand.WhereClauses.Any(v => v.Key.Name == "Col1" && (string)v.Value == "ALFKI"));
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_deleted_entities()
        {
            var properties = new Dictionary<string, object> { { "Col1", 1 }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Deleted, properties, new[] { "Col1" });
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
            var stateEntry = CreateMockStateEntry("T1", EntityState.Unchanged, new Dictionary<string, object>(), new string[0]);

            Assert.Throws<NotSupportedException>(() => new ModificationCommand(stateEntry, new Table("Table")));
        }

        [Fact]
        public void ModificationCommand_throws_for_unknown_entities()
        {
            var stateEntry = CreateMockStateEntry("T1", EntityState.Unknown, new Dictionary<string, object>(), new string[0]);

            Assert.Throws<NotSupportedException>(() => new ModificationCommand(stateEntry, new Table("Table")));
        }

        private static StateEntry CreateMockStateEntry(string tableName, EntityState entityState,
            ICollection<KeyValuePair<string, object>> propertyValues, IEnumerable<string> keyProperties)
        {
            var entityType =
                CreateMockEntityType(
                    tableName,
                    propertyValues.Select(p => CreateProperty(p, keyProperties.Contains(p.Key))).ToArray(),
                    keyProperties);

            var mockStateEntry = new Mock<StateEntry>();
            mockStateEntry.Setup(e => e.EntityState).Returns(entityState);
            mockStateEntry.Setup(e => e.EntityType).Returns(entityType);
            mockStateEntry
                .Setup(e => e.GetPropertyValue(It.IsAny<IProperty>()))
                .Returns((IProperty p) => propertyValues.Single(v => p.Name == v.Key).Value);

            return mockStateEntry.Object;
        }

        private static IEntityType CreateMockEntityType(string tableName, IProperty[] properties, IEnumerable<string> keyProperties)
        {
            var mockKey = new Mock<IKey>();
            mockKey.Setup(k => k.Properties).Returns(properties.Where(p => keyProperties.Contains(p.Name)).ToArray());

            var mockEntityType = new Mock<IEntityType>();
            mockEntityType.Setup(e => e.Properties).Returns(properties);
            mockEntityType.Setup(e => e.GetKey()).Returns(mockKey.Object);
            mockEntityType.Setup(e => e.StorageName).Returns(tableName);

            return mockEntityType.Object;
        }

        private static IProperty CreateProperty(KeyValuePair<string, object> propertyValue, bool isKey)
        {
            var mockProperty = new Mock<IProperty>();
            mockProperty.Setup(p => p.Name).Returns(propertyValue.Key);
            mockProperty.Setup(p => p.StorageName).Returns(propertyValue.Key);
            mockProperty.Setup(p => p.ValueGenerationStrategy)
                .Returns(isKey && propertyValue.Value is int
                    ? ValueGenerationStrategy.StoreIdentity
                    : ValueGenerationStrategy.None);

            return mockProperty.Object;
        }
    }
}
