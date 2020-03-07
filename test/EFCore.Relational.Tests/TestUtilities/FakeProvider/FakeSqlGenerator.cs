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

        public override ResultSetMapping AppendInsertOperation(
            StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            AppendInsertOperationCalls++;
            return base.AppendInsertOperation(commandStringBuilder, command, commandPosition);
        }

        public override ResultSetMapping AppendUpdateOperation(
            StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            AppendUpdateOperationCalls++;
            return base.AppendUpdateOperation(commandStringBuilder, command, commandPosition);
        }

        public override ResultSetMapping AppendDeleteOperation(
            StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            AppendDeleteOperationCalls++;
            return base.AppendDeleteOperation(commandStringBuilder, command, commandPosition);
        }

        public int AppendBatchHeaderCalls { get; set; }
        public int AppendInsertOperationCalls { get; set; }
        public int AppendUpdateOperationCalls { get; set; }
        public int AppendDeleteOperationCalls { get; set; }

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            AppendBatchHeaderCalls++;
            base.AppendBatchHeader(commandStringBuilder);
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
            => commandStringBuilder
                .Append(SqlGenerationHelper.DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append("provider_specific_identity()");

        protected override ResultSetMapping AppendSelectAffectedCountCommand(
            StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            commandStringBuilder
                .Append("SELECT provider_specific_rowcount();").Append(Environment.NewLine).Append(Environment.NewLine);

            return ResultSetMapping.LastInResultSet;
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => commandStringBuilder
                .Append("provider_specific_rowcount() = ").Append(expectedRowsAffected);
    }
}
