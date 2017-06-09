// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeSqlGenerator : UpdateSqlGenerator
    {
        public FakeSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public int AppendBatchHeaderCalls { get; set; }

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            AppendBatchHeaderCalls++;
            base.AppendBatchHeader(commandStringBuilder);
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModificationBase columnModification)
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
