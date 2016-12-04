// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Update
{
    public class UpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator() => new ConcreteSqlGenerator();

        protected override string RowsAffected => "provider_specific_rowcount()";

        protected override string Identity => "provider_specific_identity()";

        private class ConcreteSqlGenerator : UpdateSqlGenerator
        {
            public ConcreteSqlGenerator()
                : base(new RelationalSqlGenerationHelper())
            {
            }

            protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
                => commandStringBuilder
                    .Append(SqlGenerationHelper.DelimitIdentifier(columnModification.ColumnName))
                    .Append(" = ")
                    .Append("provider_specific_identity()");

            protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
            {
                commandStringBuilder
                    .Append("SELECT provider_specific_rowcount();" + Environment.NewLine + Environment.NewLine);

                return ResultSetMapping.LastInResultSet;
            }

            protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
                => commandStringBuilder
                    .Append("provider_specific_rowcount() = " + expectedRowsAffected);
        }
    }
}
