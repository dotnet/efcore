// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class CompositeDependentValueFactory : IDependentKeyValueFactory<IKeyValue>
    {
        private readonly IForeignKey _foreignKey;

        public CompositeDependentValueFactory([NotNull] IForeignKey foreignKey)
        {
            _foreignKey = foreignKey;
        }

        public virtual bool TryCreateFromBuffer(ValueBuffer valueBuffer, out IKeyValue key)
        {
            key = new CompositeKeyValueFactory(_foreignKey.PrincipalKey).Create(_foreignKey.Properties, valueBuffer);
            return !key.IsInvalid;
        }

        public virtual bool TryCreateFromCurrentValues(InternalEntityEntry entry, out IKeyValue key)
        {
            key = entry.GetDependentKeyValue(_foreignKey);
            return !key.IsInvalid;
        }

        public virtual bool TryCreateFromOriginalValues(InternalEntityEntry entry, out IKeyValue key)
        {
            key = entry.GetDependentKeyValue(_foreignKey, ValueSource.Original);
            return !key.IsInvalid;
        }

        public virtual bool TryCreateFromRelationshipSnapshot(InternalEntityEntry entry, out IKeyValue key)
        {
            key = entry.GetDependentKeyValue(_foreignKey, ValueSource.RelationshipSnapshot);
            return !key.IsInvalid;
        }
    }
}
