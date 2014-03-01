// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class ForeignKeyTest
    {
        [Fact]
        public void Can_create_foreign_key()
        {
            var entityType = new EntityType("E");
            var dependentProp = new Property("P", typeof(int));
            var principalProp = new Property("Id", typeof(int));

            var foreignKey = new ForeignKey(entityType, new[] { new PropertyPair(principalProp, dependentProp) })
                {
                    IsUnique = true,
                    StorageName = "FK_Foo"
                };

            Assert.Same(entityType, foreignKey.PrincipalType);
            Assert.Same(principalProp, foreignKey.Properties.Single().Principal);
            Assert.Same(dependentProp, foreignKey.Properties.Single().Dependent);
            Assert.True(foreignKey.IsUnique);
            Assert.Equal("FK_Foo", foreignKey.StorageName);
        }

        [Fact]
        public void IsRequired_when_dependent_property_not_nullable()
        {
            var entityType = new EntityType("E");
            var dependentProp = new Property("P", typeof(int));
            var principalProp = new Property("Id", typeof(int));

            var foreignKey = new ForeignKey(entityType, new[] { new PropertyPair(principalProp, dependentProp) });

            Assert.True(foreignKey.IsRequired);
        }

        [Fact]
        public void IsRequired_when_dependent_property_nullable()
        {
            var entityType = new EntityType("E");
            var dependentProp = new Property("P", typeof(int?));
            var principalProp = new Property("Id", typeof(int));

            var foreignKey = new ForeignKey(entityType, new[] { new PropertyPair(principalProp, dependentProp) });

            Assert.False(foreignKey.IsRequired);
        }
    }
}
