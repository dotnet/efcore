// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        private Reference<ForeignKey> _trackedForeignKey;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Update([NotNull] ForeignKey oldForeignKey, [NotNull] ForeignKey newForeignKey)
        {
            Debug.Assert(oldForeignKey.Builder == null && newForeignKey.Builder != null);

            if (_trackedForeignKey?.Object == oldForeignKey)
            {
                _trackedForeignKey.Object = newForeignKey;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Reference<ForeignKey> Track(ForeignKey foreignKey)
        {
            var canTrack = _trackedForeignKey == null;
            var reference = new Reference<ForeignKey>(foreignKey, canTrack ? this : null);
            if (canTrack)
            {
                _trackedForeignKey = reference;
            }

            return reference;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void IReferenceRoot<ForeignKey>.Release(Reference<ForeignKey> foreignKeyReference)
        {
            Debug.Assert(foreignKeyReference == _trackedForeignKey);
            _trackedForeignKey = null;
        }
    }
}
