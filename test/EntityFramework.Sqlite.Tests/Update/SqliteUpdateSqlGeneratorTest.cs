// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.Update.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Update
{
    public class SqliteUpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator()
            => new SqliteUpdateSqlGenerator(new SqliteSqlGenerationHelper());

        protected override string RowsAffected => "changes()";
        protected override string Identity => "last_insert_rowid()";
        protected override string Schema => null;

        public override void GenerateNextSequenceValueOperation_correctly_handles_schemas()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.GenerateNextSequenceValueOperation_correctly_handles_schemas());
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }

        public override void GenerateNextSequenceValueOperation_returns_statement_with_sanatized_sequence()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.GenerateNextSequenceValueOperation_returns_statement_with_sanatized_sequence());
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }


    }
}
