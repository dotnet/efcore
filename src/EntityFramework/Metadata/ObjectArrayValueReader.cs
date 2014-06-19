// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ObjectArrayValueReader : IValueReader
    {
        private readonly object[] _valueBuffer;

        public ObjectArrayValueReader([NotNull] object[] valueBuffer)
        {
            Check.NotNull(valueBuffer, "valueBuffer");

            _valueBuffer = valueBuffer;
        }

        public virtual bool IsNull(int index)
        {
            return _valueBuffer[index] == null;
        }

        public virtual T ReadValue<T>(int index)
        {
            return (T)_valueBuffer[index];
        }

        public virtual int Count
        {
            get { return _valueBuffer.Length; }
        }
    }
}
