// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class OffsetValueReaderDecorator : IValueReader
    {
        private readonly IValueReader _valueReader;
        private readonly int _offset;

        public OffsetValueReaderDecorator([NotNull] IValueReader valueReader, int offset)
        {
            Check.NotNull(valueReader, "valueReader");

            _valueReader = valueReader;
            _offset = offset;
        }

        public virtual bool IsNull(int index)
        {
            return _valueReader.IsNull(_offset + index);
        }

        public virtual T ReadValue<T>(int index)
        {
            return _valueReader.ReadValue<T>(_offset + index);
        }

        public virtual int Count
        {
            get { return _valueReader.Count; }
        }
    }
}
