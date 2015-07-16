// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public interface IIncludeRelatedValuesStrategy : IDisposable
    {
        IEnumerable<EntityLoadInfo> GetRelatedValues(
            [NotNull] EntityKey key, [NotNull] Func<ValueBuffer, EntityKey> keyFactory);
    }
}
