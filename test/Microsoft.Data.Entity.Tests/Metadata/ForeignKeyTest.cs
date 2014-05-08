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
            var dependentProp = entityType.AddProperty("P", typeof(int));
            var principalProp = entityType.AddProperty("Id", typeof(int));
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
            var keyProp = entityType.AddProperty("Id", typeof(int));
            var dependentProp = entityType.AddProperty("P", typeof(int));
            var principalProp = entityType.AddProperty("U", typeof(int));
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
            entityType.SetKey(entityType.AddProperty("Id", typeof(int)));
            var dependentProp = entityType.AddProperty("P", typeof(int));

            var foreignKey = new ForeignKey(entityType.GetKey(), new[] { dependentProp });

            Assert.True(foreignKey.IsRequired);
        }

        [Fact]
        public void IsRequired_when_dependent_property_nullable()
        {
            var entityType = new EntityType("E");
            entityType.SetKey(entityType.AddProperty("Id", typeof(int)));
            var dependentProp = entityType.AddProperty("P", typeof(int?));

            var foreignKey = new ForeignKey(entityType.GetKey(), new[] { dependentProp });

            Assert.False(foreignKey.IsRequired);
        }
    }
}
