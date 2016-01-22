// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public interface IWeakReferenceIdentityMap
    {
        IKey Key { get; }

        WeakReference<object> TryGetEntity(ValueBuffer valueBuffer, out bool hasNullKey);

        void CollectGarbage();

        void Add(ValueBuffer valueBuffer, [NotNull] object entity);

        IIncludeKeyComparer CreateIncludeKeyComparer([NotNull] INavigation navigation, ValueBuffer valueBuffer);

        IIncludeKeyComparer CreateIncludeKeyComparer([NotNull] INavigation navigation, [NotNull] InternalEntityEntry entry);
    }
}
