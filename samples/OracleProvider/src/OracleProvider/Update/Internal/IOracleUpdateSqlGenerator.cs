// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Oracle.Update.Internal
{
    public interface IOracleUpdateSqlGenerator : IUpdateSqlGenerator
    {
        ResultSetMapping AppendBatchInsertOperation(
            StringBuilder commandStringBuilder,
            Dictionary<string, string> variablesInsert,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            ref int cursorPosition);

        ResultSetMapping AppendBatchUpdateOperation(
            StringBuilder commandStringBuilder,
            StringBuilder variablesCommand,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            ref int cursorPosition);

        ResultSetMapping AppendBatchDeleteOperation(
            StringBuilder commandStringBuilder,
            StringBuilder variablesCommand,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            ref int cursorPosition);
    }
}
