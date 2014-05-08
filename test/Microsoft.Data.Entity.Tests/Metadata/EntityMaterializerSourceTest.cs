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

            var reader = Mock.Of<IValueReader>();
            new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(typeMock.Object)(reader);

            materializerMock.Verify(m => m.CreatEntity(reader));
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
            var entity = (SomeEntity)factory(new ObjectArrayValueReader(new object[] { "Fu", gu, 77 }));

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
            var entity = (SomeEntityWithFields)factory(new ObjectArrayValueReader(new object[] { "Fu", gu, 77 }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        [Fact]
        public void Fields_flagged_as_null_are_converted_to_nulls()
        {
            var valueReaderMock = new Mock<IValueReader>();
            valueReaderMock.Setup(m => m.ReadValue<int>(2)).Returns(77);
            valueReaderMock.Setup(m => m.ReadValue<string>(0)).Throws(new InvalidCastException("Attempt to cast DBNull value."));
            valueReaderMock.Setup(m => m.ReadValue<Guid?>(1)).Throws(new InvalidCastException("Attempt to cast DBNull value."));
            valueReaderMock.Setup(m => m.IsNull(2)).Returns(false);
            valueReaderMock.Setup(m => m.IsNull(0)).Returns(true);
            valueReaderMock.Setup(m => m.IsNull(1)).Returns(true);

            var entityType = new EntityType(typeof(SomeEntity));
            entityType.AddProperty("Id", typeof(int));
            entityType.AddProperty("Foo", typeof(string));
            entityType.AddProperty("Goo", typeof(Guid?));

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

            var entity = (SomeEntity)factory(valueReaderMock.Object);

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
            var entity = (SomeEntity)factory(new ObjectArrayValueReader(new object[] { "Fu", "FuS", gu, Guid.NewGuid(), 77, 777 }));

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
