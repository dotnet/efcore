// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public interface IPrincipalKeyValueFactory<TKey>
    {
        object CreateFromBuffer(ValueBuffer valueBuffer);
        TKey CreateFromCurrentValues([NotNull] InternalEntityEntry entry);
        TKey CreateFromOriginalValues([NotNull] InternalEntityEntry entry);
        TKey CreateFromRelationshipSnapshot([NotNull] InternalEntityEntry entry);
        IEqualityComparer<TKey> EqualityComparer { get; }
    }
}
