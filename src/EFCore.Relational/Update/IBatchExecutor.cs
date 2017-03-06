// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update
{
    public interface IBatchExecutor
    {
        int Execute(
            [NotNull] IEnumerable<ModificationCommandBatch> commandBatches,
            [NotNull] IRelationalConnection connection);

        Task<int> ExecuteAsync(
            [NotNull] IEnumerable<ModificationCommandBatch> commandBatches,
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
