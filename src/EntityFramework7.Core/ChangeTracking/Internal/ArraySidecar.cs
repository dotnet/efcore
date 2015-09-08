// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class ArraySidecar : Sidecar
    {
        private readonly object[] _values;

        protected ArraySidecar([NotNull] InternalEntityEntry entry, int count)
            : base(entry)
        {
            _values = new object[count];
        }

        protected abstract int Index([NotNull] IPropertyBase property);

        protected abstract void ThrowInvalidIndexException([NotNull] IPropertyBase property);

        public override bool CanStoreValue(IPropertyBase property) => Index(property) != -1;

        protected override object ReadValue(IPropertyBase property) => _values[IndexChecked(property)];

        protected override void WriteValue(IPropertyBase property, object value)
        {
            _values[IndexChecked(property)] = value;
        }

        private int IndexChecked(IPropertyBase property)
        {
            var index = Index(property);
            if (index == -1)
            {
                ThrowInvalidIndexException(property);
            }
            return index;
        }
    }
}
