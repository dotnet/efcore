// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Update
{
    public interface IUpdateSqlGenerator
    {
        string GenerateNextSequenceValueOperation([NotNull] string name, [CanBeNull] string schema);

        void AppendNextSequenceValueOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] string name, [CanBeNull] string schema);

        void AppendBatchHeader([NotNull] StringBuilder commandStringBuilder);

        ResultSetMapping AppendDeleteOperation(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command, int commandPosition);

        ResultSetMapping AppendInsertOperation(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command, int commandPosition);

        ResultSetMapping AppendUpdateOperation(
            [NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command, int commandPosition);
    }
}
