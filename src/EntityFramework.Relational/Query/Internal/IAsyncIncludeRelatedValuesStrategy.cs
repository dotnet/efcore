// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.Internal
{
    public interface IAsyncIncludeRelatedValuesStrategy : IDisposable
    {
        IAsyncEnumerable<EntityLoadInfo> GetRelatedValues(
            [NotNull] IIncludeKeyComparer keyComparer);
    }
}
