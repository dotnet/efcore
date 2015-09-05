// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class RemappingUntypedRelationalValueBufferFactory : IRelationalValueBufferFactory
    {
        private readonly IReadOnlyList<int> _indexMap;
        private readonly Action<object[]> _processValuesAction;

        public RemappingUntypedRelationalValueBufferFactory(
            [NotNull] IReadOnlyList<int> indexMap,
            [CanBeNull] Action<object[]> processValuesAction)
        {
            _indexMap = indexMap;
            _processValuesAction = processValuesAction;
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

            _processValuesAction?.Invoke(values);

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
