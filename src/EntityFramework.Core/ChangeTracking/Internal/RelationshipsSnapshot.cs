// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        private struct RelationshipsSnapshot
        {
            private readonly object[] _values;

            public RelationshipsSnapshot(InternalEntityEntry entry)
            {
                _values = entry.EntityType.GetRelationshipSnapshotFactory()(entry);
            }

            public object GetValue(InternalEntityEntry entry, IPropertyBase propertyBase)
                => IsEmpty ? entry[propertyBase] : _values[propertyBase.GetRelationshipIndex()];

            public void SetValue(IPropertyBase propertyBase, object value)
            {
                if (value == null)
                {
                    var property = propertyBase as IProperty;
                    if ((property != null)
                        && !property.IsNullable)
                    {
                        return;
                    }
                }

                Debug.Assert(!IsEmpty);
                Debug.Assert(!(propertyBase is INavigation) || !((INavigation)propertyBase).IsCollection());

                _values[propertyBase.GetRelationshipIndex()] = value;
            }

            public void RemoveFromCollection(IPropertyBase propertyBase, object removedEntity)
                => ((HashSet<object>)_values[propertyBase.GetRelationshipIndex()]).Remove(removedEntity);

            public void AddToCollection(IPropertyBase propertyBase, object addedEntity)
            {
                var index = propertyBase.GetRelationshipIndex();

                var snapshot = (HashSet<object>)_values[index];
                if (snapshot == null)
                {
                    snapshot = new HashSet<object>(ReferenceEqualityComparer.Instance);
                    _values[index] = snapshot;
                }

                snapshot.Add(addedEntity);
            }

            public bool IsEmpty => _values == null;
        }
    }
}
