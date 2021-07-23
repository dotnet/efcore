// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public sealed partial class InternalEntityEntry
    {
        private readonly struct RelationshipsSnapshot
        {
            private readonly ISnapshot _values;

            public RelationshipsSnapshot(InternalEntityEntry entry)
            {
                _values = ((IRuntimeEntityType)entry.EntityType).RelationshipSnapshotFactory(entry);
            }

            public object? GetValue(InternalEntityEntry entry, IPropertyBase propertyBase)
                => IsEmpty ? entry[propertyBase] : _values[propertyBase.GetRelationshipIndex()];

            public T GetValue<T>(InternalEntityEntry entry, IPropertyBase propertyBase, int index)
                => IsEmpty
                    ? entry.GetCurrentValue<T>(propertyBase)
                    : _values.GetValue<T>(index);

            public void SetValue(IPropertyBase propertyBase, object? value)
            {
                if (value == null)
                {
                    if (propertyBase is IProperty property
                        && !property.IsNullable)
                    {
                        return;
                    }
                }

                Check.DebugAssert(!IsEmpty, "relationship snapshot is empty");
                Check.DebugAssert(
                    propertyBase is not INavigation { IsCollection : true },
                    $"property {propertyBase} is is not reference navigation");

                _values[propertyBase.GetRelationshipIndex()] = SnapshotValue(propertyBase, value);
            }

            private static object? SnapshotValue(IPropertyBase propertyBase, object? value)
            {
                if (propertyBase is IProperty property)
                {
                    var comparer = property.GetKeyValueComparer();

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
                    ((HashSet<object>)_values[index]!)?.Remove(removedEntity);
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
                var snapshot = (HashSet<object>?)_values[index];
                if (snapshot == null)
                {
                    snapshot = new HashSet<object>(LegacyReferenceEqualityComparer.Instance);
                    _values[index] = snapshot;
                }

                return snapshot;
            }

            public bool IsEmpty
                => _values == null;
        }
    }
}
