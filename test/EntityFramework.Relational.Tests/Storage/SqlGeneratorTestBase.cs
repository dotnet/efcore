using System;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class SqlGeneratorTestBase
    {
        [Fact]
        public virtual void GenerateLiteral_returns_bool_literal_when_true()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(true);
            Assert.Equal("1", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_bool_literal_when_false()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(false);
            Assert.Equal("0", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_char_literal()
        {
            var literal = CreateSqlGenerator().GenerateLiteral('A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_ByteArray_literal()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(new byte[] { 0xDA, 0x7A });
            Assert.Equal("X'DA7A'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_object_literal_when_null()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(default(object));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_object_literal_when_not_null()
        {
            var literal = CreateSqlGenerator().GenerateLiteral((object)42);
            Assert.Equal("42", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_Guid_literal()
        {
            var value = new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292");
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("'c6f43a9e-91e1-45ef-a320-832ea23b7292'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_NullableInt_literal_when_null()
        {
            var literal = CreateSqlGenerator().GenerateLiteral(default(int?));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_NullableInt_literal_when_not_null()
        {
            var literal = CreateSqlGenerator().GenerateLiteral((char?)'A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public void Default_BatchCommandSeparator_is_semicolon()
        {
            Assert.Equal(";", CreateSqlGenerator().BatchCommandSeparator);
        }

        [Fact]
        public virtual void BatchSeparator_returns_seperator()
        {
            Assert.Equal(string.Empty, CreateSqlGenerator().BatchSeparator);
        }

        protected abstract ISqlGenerator CreateSqlGenerator();
    }
}
