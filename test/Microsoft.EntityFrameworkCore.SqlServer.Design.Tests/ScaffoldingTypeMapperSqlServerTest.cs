// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design
{
    public class ScaffoldingTypeMapperSqlServerTest
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_int_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("int", isKeyOrIndex, rowVersion: false);

            AssertMapping<int>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_bigint_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("bigint", isKeyOrIndex, rowVersion: false);

            AssertMapping<long>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_bit_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("bit", isKeyOrIndex, rowVersion: false);

            AssertMapping<bool>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_datetime_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("datetime", isKeyOrIndex, rowVersion: false);

            AssertMapping<DateTime>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_datetime2_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("datetime2", isKeyOrIndex, rowVersion: false);

            AssertMapping<DateTime>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_normal_varbinary_max_column()
        {
            var mapping = CreateMapper().FindMapping("varbinary(max)", keyOrIndex: false, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_normal_varbinary_sized_column()
        {
            var mapping = CreateMapper().FindMapping("varbinary(200)", keyOrIndex: false, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: true, maxLength: 200, unicode: null);
        }

        [Fact]
        public void Maps_normal_binary_max_column()
        {
            var mapping = CreateMapper().FindMapping("binary(max)", keyOrIndex: false, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_normal_binary_sized_column()
        {
            var mapping = CreateMapper().FindMapping("binary(200)", keyOrIndex: false, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_key_varbinary_max_column()
        {
            var mapping = CreateMapper().FindMapping("varbinary(max)", keyOrIndex: true, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_key_varbinary_sized_column()
        {
            var mapping = CreateMapper().FindMapping("varbinary(200)", keyOrIndex: true, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: true, maxLength: 200, unicode: null);
        }

        [Fact]
        public void Maps_key_varbinary_default_sized_column()
        {
            var mapping = CreateMapper().FindMapping("varbinary(900)", keyOrIndex: true, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_key_binary_max_column()
        {
            var mapping = CreateMapper().FindMapping("binary(max)", keyOrIndex: true, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_key_binary_sized_column()
        {
            var mapping = CreateMapper().FindMapping("binary(200)", keyOrIndex: true, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_key_binary_default_sized_column()
        {
            var mapping = CreateMapper().FindMapping("binary(900)", keyOrIndex: true, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_rowversion_rowversion_column()
        {
            var mapping = CreateMapper().FindMapping("rowversion", keyOrIndex: false, rowVersion: true);

            AssertMapping<byte[]>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_rowversion_varbinary_max_column()
        {
            var mapping = CreateMapper().FindMapping("varbinary(max)", keyOrIndex: false, rowVersion: true);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_rowversion_varbinary_sized_column()
        {
            var mapping = CreateMapper().FindMapping("varbinary(200)", keyOrIndex: false, rowVersion: true);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_rowversion_varbinary_default_sized_column()
        {
            var mapping = CreateMapper().FindMapping("varbinary(8)", keyOrIndex: false, rowVersion: true);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_rowversion_binary_max_column()
        {
            var mapping = CreateMapper().FindMapping("binary(max)", keyOrIndex: false, rowVersion: true);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_rowversion_binary_sized_column()
        {
            var mapping = CreateMapper().FindMapping("binary(200)", keyOrIndex: false, rowVersion: true);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_rowversion_binary_default_sized_column()
        {
            var mapping = CreateMapper().FindMapping("binary(8)", keyOrIndex: false, rowVersion: true);

            AssertMapping<byte[]>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_normal_nvarchar_max_column()
        {
            var mapping = CreateMapper().FindMapping("nvarchar(max)", keyOrIndex: false, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_normal_nvarchar_sized_column()
        {
            var mapping = CreateMapper().FindMapping("nvarchar(200)", keyOrIndex: false, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: 200, unicode: null);
        }

        [Fact]
        public void Maps_normal_varchar_max_column()
        {
            var mapping = CreateMapper().FindMapping("varchar(max)", keyOrIndex: false, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: null, unicode: false);
        }

        [Fact]
        public void Maps_normal_varchar_sized_column()
        {
            var mapping = CreateMapper().FindMapping("varchar(200)", keyOrIndex: false, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: 200, unicode: false);
        }

        [Fact]
        public void Maps_key_nvarchar_max_column()
        {
            var mapping = CreateMapper().FindMapping("nvarchar(max)", keyOrIndex: true, rowVersion: false);

            AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_key_nvarchar_sized_column()
        {
            var mapping = CreateMapper().FindMapping("nvarchar(200)", keyOrIndex: true, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: 200, unicode: null);
        }

        [Fact]
        public void Maps_key_varchar_max_column()
        {
            var mapping = CreateMapper().FindMapping("varchar(max)", keyOrIndex: true, rowVersion: false);

            AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_key_varchar_sized_column()
        {
            var mapping = CreateMapper().FindMapping("varchar(200)", keyOrIndex: true, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: 200, unicode: false);
        }

        [Fact]
        public void Maps_key_nvarchar_default_sized_column()
        {
            var mapping = CreateMapper().FindMapping("nvarchar(450)", keyOrIndex: true, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Fact]
        public void Maps_key_varchar_default_sized_column()
        {
            var mapping = CreateMapper().FindMapping("varchar(900)", keyOrIndex: true, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: null, unicode: false);
        }

        [Fact]
        public void Maps_text_column()
        {
            var mapping = CreateMapper().FindMapping("text", keyOrIndex: true, rowVersion: false);

            AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        private static void AssertMapping<T>(TypeScaffoldingInfo mapping, bool inferred, int? maxLength, bool? unicode)
        {
            Assert.Same(typeof(T), mapping.ClrType);
            Assert.Equal(inferred, mapping.IsInferred);
            Assert.Equal(maxLength, mapping.ScaffoldMaxLength);
            Assert.Equal(unicode, mapping.ScaffoldUnicode);
        }

        private static ScaffoldingTypeMapper CreateMapper()
            => new ScaffoldingTypeMapper(new SqlServerTypeMapper());
    }
}
