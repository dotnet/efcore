// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Model
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
            var property = CreateEntityType().AddProperty("MyProp", propertyType, shadowProperty: true);

            if (maxLength.HasValue)
            {
                property.SetMaxLength(maxLength);
            }

            return new ConcreteTypeMapper().MapPropertyType(property);
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
            var property = CreateEntityType().AddProperty("MyProp", propertyType, shadowProperty: true);
            property.Relational().ColumnType = typeName;

            return new ConcreteTypeMapper().MapPropertyType(property);
        }

        private static EntityType CreateEntityType() => new Entity.Metadata.Model().AddEntityType("MyType");

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
            private static readonly RelationalTypeMapping _string = new RelationalTypeMapping("just_string(2000)");
            private static readonly RelationalTypeMapping _unboundedStrng = new RelationalTypeMapping("just_string(max)");
            private static readonly RelationalTypeMapping _stringKey = new RelationalSizedTypeMapping("just_string(450)", 450);
            private static readonly RelationalTypeMapping _unboundedBinary = new RelationalTypeMapping("just_binary(max)", DbType.Binary);
            private static readonly RelationalTypeMapping _binary = new RelationalTypeMapping("just_binary(max)", DbType.Binary);
            private static readonly RelationalTypeMapping _binaryKey = new RelationalSizedTypeMapping("just_binary(900)", DbType.Binary, 900);
            private static readonly RelationalTypeMapping _rowversion = new RelationalSizedTypeMapping("rowversion", DbType.Binary, 8);
            private static readonly RelationalTypeMapping _defaultIntMapping = new RelationalTypeMapping("default_int_mapping");
            private static readonly RelationalTypeMapping _someIntMapping = new RelationalTypeMapping("some_int_mapping");

            protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }
                = new Dictionary<Type, RelationalTypeMapping>
                    {
                        { typeof(int), _defaultIntMapping }
                    };

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }
                = new Dictionary<string, RelationalTypeMapping>
                    {
                        { "some_int_mapping", _someIntMapping },
                        { "some_string(max)", _string },
                        { "some_binary(max)", _binary }
                    };

            protected override RelationalTypeMapping GetCustomMapping(IProperty property)
            {
                var clrType = property.ClrType.UnwrapEnumType();

                return clrType == typeof(string)
                    ? MapString(
                        property, 2000,
                        l => new RelationalSizedTypeMapping("just_string(" + l + ")", l),
                        _unboundedStrng, _string, _stringKey)
                    : clrType == typeof(byte[])
                        ? MapByteArray(
                            property, 2000,
                            l => new RelationalSizedTypeMapping("just_binary(" + l + ")", l),
                            _unboundedBinary, _binary, _binaryKey, _rowversion)
                        : base.GetCustomMapping(property);
            }
        }
    }
}
