// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RemappingUntypedRelationalValueBufferFactory : IRelationalValueBufferFactory
    {
        private readonly IReadOnlyList<int> _indexMap;

        public RemappingUntypedRelationalValueBufferFactory([NotNull] IReadOnlyList<int> indexMap)
        {
            Check.NotNull(indexMap, nameof(indexMap));

            _indexMap = indexMap;
        }

        public virtual ValueBuffer Create(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path
            Debug.Assert(dataReader.FieldCount >= _indexMap.Count);

            if (_indexMap.Count == 0)
            {
                return ValueBuffer.Empty;
            }

            var values = new object[dataReader.FieldCount];

            dataReader.GetValues(values);

            var remappedValues = new object[_indexMap.Count];

            for (var i = 0; i < _indexMap.Count; i++)
            {
                remappedValues[i]
                    = ReferenceEquals(values[_indexMap[i]], DBNull.Value)
                        ? null
                        : values[_indexMap[i]];
            }

            return new ValueBuffer(remappedValues);
        }
    }
}
