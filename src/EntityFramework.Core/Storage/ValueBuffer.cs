// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public struct ValueBuffer
    {
        public static readonly ValueBuffer Empty = new ValueBuffer();

        private readonly object[] _values;
        private readonly int _offset;

        public ValueBuffer([NotNull] object[] values)
            : this(values, 0)
        {
        }

        public ValueBuffer([NotNull] object[] values, int offset)
        {
            Debug.Assert(values != null);

            _values = values;
            _offset = offset;
        }

        public object this[int index] => _values[_offset + index];

        public int Count => _values.Length - _offset;
    }
}
