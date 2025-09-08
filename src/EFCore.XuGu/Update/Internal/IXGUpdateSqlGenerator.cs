// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.XuGu.Update.Internal
{
    public interface IXGUpdateSqlGenerator : IUpdateSqlGenerator
    {
        ResultSetMapping AppendBulkInsertOperation(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
            int commandPosition,
            out bool requiresTransaction);
    }
}
