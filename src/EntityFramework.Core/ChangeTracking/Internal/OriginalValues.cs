// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class OriginalValues : ArraySidecar
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected OriginalValues()
        {
        }

        public OriginalValues([NotNull] InternalEntityEntry entry)
            : base(entry, entry.EntityType.OriginalValueCount())
        {
        }

        protected override int Index(IPropertyBase property) => (property as IProperty)?.OriginalValueIndex ?? -1;

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
