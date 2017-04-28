// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CompositePrincipalKeyValueFactory : CompositeValueFactory, IPrincipalKeyValueFactory<object[]>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CompositePrincipalKeyValueFactory([NotNull] IKey key)
            : base(key.Properties)
        {
            EqualityComparer = CreateEqualityComparer(key.Properties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateFromKeyValues(object[] keyValues)
            => keyValues.Any(v => v == null) ? null : keyValues;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateFromBuffer(ValueBuffer valueBuffer)
            => TryCreateFromBuffer(valueBuffer, out var values) ? values : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IProperty FindNullPropertyInValueBuffer(ValueBuffer valueBuffer)
            => Properties.FirstOrDefault(p => valueBuffer[p.GetIndex()] == null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object[] CreateFromCurrentValues(InternalEntityEntry entry)
            => CreateFromEntry(entry, (e, p) => e.GetCurrentValue(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IProperty FindNullPropertyInCurrentValues(InternalEntityEntry entry)
            => Properties.FirstOrDefault(p => entry.GetCurrentValue(p) == null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object[] CreateFromOriginalValues(InternalEntityEntry entry)
            => CreateFromEntry(entry, (e, p) => e.GetOriginalValue(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object[] CreateFromRelationshipSnapshot(InternalEntityEntry entry)
            => CreateFromEntry(entry, (e, p) => e.GetRelationshipSnapshotValue(p));

        private object[] CreateFromEntry(
            InternalEntityEntry entry,
            Func<InternalEntityEntry, IProperty, object> getValue)
        {
            var values = new object[Properties.Count];
            var index = 0;

            foreach (var property in Properties)
            {
                if ((values[index++] = getValue(entry, property)) == null)
                {
                    return null;
                }
            }

            return values;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEqualityComparer<object[]> EqualityComparer { get; }
    }
}
