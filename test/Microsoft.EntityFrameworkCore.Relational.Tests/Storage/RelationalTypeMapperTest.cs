// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;
using Microsoft.EntityFrameworkCore.Tests;

namespace Microsoft.EntityFrameworkCore.Storage.Tests
{
    public class RelationalTypeMapperTest
    {
        [Fact]
        public void Does_simple_mapping_from_CLR_type()
        {
            Assert.Equal("default_int_mapping", GetTypeMapping(typeof(int)).DefaultTypeName);
        }

        [Fact]
        public void Does_simple_mapping_from_nullable_CLR_type()
        {
            Assert.Equal("default_int_mapping", GetTypeMapping(typeof(int?)).DefaultTypeName);
        }

        [Fact]
        public void Does_type_mapping_from_string_with_no_MaxLength()
        {
            var mapping = GetTypeMapping(typeof(string));

            Assert.Equal("just_string(2000)", mapping.DefaultTypeName);
        }

        [Fact]
        public void Does_type_mapping_from_string_with_MaxLength()
        {
            var mapping = GetTypeMapping(typeof(string), 666);

            Assert.Equal("just_string(666)", mapping.DefaultTypeName);
            Assert.Equal(666, ((RelationalSizedTypeMapping)mapping).Size);
        }


        [Fact]
        public void Does_type_mapping_from_string_with_MaxLength_greater_than_unbounded_max()
        {
            var mapping = GetTypeMapping(typeof(string), 2020);

            Assert.Equal("just_string(max)", mapping.DefaultTypeName);
        }

        [Fact]
        public void Does_type_mapping_from_btye_array_with_no_MaxLength()
        {
            var mapping = GetTypeMapping(typeof(byte[]));

            Assert.Equal("just_binary(max)", mapping.DefaultTypeName);
        }

        [Fact]
        public void Does_type_mapping_from_btye_array_with_MaxLength()
        {
            var mapping = GetTypeMapping(typeof(byte[]), 777);

            Assert.Equal("just_binary(777)", mapping.DefaultTypeName);
            Assert.Equal(777, ((RelationalSizedTypeMapping)mapping).Size);
        }

        [Fact]
        public void Does_type_mapping_from_btye_array_greater_than_unbounded_max()
        {
            var mapping = GetTypeMapping(typeof(byte[]), 2020);

            Assert.Equal("just_binary(max)", mapping.DefaultTypeName);
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
            Assert.Equal("default_int_mapping", GetNamedMapping(typeof(int), "int").DefaultTypeName);
        }

        [Fact]
        public void Does_default_mapping_for_unrecognized_store_type()
        {
            Assert.Equal("default_int_mapping", GetNamedMapping(typeof(int), "int").DefaultTypeName);
        }

        [Fact]
        public void Does_type_mapping_from_named_string_with_no_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(string), "some_string(max)");

            Assert.Equal("just_string(2000)", mapping.DefaultTypeName);
        }

        [Fact]
        public void Does_type_mapping_from_named_string_with_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(string), "some_string(666)");

            Assert.Equal("just_string(2000)", mapping.DefaultTypeName);
        }

        [Fact]
        public void Does_type_mapping_from_named_binary_with_no_MaxLength()
        {
            var mapping = GetNamedMapping(typeof(byte[]), "some_binary(max)");

            Assert.Equal("just_binary(max)", mapping.DefaultTypeName);
        }

        private static RelationalTypeMapping GetNamedMapping(Type propertyType, string typeName)
        {
            var property = CreateEntityType().AddProperty("MyProp", propertyType);
            property.Relational().ColumnType = typeName;

            return new TestRelationalTypeMapper().GetMapping(property);
        }

        private static EntityType CreateEntityType() => new Entity.Metadata.Internal.Model().AddEntityType("MyType");
    }
}
