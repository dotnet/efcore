// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeSqlGenerator(UpdateSqlGeneratorDependencies dependencies) : UpdateSqlGenerator(dependencies)
{
    public override ResultSetMapping AppendInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        AppendInsertOperationCalls++;
        return base.AppendInsertOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);
    }

    public override ResultSetMapping AppendUpdateOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        AppendUpdateOperationCalls++;
        return base.AppendUpdateOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);
    }

    public override ResultSetMapping AppendDeleteOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        AppendDeleteOperationCalls++;
        return base.AppendDeleteOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);
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
}
