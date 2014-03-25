// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class EntityMaterializerSourceTest
    {
        [Fact]
        public void Delegate_from_entity_type_is_returned_if_it_implements_IEntityMaterializer()
        {
            var materializerMock = new Mock<IEntityMaterializer>();
            var typeMock = materializerMock.As<IEntityType>();

            var valueBuffer = new object[0];
            new EntityMaterializerSource().GetMaterializer(typeMock.Object)(valueBuffer);

            materializerMock.Verify(m => m.CreatEntity(valueBuffer));
        }

        [Fact]
        public void Can_create_materializer_for_entity()
        {
            var entityType = new EntityType(typeof(SomeEntity));
            entityType.AddProperty("Id", typeof(int), shadowProperty: false);
            entityType.AddProperty("Foo", typeof(string), shadowProperty: false);
            entityType.AddProperty("Goo", typeof(Guid), shadowProperty: false);

            var factory = new EntityMaterializerSource().GetMaterializer(entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new object[] { "Fu", gu, 77 });

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        [Fact]
        public void DBNulls_are_converted_to_nulls()
        {
            var entityType = new EntityType(typeof(SomeEntity));
            entityType.AddProperty("Id", typeof(int), shadowProperty: false);
            entityType.AddProperty("Foo", typeof(string), shadowProperty: false);
            entityType.AddProperty("Goo", typeof(Guid?), shadowProperty: false);

            var factory = new EntityMaterializerSource().GetMaterializer(entityType);

            var entity = (SomeEntity)factory(new object[] { DBNull.Value, DBNull.Value, 77 });

            Assert.Equal(77, entity.Id);
            Assert.Null(entity.Foo);
            Assert.Null(entity.Goo);
        }

        [Fact]
        public void Can_create_materializer_for_entity_ignoring_shadow_fields()
        {
            var entityType = new EntityType(typeof(SomeEntity));
            entityType.AddProperty("Id", typeof(int), shadowProperty: false);
            entityType.AddProperty("IdShadow", typeof(int), shadowProperty: true);
            entityType.AddProperty("Foo", typeof(string), shadowProperty: false);
            entityType.AddProperty("FooShadow", typeof(string), shadowProperty: true);
            entityType.AddProperty("Goo", typeof(Guid), shadowProperty: false);
            entityType.AddProperty("GooShadow", typeof(Guid), shadowProperty: true);

            var factory = new EntityMaterializerSource().GetMaterializer(entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new object[] { "Fu", "FuS", gu, Guid.NewGuid(), 77, 777 });

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        private class SomeEntity
        {
            public int Id { get; set; }
            public string Foo { get; set; }
            public Guid? Goo { get; set; }
        }
    }
}
