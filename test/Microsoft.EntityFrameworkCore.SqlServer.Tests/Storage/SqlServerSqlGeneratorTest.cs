// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Relational.Tests.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Storage
{
    public class SqlServerSqlGeneratorTest : SqlGeneratorTestBase
    {
        [Fact]
        public override void GenerateLiteral_returns_ByteArray_literal()
        {
            var literal = CreateSqlGenerationHelper().GenerateLiteral(new byte[] { 0xDA, 0x7A });
            Assert.Equal("0xDA7A", literal);
        }

        [Fact]
        public override void GenerateLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = CreateSqlGenerationHelper().GenerateLiteral(value);

            Assert.Equal("'2015-03-12T13:36:37.371'", literal);
        }

        [Fact]
        public override void GenerateLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = CreateSqlGenerationHelper().GenerateLiteral(value);

            Assert.Equal("'2015-03-12T13:36:37.371-07:00'", literal);
        }

        [Fact]
        public override void BatchSeparator_returns_seperator()
        {
            Assert.Equal("GO" + Environment.NewLine + Environment.NewLine, CreateSqlGenerationHelper().BatchTerminator);
        }

        protected override ISqlGenerationHelper CreateSqlGenerationHelper()
            => new SqlServerSqlGenerationHelper();
    }
}
