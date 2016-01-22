// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class NullableKeyIdentityMap<TKey> : IdentityMap<TKey>
    {
        public NullableKeyIdentityMap(
            [NotNull] IKey key,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
            : base(key, principalKeyValueFactory)
        {
        }

        public override void Add(InternalEntityEntry entry)
        {
            var key = PrincipalKeyValueFactory.CreateFromCurrentValues(entry);

            if (key == null)
            {
                throw new InvalidOperationException(CoreStrings.InvalidKeyValue(entry.EntityType.DisplayName()));
            }

            Add(key, entry);
        }

        public override void RemoveUsingRelationshipSnapshot(InternalEntityEntry entry)
        {
            var key = PrincipalKeyValueFactory.CreateFromRelationshipSnapshot(entry);

            if (key != null)
            {
                Remove(key, entry);
            }
        }
    }
}
