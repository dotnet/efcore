// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ForeignKeyTest
    {
        [Fact]
        public void Can_create_foreign_key()
        {
            var entityType = new EntityType("E");
            var dependentProp = entityType.AddProperty("P", typeof(int), shadowProperty: false);
            var principalProp = entityType.AddProperty("Id", typeof(int), shadowProperty: false);
            entityType.SetKey(principalProp);

            var foreignKey 
                = new ForeignKey(entityType.GetKey(), new[] { dependentProp })
                {
                    IsUnique = true,
                    StorageName = "FK_Foo"
                };

            Assert.Same(entityType, foreignKey.ReferencedEntityType);
            Assert.Same(principalProp, foreignKey.ReferencedProperties.Single());
            Assert.Same(dependentProp, foreignKey.Properties.Single());
            Assert.True(foreignKey.IsUnique);
            Assert.Equal("FK_Foo", foreignKey.StorageName);
        }

        [Fact]
        public void Can_create_foreign_key_with_non_pk_principal()
        {
            var entityType = new EntityType("E");
            var keyProp = entityType.AddProperty("Id", typeof(int), shadowProperty: false);
            var dependentProp = entityType.AddProperty("P", typeof(int), shadowProperty: false);
            var principalProp = entityType.AddProperty("U", typeof(int), shadowProperty: false);
            entityType.SetKey(keyProp);

            var foreignKey 
                = new ForeignKey(new Key(new[] { principalProp }), new[] { dependentProp })
                {
                    IsUnique = true,
                    StorageName = "FK_Foo"
                };

            Assert.Same(entityType, foreignKey.ReferencedEntityType);
            Assert.Same(principalProp, foreignKey.ReferencedProperties.Single());
            Assert.Same(dependentProp, foreignKey.Properties.Single());
            Assert.True(foreignKey.IsUnique);
            Assert.Equal("FK_Foo", foreignKey.StorageName);
        }

        [Fact]
        public void IsRequired_when_dependent_property_not_nullable()
        {
            var entityType = new EntityType("E");
            entityType.SetKey(entityType.AddProperty("Id", typeof(int), shadowProperty: false));
            var dependentProp = entityType.AddProperty("P", typeof(int), shadowProperty: false);

            var foreignKey = new ForeignKey(entityType.GetKey(), new[] { dependentProp });

            Assert.True(foreignKey.IsRequired);
        }

        [Fact]
        public void IsRequired_when_dependent_property_nullable()
        {
            var entityType = new EntityType("E");
            entityType.SetKey(entityType.AddProperty("Id", typeof(int), shadowProperty: false));
            var dependentProp = entityType.AddProperty("P", typeof(int?), shadowProperty: false);

            var foreignKey = new ForeignKey(entityType.GetKey(), new[] { dependentProp });

            Assert.False(foreignKey.IsRequired);
        }
    }
}
