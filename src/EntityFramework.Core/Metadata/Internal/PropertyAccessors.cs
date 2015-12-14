// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public sealed class PropertyAccessors
    {
        public PropertyAccessors(
            [NotNull] Delegate currentValueGetter,
            [CanBeNull] Delegate originalValueGetter,
            [NotNull] Delegate relationshipSnapshotGetter,
            [CanBeNull] Func<ValueBuffer, object> valueBufferGetter)
        {
            CurrentValueGetter = currentValueGetter;
            OriginalValueGetter = originalValueGetter;
            RelationshipSnapshotGetter = relationshipSnapshotGetter;
            ValueBufferGetter = valueBufferGetter;
        }

        public Delegate CurrentValueGetter { get; }
        public Delegate OriginalValueGetter { get; }
        public Delegate RelationshipSnapshotGetter { get; }
        public Func<ValueBuffer, object> ValueBufferGetter { get; }
    }
}
