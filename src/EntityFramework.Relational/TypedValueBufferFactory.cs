// Copyright (c) .NET Foundation. All rights reserved.
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
        private readonly Func<DbDataReader, object[]> _valueFactory;

        public TypedValueBufferFactory([NotNull] Func<DbDataReader, object[]> valueFactory)
        {
            Check.NotNull(valueFactory, nameof(valueFactory));

            _valueFactory = valueFactory;
        }

        public virtual ValueBuffer Create(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            var values = _valueFactory(dataReader);

            return values.Length == 0
                ? ValueBuffer.Empty
                : new ValueBuffer(values);
        }
    }
}
