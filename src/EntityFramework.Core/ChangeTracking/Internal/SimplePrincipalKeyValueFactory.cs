// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class SimplePrincipalKeyValueFactory<TKey> : IPrincipalKeyValueFactory<TKey>
    {
        private readonly PropertyAccessors _propertyAccessors;

        public SimplePrincipalKeyValueFactory([NotNull] PropertyAccessors propertyAccessors)
        {
            _propertyAccessors = propertyAccessors;
        }

        public virtual object CreateFromBuffer(ValueBuffer valueBuffer)
            => _propertyAccessors.ValueBufferGetter(valueBuffer);

        public virtual TKey CreateFromCurrentValues(InternalEntityEntry entry)
            => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.CurrentValueGetter)(entry);

        public virtual TKey CreateFromOriginalValues(InternalEntityEntry entry)
            => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.OriginalValueGetter)(entry);

        public virtual TKey CreateFromRelationshipSnapshot(InternalEntityEntry entry)
            => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.RelationshipSnapshotGetter)(entry);
    }
}
