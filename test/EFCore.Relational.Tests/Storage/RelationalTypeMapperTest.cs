// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalTypeMapperTest : RelationalTypeMapperTestBase
    {
        [Fact]
        public void Does_simple_mapping_from_CLR_type()
        {
            Assert.Equal("default_int_mapping", GetTypeMapping(typeof(int)).StoreType);
        }

        [Fact]
        public void Does_simple_mapping_from_nullable_CLR_type()
        {
            Assert.Equal("default_int_mapping", GetTypeMapping(typeof(int?)).StoreType);
        }

        [Fact]
        public void Does_type_mapping_from_string_with_no_MaxLength()
        {
            var mapping = GetTypeMapping(typeof(string));

            Assert.Equal("just_string(max)", mapping.StoreType);
        }

        [Fact]
        public void Does_type_mapping_from_string_with_MaxLength()
        {
            var mapping = GetTypeMapping(typeof(string), 666);

            Assert.Equal("just_string(666)", mapping.StoreType);
            Assert.Equal(666, mapping.Size);
        }

        [Fact]
        public void Does_type_mapping_from_string_with_MaxLength_greater_than_unbounded_max()
        {
            var mapping = GetTypeMapping(typeof(string), 2020);

            Assert.Equal("just_string(2020)", mapping.StoreType);
            Assert.Equal(2020, mapping.Size);
        }

        [Fact]
        public void Does_type_mapping_from_btye_array_with_no_MaxLength()
        {
            var mapping = GetTypeMapping(typeof(byte[]));

            Assert.Equal("just_binary(max)", mapping.StoreType);
        }

        [Fact]
        public void Does_type_mapping_from_btye_array_with_MaxLength()
        {
            var mapping = GetTypeMapping(typeof(byte[]), 777);

            Assert.Equal("just_binary(777)", mapping.StoreType);
            Assert.Equal(777, mapping.Size);
        }

        [Fact]
        public void Does_type_mapping_from_btye_array_greater_than_unbounded_max()
        {
            var mapping = GetTypeMapping(typeof(byte[]), 2020);

            Assert.Equal("just_binary(2020)", mapping.StoreType);
        }

        private RelationalTypeMapping GetTypeMapping(Type propertyType, int? maxLength = null)
        {
            var property = CreateEntityType().AddProperty("MyProp", propertyType);
            if (maxLength.HasValue)
            {
                property.SetMaxLength(maxLength);
            }

            return GetMapping(property);
        }

        [Fact]
        public void Does_simple_mapping_from_name()
        {
            Assert.Equal("int", GetNamedMapping(typeof(int), "int").StoreType);
        }

        [Fact]
        public void Does_default_mapping_for_unrecognized_store_type()
        {
            Assert.Equal("int", GetNamedMapping(typeof(int), "int").StoreType);
        }

        [Fact]
        public void Does_type_mapping_from_named_string_with_no_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(string), "some_string(max)");

            Assert.Equal("some_string(max)", mapping.StoreType);
        }

        [Fact]
        public void Does_type_mapping_from_named_string_with_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(string), "some_string(666)");

            Assert.Equal("some_string(666)", mapping.StoreType);
            Assert.Equal(666, mapping.Size);
        }

        [Fact]
        public void Does_type_mapping_from_named_binary_with_no_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(byte[]), "some_binary(max)");

            Assert.Equal("some_binary(max)", mapping.StoreType);
        }

        private RelationalTypeMapping GetNamedMapping(Type propertyType, string typeName)
        {
            var property = CreateEntityType().AddProperty("MyProp", propertyType);
            property.Relational().ColumnType = typeName;

            return GetMapping(property);
        }

        [Fact]
        public void Key_with_store_type_is_picked_up_by_FK()
        {
            var model = CreateModel();
            var mapper = CreateTestTypeMapper();

            Assert.Equal(
                "money",
                GetMapping(mapper, model.FindEntityType(typeof(MyType)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "money",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Relationship1Id")).StoreType);
        }

        private static IRelationalTypeMappingSource CreateTestTypeMapper()
            => new TestRelationalTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        public static RelationalTypeMapping GetMapping(
            Type type)
            => CreateTestTypeMapper().FindMapping(type);

        public static RelationalTypeMapping GetMapping(
            IProperty property)
            => CreateTestTypeMapper().FindMapping(property);

        [Fact]
        public void String_key_with_max_length_is_picked_up_by_FK()
        {
            var model = CreateModel();
            var mapper = CreateTestTypeMapper();

            Assert.Equal(
                "just_string(200)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_string(200)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Relationship1Id")).StoreType);
        }

        [Fact]
        public void Binary_key_with_max_length_is_picked_up_by_FK()
        {
            var model = CreateModel();
            var mapper = CreateTestTypeMapper();

            Assert.Equal(
                "just_binary(100)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_binary(100)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Relationship1Id")).StoreType);
        }

        [Fact]
        public void String_key_with_unicode_is_picked_up_by_FK()
        {
            var model = CreateModel();
            var mapper = CreateTestTypeMapper();

            Assert.Equal(
                "ansi_string(900)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "ansi_string(900)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType4)).FindProperty("Relationship1Id")).StoreType);
        }

        [Fact]
        public void Key_store_type_is_preferred_if_specified()
        {
            var model = CreateModel();
            var mapper = CreateTestTypeMapper();

            Assert.Equal(
                "money",
                GetMapping(mapper, model.FindEntityType(typeof(MyType)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "dec(6,1)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Relationship2Id")).StoreType);
        }

        [Fact]
        public void String_FK_max_length_is_preferred_if_specified()
        {
            var model = CreateModel();
            var mapper = CreateTestTypeMapper();

            Assert.Equal(
                "just_string(200)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_string(787)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Relationship2Id")).StoreType);
        }

        [Fact]
        public void Binary_FK_max_length_is_preferred_if_specified()
        {
            var model = CreateModel();
            var mapper = CreateTestTypeMapper();

            Assert.Equal(
                "just_binary(100)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_binary(767)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Relationship2Id")).StoreType);
        }

        [Fact]
        public void String_FK_unicode_is_preferred_if_specified()
        {
            var model = CreateModel();
            var mapper = CreateTestTypeMapper();

            Assert.Equal(
                "ansi_string(900)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_string(450)",
                GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType4)).FindProperty("Relationship2Id")).StoreType);
        }

        public static RelationalTypeMapping GetMapping(
            IRelationalTypeMappingSource typeMappingSource,
            IProperty property)
            => typeMappingSource.FindMapping(property);

        protected override ModelBuilder CreateModelBuilder() => RelationalTestHelpers.Instance.CreateConventionBuilder();
    }
}
