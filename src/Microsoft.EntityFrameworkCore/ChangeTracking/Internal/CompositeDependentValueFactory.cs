// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class CompositeDependentValueFactory : IDependentKeyValueFactory<object[]>
    {
        private readonly IReadOnlyList<IProperty> _properties;

        public CompositeDependentValueFactory([NotNull] IForeignKey foreignKey)
        {
            _properties = foreignKey.Properties;
        }

        public virtual bool TryCreateFromBuffer(ValueBuffer valueBuffer, out object[] key)
        {
            key = new object[_properties.Count];
            var index = 0;

            foreach (var property in _properties)
            {
                if ((key[index++] = valueBuffer[property.GetIndex()]) == null)
                {
                    key = null;
                    return false;
                }
            }

            return true;
        }

        public virtual bool TryCreateFromCurrentValues(InternalEntityEntry entry, out object[] key)
            => TryCreateFromEntry(entry, (e, p) => e.GetCurrentValue(p), out key);

        public virtual bool TryCreateFromOriginalValues(InternalEntityEntry entry, out object[] key)
            => TryCreateFromEntry(entry, (e, p) => e.GetOriginalValue(p), out key);

        public virtual bool TryCreateFromRelationshipSnapshot(InternalEntityEntry entry, out object[] key)
            => TryCreateFromEntry(entry, (e, p) => e.GetRelationshipSnapshotValue(p), out key);

        private bool TryCreateFromEntry(
            InternalEntityEntry entry,
            Func<InternalEntityEntry, IProperty, object> getValue,
            out object[] key)
        {
            key = new object[_properties.Count];
            var index = 0;

            foreach (var property in _properties)
            {
                if ((key[index++] = getValue(entry, property)) == null)
                {
                    key = null;
                    return false;
                }
            }

            return true;
        }
    }
}
