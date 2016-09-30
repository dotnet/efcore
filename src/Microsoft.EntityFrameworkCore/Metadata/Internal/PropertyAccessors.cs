// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public sealed class PropertyAccessors
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public PropertyAccessors(
            [NotNull] Delegate currentValueGetter,
            [NotNull] Delegate preStoreGeneratedCurrentValueGetter,
            [CanBeNull] Delegate originalValueGetter,
            [NotNull] Delegate relationshipSnapshotGetter,
            [CanBeNull] Func<ValueBuffer, object> valueBufferGetter)
        {
            CurrentValueGetter = currentValueGetter;
            PreStoreGeneratedCurrentValueGetter = preStoreGeneratedCurrentValueGetter;
            OriginalValueGetter = originalValueGetter;
            RelationshipSnapshotGetter = relationshipSnapshotGetter;
            ValueBufferGetter = valueBufferGetter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Delegate CurrentValueGetter { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Delegate PreStoreGeneratedCurrentValueGetter { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Delegate OriginalValueGetter { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Delegate RelationshipSnapshotGetter { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Func<ValueBuffer, object> ValueBufferGetter { get; }
    }
}
