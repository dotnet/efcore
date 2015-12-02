// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public sealed class PropertyAccessors
    {
        public PropertyAccessors(
            [NotNull] Delegate currentValueGetter,
            [NotNull] Delegate originalValueGetter,
            [NotNull] Delegate relationshipSnapshotGetter)
        {
            CurrentValueGetter = currentValueGetter;
            OriginalValueGetter = originalValueGetter;
            RelationshipSnapshotGetter = relationshipSnapshotGetter;
        }

        public Delegate CurrentValueGetter { get; }
        public Delegate OriginalValueGetter { get; }
        public Delegate RelationshipSnapshotGetter { get; }
    }
}
