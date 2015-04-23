// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class TypedValueBufferFactory : IRelationalValueBufferFactory
    {
        private readonly Func<DbDataReader, int, object[]> _valueFactory;
        private readonly int _offset;

        public TypedValueBufferFactory([NotNull] Func<DbDataReader, int, object[]> valueFactory, int offset)
        {
            Check.NotNull(valueFactory, nameof(valueFactory));

            _valueFactory = valueFactory;
            _offset = offset;
        }

        public virtual ValueBuffer CreateValueBuffer(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            var values = _valueFactory(dataReader, _offset);

            return values.Length == 0
                ? ValueBuffer.Empty
                : new ValueBuffer(values);
        }
    }
}
