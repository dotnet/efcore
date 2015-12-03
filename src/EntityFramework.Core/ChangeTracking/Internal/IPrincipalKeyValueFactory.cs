// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IPrincipalKeyValueFactory<out TKey>
    {
        object CreateFromBuffer(ValueBuffer valueBuffer);
        TKey CreateFromCurrentValues([NotNull] InternalEntityEntry entry);
        TKey CreateFromOriginalValues([NotNull] InternalEntityEntry entry);
        TKey CreateFromRelationshipSnapshot([NotNull] InternalEntityEntry entry);
    }
}
