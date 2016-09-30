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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CompositeDependentValueFactory : IDependentKeyValueFactory<object[]>
    {
        private readonly IReadOnlyList<IProperty> _properties;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CompositeDependentValueFactory([NotNull] IForeignKey foreignKey)
        {
            _properties = foreignKey.Properties;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromCurrentValues(InternalEntityEntry entry, out object[] key)
            => TryCreateFromEntry(entry, (e, p) => e.GetCurrentValue(p), out key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(InternalEntityEntry entry, out object[] key)
            => TryCreateFromEntry(entry, (e, p) => e.GetPreStoreGeneratedCurrentValue(p), out key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromOriginalValues(InternalEntityEntry entry, out object[] key)
            => TryCreateFromEntry(entry, (e, p) => e.GetOriginalValue(p), out key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
