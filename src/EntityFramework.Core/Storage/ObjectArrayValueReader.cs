// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class ObjectArrayValueReader : IValueReader
    {
        private readonly object[] _valueBuffer;
        private readonly int _offset;

        public ObjectArrayValueReader([NotNull] object[] valueBuffer)
            : this(valueBuffer, 0)
        {
        }

        public ObjectArrayValueReader([NotNull] object[] valueBuffer, int offset)
        {
            Check.NotNull(valueBuffer, nameof(valueBuffer));

            _valueBuffer = valueBuffer;
            _offset = offset;
        }

        public virtual bool IsNull(int index) => _valueBuffer[_offset + index] == null;

        public virtual T ReadValue<T>(int index) => (T)_valueBuffer[_offset + index];

        public virtual int Count => _valueBuffer.Length - _offset;
    }
}
