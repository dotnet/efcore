// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalObjectArrayValueReader : IValueReader
    {
        private readonly object[] _values;
        private readonly int _offset;

        public RelationalObjectArrayValueReader([NotNull] object[] values, int offset)
        {
            Debug.Assert(values != null); // hot path

            _values = values;
            _offset = offset;
        }

        public virtual bool IsNull(int index) => ReferenceEquals(_values[_offset + index], DBNull.Value);

        public virtual T ReadValue<T>(int index) => (T)_values[_offset + index];

        public virtual int Count => _values.Length - _offset;
    }
}
