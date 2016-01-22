// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class InMemoryTableSnapshot
    {
        public InMemoryTableSnapshot(
            [NotNull] IEntityType entityType,
            [NotNull] IReadOnlyList<object[]> rows)
        {
            EntityType = entityType;
            Rows = rows;
        }

        public virtual IEntityType EntityType { get; }

        public virtual IReadOnlyList<object[]> Rows { get; }
    }
}
