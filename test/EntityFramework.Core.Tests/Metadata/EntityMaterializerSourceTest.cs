// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable ConvertToAutoProperty

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class EntityMaterializerSourceTest
    {
        [Fact]
        public void Throws_for_shadow_entity_type()
        {
            var entityType = new Model().AddEntityType("SomeEntity");

            Assert.Equal(
                Strings.NoClrType("SomeEntity"),
                Assert.Throws<InvalidOperationException>(
                    () => new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType)).Message);
        }

        [Fact]
        public void Delegate_from_entity_type_is_returned_if_it_implements_IEntityMaterializer()
        {
            var materializerMock = new Mock<IEntityMaterializer>();
            var typeMock = materializerMock.As<IEntityType>();
            typeMock.SetupGet(et => et.Type).Returns(typeof(string));
            typeMock.SetupGet(et => et.HasClrType).Returns(true);

            var reader = Mock.Of<IValueReader>();
            new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(typeMock.Object)(reader);

            materializerMock.Verify(m => m.CreateEntity(reader));
        }

        [Fact]
        public void Can_create_materializer_for_entity_with_auto_properties()
        {
            var entityType = new Model().AddEntityType(typeof(SomeEntity));
            entityType.GetOrAddProperty("Enum", typeof(SomeEnum));
            entityType.GetOrAddProperty("Foo", typeof(string));
            entityType.GetOrAddProperty("Goo", typeof(Guid?));
            entityType.GetOrAddProperty("Id", typeof(int));
            entityType.GetOrAddProperty("MaybeEnum", typeof(SomeEnum?));

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new ObjectArrayValueReader(new object[] { 0, "Fu", gu, 77, 0 }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Equal(SomeEnum.EnumValue, entity.MaybeEnum);
        }

        [Fact]
        public void Can_create_materializer_for_entity_with_fields()
        {
            var entityType = new Model().AddEntityType(typeof(SomeEntityWithFields));
            entityType.GetOrAddProperty("Enum", typeof(SomeEnum));
            entityType.GetOrAddProperty("Foo", typeof(string));
            entityType.GetOrAddProperty("Goo", typeof(Guid?));
            entityType.GetOrAddProperty("Id", typeof(int));
            entityType.GetOrAddProperty("MaybeEnum", typeof(SomeEnum?));

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntityWithFields)factory(new ObjectArrayValueReader(new object[] { 0, "Fu", gu, 77, null }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Null(entity.MaybeEnum);
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

            var entityType = new Model().AddEntityType(typeof(SomeEntity));
            entityType.GetOrAddProperty("Id", typeof(int));
            entityType.GetOrAddProperty("Foo", typeof(string));
            entityType.GetOrAddProperty("Goo", typeof(Guid?));

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

            var entity = (SomeEntity)factory(valueReaderMock.Object);

            Assert.Equal(77, entity.Id);
            Assert.Null(entity.Foo);
            Assert.Null(entity.Goo);
        }

        [Fact]
        public void Can_create_materializer_for_entity_ignoring_shadow_fields()
        {
            var entityType = new Model().AddEntityType(typeof(SomeEntity));
            entityType.GetOrAddProperty("Id", typeof(int));
            entityType.GetOrAddProperty("IdShadow", typeof(int), shadowProperty: true);
            entityType.GetOrAddProperty("Foo", typeof(string));
            entityType.GetOrAddProperty("FooShadow", typeof(string), shadowProperty: true);
            entityType.GetOrAddProperty("Goo", typeof(Guid?));
            entityType.GetOrAddProperty("GooShadow", typeof(Guid), shadowProperty: true);

            var factory = new EntityMaterializerSource(new MemberMapper(new FieldMatcher())).GetMaterializer(entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new ObjectArrayValueReader(new object[] { "Fu", "FuS", gu, Guid.NewGuid(), 77, 777 }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        private class SomeEntity
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int Id { get; set; }
            public string Foo { get; set; }
            public Guid? Goo { get; set; }
            public SomeEnum Enum { get; set; }
            public SomeEnum? MaybeEnum { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        private class SomeEntityWithFields
        {
#pragma warning disable 649
            private int _id;
            private string _foo;
            private Guid? _goo;
            private SomeEnum _enum;
            private SomeEnum? _maybeEnum;
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

            public SomeEnum Enum
            {
                get { return _enum; }
            }

            public SomeEnum? MaybeEnum
            {
                get { return _maybeEnum; }
            }
        }

        private enum SomeEnum
        {
            EnumValue
        }
    }
}
