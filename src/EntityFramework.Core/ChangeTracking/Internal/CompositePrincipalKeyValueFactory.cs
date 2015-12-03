// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class CompositePrincipalKeyValueFactory : IPrincipalKeyValueFactory<IKeyValue>
    {
        private readonly IKey _key;

        public CompositePrincipalKeyValueFactory([NotNull] IKey key)
        {
            _key = key;
        }

        public virtual object CreateFromBuffer(ValueBuffer valueBuffer)
        {
            var value = new CompositeKeyValueFactory(_key).Create(valueBuffer);
            return value.IsInvalid ? null : value;
        }

        public virtual IKeyValue CreateFromCurrentValues(InternalEntityEntry entry)
            => entry.GetPrincipalKeyValue(_key);

        public virtual IKeyValue CreateFromOriginalValues(InternalEntityEntry entry)
            => entry.GetPrincipalKeyValue(_key, ValueSource.Original);

        public virtual IKeyValue CreateFromRelationshipSnapshot(InternalEntityEntry entry)
            => entry.GetPrincipalKeyValue(_key, ValueSource.RelationshipSnapshot);
    }
}
