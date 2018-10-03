// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        private readonly struct RelationshipsSnapshot
        {
            private readonly ISnapshot _values;

            public RelationshipsSnapshot(InternalEntityEntry entry)
            {
                _values = ((EntityType)entry.EntityType).RelationshipSnapshotFactory(entry);
            }

            public object GetValue(InternalEntityEntry entry, IPropertyBase propertyBase)
                => IsEmpty ? entry[propertyBase] : _values[propertyBase.GetRelationshipIndex()];

            public T GetValue<T>(InternalEntityEntry entry, IPropertyBase propertyBase, int index)
                => IsEmpty
                    ? entry.GetCurrentValue<T>(propertyBase)
                    : _values.GetValue<T>(index);

            public void SetValue(IPropertyBase propertyBase, object value)
            {
                if (value == null)
                {
                    if (propertyBase is IProperty property
                        && !property.IsNullable)
                    {
                        return;
                    }
                }

                Debug.Assert(!IsEmpty);
                Debug.Assert(!(propertyBase is INavigation) || !((INavigation)propertyBase).IsCollection());

                _values[propertyBase.GetRelationshipIndex()] = SnapshotValue(propertyBase, value);
            }

            private static object SnapshotValue(IPropertyBase propertyBase, object value)
            {
                if (propertyBase is IProperty property)
                {
                    var comparer = property.GetKeyValueComparer() ?? property.FindMapping()?.KeyComparer;

                    if (comparer != null)
                    {
                        return comparer.Snapshot(value);
                    }
                }

                return value;
            }

            public void RemoveFromCollection(IPropertyBase propertyBase, object removedEntity)
            {
                var index = propertyBase.GetRelationshipIndex();
                if (index != -1)
                {
                    ((HashSet<object>)_values[index])?.Remove(removedEntity);
                }
            }

            public void AddToCollection(IPropertyBase propertyBase, object addedEntity)
            {
                var index = propertyBase.GetRelationshipIndex();

                if (index != -1)
                {
                    var snapshot = GetOrCreateCollection(index);

                    snapshot.Add(addedEntity);
                }
            }

            public void AddRangeToCollection(IPropertyBase propertyBase, IEnumerable<object> addedEntities)
            {
                var index = propertyBase.GetRelationshipIndex();

                if (index != -1)
                {
                    var snapshot = GetOrCreateCollection(index);

                    foreach (var addedEntity in addedEntities)
                    {
                        snapshot.Add(addedEntity);
                    }
                }
            }

            private HashSet<object> GetOrCreateCollection(int index)
            {
                var snapshot = (HashSet<object>)_values[index];
                if (snapshot == null)
                {
                    snapshot = new HashSet<object>(ReferenceEqualityComparer.Instance);
                    _values[index] = snapshot;
                }

                return snapshot;
            }

            public bool IsEmpty => _values == null;
        }
    }
}
