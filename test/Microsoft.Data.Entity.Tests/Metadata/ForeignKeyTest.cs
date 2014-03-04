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
            entityType.Key = new[] { principalProp };

            var foreignKey = new ForeignKey(entityType, new[] { dependentProp })
                {
                    IsUnique = true,
                    StorageName = "FK_Foo"
                };

            Assert.Same(entityType, foreignKey.PrincipalType);
            Assert.Same(principalProp, foreignKey.PrincipalProperties.Single());
            Assert.Same(dependentProp, foreignKey.DependentProperties.Single());
            Assert.True(foreignKey.IsUnique);
            Assert.Equal("FK_Foo", foreignKey.StorageName);
        }

        [Fact]
        public void Can_create_foreign_key_with_non_PK_principal()
        {
            var entityType = new EntityType("E");
            var keyProp = new Property("Id", typeof(int));
            var dependentProp = new Property("P", typeof(int));
            var principalProp = new Property("U", typeof(int));
            entityType.Key = new[] { keyProp };

            var foreignKey = new ForeignKey(entityType, new[] { dependentProp })
                {
                    IsUnique = true,
                    StorageName = "FK_Foo",
                    PrincipalProperties = new[] { principalProp }
                };

            Assert.Same(entityType, foreignKey.PrincipalType);
            Assert.Same(principalProp, foreignKey.PrincipalProperties.Single());
            Assert.Same(dependentProp, foreignKey.DependentProperties.Single());
            Assert.True(foreignKey.IsUnique);
            Assert.Equal("FK_Foo", foreignKey.StorageName);
        }

        [Fact]
        public void IsRequired_when_dependent_property_not_nullable()
        {
            var entityType = new EntityType("E");
            var dependentProp = new Property("P", typeof(int));

            var foreignKey = new ForeignKey(entityType, new[] { dependentProp });

            Assert.True(foreignKey.IsRequired);
        }

        [Fact]
        public void IsRequired_when_dependent_property_nullable()
        {
            var entityType = new EntityType("E");
            var dependentProp = new Property("P", typeof(int?));

            var foreignKey = new ForeignKey(entityType, new[] { dependentProp });

            Assert.False(foreignKey.IsRequired);
        }
    }
}
