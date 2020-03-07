// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class MetadataTracker : IReferenceRoot<IConventionForeignKey>
    {
        private readonly Dictionary<IConventionForeignKey, Reference<IConventionForeignKey>> _trackedForeignKeys =
            new Dictionary<IConventionForeignKey, Reference<IConventionForeignKey>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Update([NotNull] ForeignKey oldForeignKey, [NotNull] ForeignKey newForeignKey)
        {
            Check.DebugAssert(
                oldForeignKey.Builder == null && newForeignKey.Builder != null,
                "oldForeignKey.Builder is not null or newForeignKey.Builder is null");

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
        public virtual Reference<IConventionForeignKey> Track(IConventionForeignKey foreignKey)
        {
            if (_trackedForeignKeys.TryGetValue(foreignKey, out var reference))
            {
                reference.IncreaseReferenceCount();
                return reference;
            }

            reference = new Reference<IConventionForeignKey>(foreignKey, this);
            _trackedForeignKeys.Add(foreignKey, reference);

            return reference;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IReferenceRoot<IConventionForeignKey>.Release(Reference<IConventionForeignKey> foreignKeyReference)
        {
            _trackedForeignKeys.Remove(foreignKeyReference.Object);
        }
    }
}
