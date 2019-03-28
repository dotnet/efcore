// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class MetadataTracker : IReferenceRoot<ForeignKey>
    {
        private readonly Dictionary<ForeignKey, Reference<ForeignKey>> _trackedForeignKeys =
            new Dictionary<ForeignKey, Reference<ForeignKey>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IReferenceRoot<ForeignKey>.Release(Reference<ForeignKey> foreignKeyReference)
        {
            _trackedForeignKeys.Remove(foreignKeyReference.Object);
        }
    }
}
