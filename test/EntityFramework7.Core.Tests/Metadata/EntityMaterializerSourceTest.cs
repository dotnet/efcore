// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable ConvertToAutoProperty

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class EntityMaterializerSourceTest
    {
        [Fact]
        public void Delegate_from_entity_type_is_returned_if_it_implements_IEntityMaterializer()
        {
            var materializerMock = new Mock<IEntityMaterializer>();
            var typeMock = materializerMock.As<IEntityType>();
            typeMock.SetupGet(et => et.ClrType).Returns(typeof(string));

            var reader = ValueBuffer.Empty;
            GetMaterializer(new EntityMaterializerSource(new MemberMapper(new FieldMatcher())), typeMock.Object)(reader);

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

            var factory = GetMaterializer(new EntityMaterializerSource(new MemberMapper(new FieldMatcher())), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, SomeEnum.EnumValue }));

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

            var factory = GetMaterializer(new EntityMaterializerSource(new MemberMapper(new FieldMatcher())), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntityWithFields)factory(new ValueBuffer(new object[] { SomeEnum.EnumValue, "Fu", gu, 77, null }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
            Assert.Equal(SomeEnum.EnumValue, entity.Enum);
            Assert.Null(entity.MaybeEnum);
        }

        [Fact]
        public void Can_read_nulls()
        {
            var entityType = new Model().AddEntityType(typeof(SomeEntity));
            entityType.GetOrAddProperty("Id", typeof(int));
            entityType.GetOrAddProperty("Foo", typeof(string));
            entityType.GetOrAddProperty("Goo", typeof(Guid?));

            var factory = GetMaterializer(new EntityMaterializerSource(new MemberMapper(new FieldMatcher())), entityType);

            var entity = (SomeEntity)factory(new ValueBuffer(new object[] { null, null, 77 }));

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

            var factory = GetMaterializer(new EntityMaterializerSource(new MemberMapper(new FieldMatcher())), entityType);

            var gu = Guid.NewGuid();
            var entity = (SomeEntity)factory(new ValueBuffer(new object[] { "Fu", "FuS", gu, Guid.NewGuid(), 77, 777 }));

            Assert.Equal(77, entity.Id);
            Assert.Equal("Fu", entity.Foo);
            Assert.Equal(gu, entity.Goo);
        }

        private static readonly ParameterExpression _readerParameter
            = Expression.Parameter(typeof(ValueBuffer), "valueBuffer");

        public virtual Func<ValueBuffer, object> GetMaterializer(IEntityMaterializerSource source, IEntityType entityType)
        {
            return Expression.Lambda<Func<ValueBuffer, object>>(
                source.CreateMaterializeExpression(entityType, _readerParameter),
                _readerParameter)
                .Compile();
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
