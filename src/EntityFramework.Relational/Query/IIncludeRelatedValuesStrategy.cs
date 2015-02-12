// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Query
{
    public interface IIncludeRelatedValuesStrategy : IDisposable
    {
        IEnumerable<IValueReader> GetRelatedValues(
            [NotNull] EntityKey key, [NotNull] Func<IValueReader, EntityKey> keyFactory);
    }
}
