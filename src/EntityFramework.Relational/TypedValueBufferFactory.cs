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
        private readonly Func<DbDataReader, int, object[]> _reader;
        private readonly int _offset;

        public TypedValueBufferFactory([NotNull] Func<DbDataReader, int, object[]> reader, int offset)
        {
            Check.NotNull(reader, nameof(reader));

            _reader = reader;
            _offset = offset;
        }

        public virtual ValueBuffer CreateValueBuffer(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            var values = new object[dataReader.FieldCount];

            dataReader.GetValues(values);

            return new ValueBuffer(_reader(dataReader, _offset));
        }
    }
}
