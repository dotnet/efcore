// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace Microsoft.EntityFrameworkCore.Relational.Tests.Storage
{
    public abstract class SqlGeneratorTestBase
    {
        [Fact]
        public virtual void GenerateParameterName_returns_parameter_name()
        {
            var name = CreateSqlGenerationHelper().GenerateParameterName("name");
            Assert.Equal("@name", name);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_bool_literal_when_true()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral(true);
            Assert.Equal("1", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_bool_literal_when_false()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral(false);
            Assert.Equal("0", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_char_literal()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral('A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_ByteArray_literal()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral(new byte[] { 0xDA, 0x7A });
            Assert.Equal("X'DA7A'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_object_literal_when_null()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral(default(object));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_object_literal_when_not_null()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral(42);
            Assert.Equal("42", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_Guid_literal()
        {
            var value = new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292");
            var literal = CreateSqlGenerationHelper().GenerateLiteral(value);
            Assert.Equal("'c6f43a9e-91e1-45ef-a320-832ea23b7292'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = CreateSqlGenerationHelper().GenerateLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = CreateSqlGenerationHelper().GenerateLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_NullableInt_literal_when_null()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral(default(int?));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateLiteral_returns_NullableInt_literal_when_not_null()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral((char?)'A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public void Default_BatchCommandSeparator_is_semicolon()
        {
            Assert.Equal(";", CreateSqlGenerationHelper().StatementTerminator);
        }

        [Fact]
        public virtual void BatchSeparator_returns_seperator()
        {
            Assert.Equal(string.Empty, CreateSqlGenerationHelper().BatchTerminator);
        }

        protected abstract ISqlGenerationHelper CreateSqlGenerationHelper();
    }
}
