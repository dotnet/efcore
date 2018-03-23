// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Update.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class SqliteUpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator()
            => new SqliteUpdateSqlGenerator(
                new UpdateSqlGeneratorDependencies(
                    new SqliteSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    TestServiceFactory.Instance.Create<SqliteTypeMappingSource>()));

        protected override TestHelpers TestHelpers => SqliteTestHelpers.Instance;

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
