// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            StringBuilder commandStringBuilder,
            IModificationCommand command,
            int commandPosition)
        {
            AppendInsertOperationCalls++;
            return base.AppendInsertOperation(commandStringBuilder, command, commandPosition);
        }

        public override ResultSetMapping AppendUpdateOperation(
            StringBuilder commandStringBuilder,
            IModificationCommand command,
            int commandPosition)
        {
            AppendUpdateOperationCalls++;
            return base.AppendUpdateOperation(commandStringBuilder, command, commandPosition);
        }

        public override ResultSetMapping AppendDeleteOperation(
            StringBuilder commandStringBuilder,
            IModificationCommand command,
            int commandPosition)
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

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, IColumnModification columnModification)
            => commandStringBuilder
                .Append(SqlGenerationHelper.DelimitIdentifier(columnModification.ColumnName))
                .Append(" = ")
                .Append("provider_specific_identity()");

        protected override ResultSetMapping AppendSelectAffectedCountCommand(
            StringBuilder commandStringBuilder,
            string name,
            string schema,
            int commandPosition)
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
