// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Update;
using System.Text;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeSqlGenerator : UpdateSqlGenerator
    {
        public FakeSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override ResultSetMapping AppendInsertOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            AppendInsertOperationCalls++;

            if (!string.IsNullOrEmpty(command.Schema))
            {
                commandStringBuilder.Append(command.Schema + ".");
            }
            commandStringBuilder.Append(command.TableName);

            return ResultSetMapping.LastInResultSet;
        }

        public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            AppendUpdateOperationCalls++;
            return base.AppendUpdateOperation(commandStringBuilder, command, commandPosition);
        }

        public override ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
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

        protected override ResultSetMapping AppendSelectAffectedCountCommand(
            StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            return ResultSetMapping.NoResultSet;
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
        }
    }
}
