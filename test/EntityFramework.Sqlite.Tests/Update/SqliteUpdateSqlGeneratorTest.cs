// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Sqlite.Internal;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Update
{
    public class SqliteUpdateSqlGeneratorTest : SqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator() => new SqliteUpdateSqlGenerator();
        protected override string RowsAffected => "changes()";
        protected override string Identity => "last_insert_rowid()";
        protected override string Schema => null;

        public override void GenerateNextSequenceValueOperation_correctly_handles_schemas()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.GenerateNextSequenceValueOperation_correctly_handles_schemas());
            Assert.Equal(Strings.SequencesNotSupported, ex.Message);
        }

        public override void GenerateNextSequenceValueOperation_returns_statement_with_sanatized_sequence()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.GenerateNextSequenceValueOperation_returns_statement_with_sanatized_sequence());
            Assert.Equal(Strings.SequencesNotSupported, ex.Message);
        }

        public override void GenerateLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("'2015-03-12 13:36:37.371-07:00'", literal);
        }

        public override void GenerateLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = CreateSqlGenerator().GenerateLiteral(value);
            Assert.Equal("'2015-03-12 13:36:37.371'", literal);
        }
    }
}
