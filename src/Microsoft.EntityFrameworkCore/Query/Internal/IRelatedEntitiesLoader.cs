// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public interface IRelatedEntitiesLoader : IDisposable
    {
        IEnumerable<EntityLoadInfo> Load(
            [NotNull] QueryContext queryContext,
            [NotNull] IIncludeKeyComparer keyComparer);
    }
}
