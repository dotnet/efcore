// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class ForeignKeyTests
    {
        [Fact]
        public void Can_create_foreign_key()
        {
            var entityType = new EntityType("E");
            var property = new Property("P", typeof(int));

            var foreignKey = new ForeignKey(entityType, new[] { property })
                {
                    IsUnique = true,
                    StorageName = "FK_Foo"
                };

            Assert.Same(entityType, foreignKey.ReferencedEntityType);
            Assert.True(foreignKey.Properties.Contains(property));
            Assert.True(foreignKey.IsUnique);
            Assert.Equal("FK_Foo", foreignKey.StorageName);
        }

        [Fact]
        public void IsRequired_when_property_not_nullable()
        {
            var entityType = new EntityType("E");
            var property = new Property("P", typeof(int));

            var foreignKey = new ForeignKey(entityType, new[] { property });

            Assert.True(foreignKey.IsRequired);
        }

        [Fact]
        public void IsRequired_when_property_nullable()
        {
            var entityType = new EntityType("E");
            var property = new Property("P", typeof(string));

            var foreignKey = new ForeignKey(entityType, new[] { property });

            Assert.False(foreignKey.IsRequired);
        }
    }
}
