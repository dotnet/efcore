// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.Design
{
    public class ScaffoldingTypeMapperSqliteTest
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_integer_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("Integer", isKeyOrIndex, rowVersion: false);

            AssertMapping<long>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_int_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("int", isKeyOrIndex, rowVersion: false);

            AssertMapping<long>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_bigint_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("bigint", isKeyOrIndex, rowVersion: false);

            AssertMapping<long>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_real_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("REAL", isKeyOrIndex, rowVersion: false);

            AssertMapping<double>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_float_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("Float", isKeyOrIndex, rowVersion: false);

            AssertMapping<double>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_double_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("double", isKeyOrIndex, rowVersion: false);

            AssertMapping<double>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_blob_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("BLOB", isKeyOrIndex, rowVersion: false);

            AssertMapping<byte[]>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_text_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("TEXT", isKeyOrIndex, rowVersion: false);

            AssertMapping<string>(mapping, inferred: true, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_clob_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("clob", isKeyOrIndex, rowVersion: false);

            AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_varchar_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("varchar", isKeyOrIndex, rowVersion: false);

            AssertMapping<string>(mapping, inferred: false, maxLength: null, unicode: null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Maps_nvarchar_max_column(bool isKeyOrIndex)
        {
            var mapping = CreateMapper().FindMapping("nvarchar(max)", isKeyOrIndex, rowVersion: false);

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
            => new ScaffoldingTypeMapper(new SqliteTypeMapper());
    }
}
