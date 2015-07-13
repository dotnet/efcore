// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class OriginalValues : ArraySidecar
    {
        public OriginalValues([NotNull] InternalEntityEntry entry)
            : base(entry, entry.EntityType.OriginalValueCount())
        {
        }

        protected override int Index(IPropertyBase property) => (property as IProperty)?.GetOriginalValueIndex() ?? -1;

        protected override void ThrowInvalidIndexException(IPropertyBase property)
        {
            throw new InvalidOperationException(Strings.OriginalValueNotTracked(property.Name, InternalEntityEntry.EntityType.Name));
        }

        public override string Name => WellKnownNames.OriginalValues;

        public override bool TransparentRead => false;

        public override bool TransparentWrite => false;

        public override bool AutoCommit => false;
    }
}
