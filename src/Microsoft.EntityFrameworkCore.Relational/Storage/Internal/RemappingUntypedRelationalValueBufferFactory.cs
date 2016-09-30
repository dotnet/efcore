// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RemappingUntypedRelationalValueBufferFactory : IRelationalValueBufferFactory
    {
        private readonly IReadOnlyList<int> _indexMap;
        private readonly Action<object[]> _processValuesAction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RemappingUntypedRelationalValueBufferFactory(
            [NotNull] IReadOnlyList<int> indexMap,
            [CanBeNull] Action<object[]> processValuesAction)
        {
            _indexMap = indexMap;
            _processValuesAction = processValuesAction;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                remappedValues[i] = values[_indexMap[i]];
            }

            _processValuesAction?.Invoke(remappedValues);

            for (var i = 0; i < _indexMap.Count; i++)
            {
                if (ReferenceEquals(remappedValues[i], DBNull.Value))
                {
                    remappedValues[i] = null;
                }
            }

            return new ValueBuffer(remappedValues);
        }
    }
}
