// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Storage
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

            Assert.Equal("just_string(2000)", mapping.StoreType);
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

            Assert.Equal("just_string(max)", mapping.StoreType);
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

            Assert.Equal("just_binary(max)", mapping.StoreType);
        }

        private static RelationalTypeMapping GetTypeMapping(Type propertyType, int? maxLength = null)
        {
            var property = CreateEntityType().AddProperty("MyProp", propertyType);
            if (maxLength.HasValue)
            {
                property.SetMaxLength(maxLength);
            }

            return new TestRelationalTypeMapper().GetMapping(property);
        }

        [Fact]
        public void Does_simple_mapping_from_name()
        {
            Assert.Equal("default_int_mapping", GetNamedMapping(typeof(int), "int").StoreType);
        }

        [Fact]
        public void Does_default_mapping_for_unrecognized_store_type()
        {
            Assert.Equal("default_int_mapping", GetNamedMapping(typeof(int), "int").StoreType);
        }

        [Fact]
        public void Does_type_mapping_from_named_string_with_no_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(string), "some_string(max)");

            Assert.Equal("just_string(2000)", mapping.StoreType);
        }

        [Fact]
        public void Does_type_mapping_from_named_string_with_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(string), "some_string(666)");

            Assert.Equal("just_string(2000)", mapping.StoreType);
        }

        [Fact]
        public void Does_type_mapping_from_named_binary_with_no_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(byte[]), "some_binary(max)");

            Assert.Equal("just_binary(max)", mapping.StoreType);
        }

        private static RelationalTypeMapping GetNamedMapping(Type propertyType, string typeName)
        {
            var property = CreateEntityType().AddProperty("MyProp", propertyType);
            property.Relational().ColumnType = typeName;

            return new TestRelationalTypeMapper().GetMapping(property);
        }

        [Fact]
        public void Key_with_store_type_is_picked_up_by_FK()
        {
            var model = CreateModel();
            var mapper = new TestRelationalTypeMapper();

            Assert.Equal(
                "money",
                mapper.FindMapping(model.FindEntityType(typeof(MyType)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "money",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Relationship1Id")).StoreType);
        }

        [Fact]
        public void String_key_with_max_length_is_picked_up_by_FK()
        {
            var model = CreateModel();
            var mapper = new TestRelationalTypeMapper();

            Assert.Equal(
                "just_string(200)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_string(200)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Relationship1Id")).StoreType);
        }

        [Fact]
        public void Binary_key_with_max_length_is_picked_up_by_FK()
        {
            var model = CreateModel();
            var mapper = new TestRelationalTypeMapper();

            Assert.Equal(
                "just_binary(100)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_binary(100)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Relationship1Id")).StoreType);
        }

        [Fact]
        public void String_key_with_unicode_is_picked_up_by_FK()
        {
            var model = CreateModel();
            var mapper = new TestRelationalTypeMapper();

            Assert.Equal(
                "ansi_string(900)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "ansi_string(900)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType4)).FindProperty("Relationship1Id")).StoreType);
        }

        [Fact]
        public void Key_store_type_if_preferred_if_specified()
        {
            var model = CreateModel();
            var mapper = new TestRelationalTypeMapper();

            Assert.Equal(
                "money",
                mapper.FindMapping(model.FindEntityType(typeof(MyType)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "dec",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Relationship2Id")).StoreType);
        }

        [Fact]
        public void String_FK_max_length_is_preferred_if_specified()
        {
            var model = CreateModel();
            var mapper = new TestRelationalTypeMapper();

            Assert.Equal(
                "just_string(200)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_string(787)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Relationship2Id")).StoreType);
        }

        [Fact]
        public void Binary_FK_max_length_is_preferred_if_specified()
        {
            var model = CreateModel();
            var mapper = new TestRelationalTypeMapper();

            Assert.Equal(
                "just_binary(100)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_binary(767)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Relationship2Id")).StoreType);
        }

        [Fact]
        public void String_FK_unicode_is_preferred_if_specified()
        {
            var model = CreateModel();
            var mapper = new TestRelationalTypeMapper();

            Assert.Equal(
                "ansi_string(900)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Id")).StoreType);

            Assert.Equal(
                "just_string(450)",
                mapper.FindMapping(model.FindEntityType(typeof(MyRelatedType4)).FindProperty("Relationship2Id")).StoreType);
        }
    }
}
