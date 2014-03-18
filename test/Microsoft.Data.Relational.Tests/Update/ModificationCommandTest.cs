// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Relational.Update
{
    public class ModificationCommandTest
    {
        [Fact]
        public void ModificationCommand_initialized_correctly_for_added_entities_with_identity_key()
        {
            var properties = new Dictionary<string, object> { { "Col1", 1 }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry( "T1", EntityState.Added, properties, new[] { "Col1" });

            var modificationCommand = new ModificationCommand(stateEntry);

            Assert.Equal("T1", modificationCommand.TableName);
            Assert.Equal(ModificationOperation.Insert, modificationCommand.Operation);
            Assert.Equal(properties.Where(p => p.Key != "Col1"), modificationCommand.ColumnValues);
            Assert.Null(modificationCommand.WhereClauses);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_added_entities_with_client_generated_key()
        {
            var properties = new Dictionary<string, object> { { "Col1", "ALFKI" }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Added, properties, new[] { "Col1" });

            var modificationCommand = new ModificationCommand(stateEntry);

            Assert.Equal("T1", modificationCommand.TableName);
            Assert.Equal(ModificationOperation.Insert, modificationCommand.Operation);
            Assert.Equal(properties, modificationCommand.ColumnValues);
            Assert.Null(modificationCommand.WhereClauses);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_modified_entities_with_identity_key()
        {
            var properties = new Dictionary<string, object> { { "Col1", 1 }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Modified, properties, new[] { "Col1" });

            var modificationCommand = new ModificationCommand(stateEntry);

            Assert.Equal("T1", modificationCommand.TableName);
            Assert.Equal(ModificationOperation.Update, modificationCommand.Operation);
            Assert.Equal(properties.Where(p => p.Key == "Col2"), modificationCommand.ColumnValues);
            Assert.Equal(properties.Where(p => p.Key == "Col1"), modificationCommand.WhereClauses);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_modified_entities_with_client_generated_key()
        {
            var properties = new Dictionary<string, object> { { "Col1", "ALFKI" }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Modified, properties, new[] { "Col1" });

            var modificationCommand = new ModificationCommand(stateEntry);

            Assert.Equal("T1", modificationCommand.TableName);
            Assert.Equal(ModificationOperation.Update, modificationCommand.Operation);
            Assert.Equal(properties.Where(p => p.Key == "Col2"), modificationCommand.ColumnValues);
            Assert.Equal(properties.Where(p => p.Key == "Col1"), modificationCommand.WhereClauses);
        }

        [Fact]
        public void ModificationCommand_initialized_correctly_for_deleted_entities()
        {
            var properties = new Dictionary<string, object> { { "Col1", 1 }, { "Col2", "Test" } };
            var stateEntry = CreateMockStateEntry("T1", EntityState.Deleted, properties, new[] { "Col1" });

            var modificationCommand = new ModificationCommand(stateEntry);

            Assert.Equal("T1", modificationCommand.TableName);
            Assert.Equal(ModificationOperation.Delete, modificationCommand.Operation);
            Assert.Null(modificationCommand.ColumnValues);
            Assert.Equal(properties.Where(p => p.Key == "Col1"), modificationCommand.WhereClauses);
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
