// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MetadataTracker : IReferenceRoot<ForeignKey>
    {
        private Dictionary<ForeignKey, Reference<ForeignKey>> _trackedForeignKeys = new Dictionary<ForeignKey, Reference<ForeignKey>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Update([NotNull] ForeignKey oldForeignKey, [NotNull] ForeignKey newForeignKey)
        {
            Debug.Assert(oldForeignKey.Builder == null && newForeignKey.Builder != null);

            if (_trackedForeignKeys.TryGetValue(oldForeignKey, out var reference))
            {
                _trackedForeignKeys.Remove(oldForeignKey);
                reference.Object = newForeignKey;
                _trackedForeignKeys.Add(newForeignKey, reference);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Reference<ForeignKey> Track(ForeignKey foreignKey)
        {
            if (_trackedForeignKeys.TryGetValue(foreignKey, out var reference))
            {
                reference.IncreaseReferenceCount();
                return reference;
            }

            reference = new Reference<ForeignKey>(foreignKey, this);
            _trackedForeignKeys.Add(foreignKey, reference);

            return reference;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void IReferenceRoot<ForeignKey>.Release(Reference<ForeignKey> foreignKeyReference)
        {
            _trackedForeignKeys.Remove(foreignKeyReference.Object);
        }
    }
}
