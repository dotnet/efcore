// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public struct ValueBuffer
    {
        private readonly object[] _values;
        private readonly int _offset;

        public ValueBuffer([NotNull] object[] values)
            : this(values, 0, values.Length)
        {
        }

        public ValueBuffer([NotNull] object[] values, int offset, int count)
        {
            Check.NotNull(values, nameof(values));

            _values = values;
            _offset = offset;
            Count = count;
        }

        public object this[int index] => _values[_offset + index];

        public int Count { get; }
    }
}
