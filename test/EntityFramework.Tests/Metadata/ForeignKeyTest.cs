// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);
            var principalProp = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(principalProp);

            var foreignKey
                = new ForeignKey(entityType.GetPrimaryKey(), new[] { dependentProp })
                    {
                        IsUnique = true,
                    };

            Assert.Same(entityType, foreignKey.ReferencedEntityType);
            Assert.Same(principalProp, foreignKey.ReferencedProperties.Single());
            Assert.Same(dependentProp, foreignKey.Properties.Single());
            Assert.True(foreignKey.IsUnique);
            Assert.Same(entityType.GetPrimaryKey(), foreignKey.ReferencedKey);
        }

        [Fact]
        public void Can_create_foreign_key_with_non_pk_principal()
        {
            var entityType = new EntityType("E");
            var keyProp = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);
            var principalProp = entityType.GetOrAddProperty("U", typeof(int), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(keyProp);
            var referencedKey = new Key(new[] { principalProp });

            var foreignKey
                = new ForeignKey(referencedKey, new[] { dependentProp })
                    {
                        IsUnique = true,
                    };

            Assert.Same(entityType, foreignKey.ReferencedEntityType);
            Assert.Same(principalProp, foreignKey.ReferencedProperties.Single());
            Assert.Same(dependentProp, foreignKey.Properties.Single());
            Assert.True(foreignKey.IsUnique);
            Assert.Same(referencedKey, foreignKey.ReferencedKey);
        }

        [Fact]
        public void IsRequired_when_dependent_property_not_nullable()
        {
            var entityType = new EntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int), shadowProperty: true);

            var foreignKey = new ForeignKey(entityType.GetPrimaryKey(), new[] { dependentProp });

            Assert.True(foreignKey.IsRequired);
        }

        [Fact]
        public void IsRequired_when_dependent_property_nullable()
        {
            var entityType = new EntityType("E");
            entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true));
            var dependentProp = entityType.GetOrAddProperty("P", typeof(int?), shadowProperty: true);

            var foreignKey = new ForeignKey(entityType.GetPrimaryKey(), new[] { dependentProp });

            Assert.False(foreignKey.IsRequired);
        }
    }
}
