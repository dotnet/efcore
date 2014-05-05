// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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

        protected abstract int Index([NotNull] IProperty property);
        protected abstract void ThrowInvalidIndexException([NotNull] IProperty property);

        public override bool CanStoreValue(IProperty property)
        {
            Check.NotNull(property, "property");

            return Index(property) != -1;
        }

        protected override object ReadValue(IProperty property)
        {
            Check.NotNull(property, "property");

            return _values[IndexChecked(property)];
        }

        protected override void WriteValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            _values[IndexChecked(property)] = value;
        }

        private int IndexChecked(IProperty property)
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
