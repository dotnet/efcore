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
        public void Throws_for_shadow_entity_type()
        {
            var entityType = new EntityType("SomeEntity");

            Assert.Equal(
                Strings.FormatNoClrType("SomeEntity"),
                Assert.Throws<InvalidOperationException>(
                    () => new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType)).Message);
        }

        [Fact]
        public void Delegate_from_entity_type_is_returned_if_it_implements_IEntityMaterializer()
        {
            var materializerMock = new Mock<IEntityMaterializer>();
            var typeMock = materializerMock.As<IEntityType>();

            var valueBuffer = new object[0];
            new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(typeMock.Object)(valueBuffer);

            materializerMock.Verify(m => m.CreatEntity(valueBuffer));
        }

        [Fact]
        public void Can_create_materializer_for_entity_with_auto_properties()
        {
            var entityType = new EntityType(typeof(SomeEntity));
            entityType.AddProperty("Id", typeof(int));
            entityType.AddProperty("Foo", typeof(string));
            entityType.AddProperty("Goo", typeof(Guid));

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new object[] { "Fu", gu, 77 });

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        [Fact]
        public void Can_create_materializer_for_entity_with_fields()
        {
            var entityType = new EntityType(typeof(SomeEntityWithFields));
            entityType.AddProperty("Id", typeof(int));
            entityType.AddProperty("Foo", typeof(string));
            entityType.AddProperty("Goo", typeof(Guid));

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntityWithFields)factory(new object[] { "Fu", gu, 77 });

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        [Fact]
        public void DBNulls_are_converted_to_nulls()
        {
            var entityType = new EntityType(typeof(SomeEntity));
            entityType.AddProperty("Id", typeof(int));
            entityType.AddProperty("Foo", typeof(string));
            entityType.AddProperty("Goo", typeof(Guid?));

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

            var entity = (SomeEntity)factory(new object[] { DBNull.Value, DBNull.Value, 77 });

            Assert.Equal(77, entity.Id);
            Assert.Null(entity.Foo);
            Assert.Null(entity.Goo);
        }

        [Fact]
        public void Can_create_materializer_for_entity_ignoring_shadow_fields()
        {
            var entityType = new EntityType(typeof(SomeEntity));
            entityType.AddProperty("Id", typeof(int));
            entityType.AddProperty("IdShadow", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType.AddProperty("Foo", typeof(string));
            entityType.AddProperty("FooShadow", typeof(string), shadowProperty: true, concurrencyToken: false);
            entityType.AddProperty("Goo", typeof(Guid));
            entityType.AddProperty("GooShadow", typeof(Guid), shadowProperty: true, concurrencyToken: false);

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

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

        private class SomeEntityWithFields
        {
#pragma warning disable 649
            private int _id;
            private string _foo;
            private Guid? _goo;
#pragma warning restore 649

            public int Id
            {
                get { return _id; }
            }

            public string Foo
            {
                get { return _foo; }
            }

            public Guid? Goo
            {
                get { return _goo; }
            }
        }
    }
}
