// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.Relational.Update
{
    public interface ICommandBatchPreparer
    {
        IEnumerable<ModificationCommandBatch> BatchCommands(
            [NotNull] IReadOnlyList<InternalEntityEntry> entries, [NotNull] IDbContextOptions options);

        IRelationalPropertyExtensions GetPropertyExtensions([NotNull] IProperty property);

        IRelationalEntityTypeExtensions GetEntityTypeExtensions([NotNull] IEntityType entityType);
    }
}
