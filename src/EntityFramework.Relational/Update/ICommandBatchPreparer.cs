// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Update
{
    public interface ICommandBatchPreparer
    {
        IEnumerable<ModificationCommandBatch> BatchCommands(
            [NotNull] IReadOnlyList<InternalEntityEntry> entries, [NotNull] IDbContextOptions options);
    }
}
