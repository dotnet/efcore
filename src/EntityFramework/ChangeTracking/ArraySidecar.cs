// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class ArraySidecar : Sidecar
    {
        private readonly object[] _values;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ArraySidecar()
        {
        }

        protected ArraySidecar([NotNull] StateEntry stateEntry, int count)
            : base(stateEntry)
        {
            _values = new object[count];
        }

        protected abstract int Index([NotNull] IPropertyBase property);
        protected abstract void ThrowInvalidIndexException([NotNull] IPropertyBase property);

        public override bool CanStoreValue(IPropertyBase property)
        {
            Check.NotNull(property, "property");

            return Index(property) != -1;
        }

        protected override object ReadValue(IPropertyBase property)
        {
            Check.NotNull(property, "property");

            return _values[IndexChecked(property)];
        }

        protected override void WriteValue(IPropertyBase property, object value)
        {
            Check.NotNull(property, "property");

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
