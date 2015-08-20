// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class ValueBufferSidecar : Sidecar
    {
        private ValueBuffer _values;

        protected ValueBufferSidecar([NotNull] InternalEntityEntry entry, ValueBuffer values)
            : base(entry)
        {
            _values = values;
        }

        public override bool CanStoreValue(IPropertyBase property)
            => property is IProperty;

        protected override object ReadValue(IPropertyBase property)
            => _values[((IProperty)property).Index] ?? NullSentinel.Value;

        protected override void WriteValue(IPropertyBase property, object value)
            => _values[((IProperty)property).Index] = value;
    }
}
