// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Query
{
    public interface IQueryBuffer
    {
        object GetEntity([NotNull] IEntityType entityType, [NotNull] IValueReader valueReader);
        StateEntry TryGetStateEntry([NotNull] object entity);
    }
}
